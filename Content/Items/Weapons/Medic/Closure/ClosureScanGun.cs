using System;
using System.Collections.Generic;
using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Content.NPCs.Friendly;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Medic.Closure
{
	// 可露希尔：工程部特制扫描枪。
	// 普通攻击发射自定义「能量镖」弹幕（ClosureScanShotProjectile，shader 程序化绘制）。
	// 三个技能均基于「部署费用」经济：开启后立即/逐渐产出部署费用球（DeploymentCost），
	// 技能结束、技力未满时玩家靠近即可把它们吸回技力条。
	// S1 递归策略：技力满自动释放（不可手动开启），持续期间逐渐产出 3 点部署费用
	//            （每使用一次技能 +1，最多 7 点）。（援军护盾部分待援军系统接入）
	// S2 模型扩展：立即 10 点 + 持续期间逐渐 15 点部署费用；开启期间攻击提速到 125%，攻击方式同普攻。
	//            （援军防御/阻挡/战术点返还部分待相关系统接入）
	// S3 Q.E.D.：持续期间逐渐 18 点部署费用；攻击方式变为锁定范围内敌人发射激光（250% 物伤 + 迟钝），
	//            攻击频率更快；每攻击 9 次目标数 +1（最多 +6）；开启期间每秒播放待机音效，
	//            每次特殊攻击统一只播一次攻击音效；攻击时显示手持武器贴图但不做枪口上抬后坐力。
	public class ClosureScanGun : ExpansionWeaponBase
	{
		// 白板（未精英化）伤害 120，精英化阶段按原比例抬升
		protected override int[] EliteDamage => [120, 145, 174];

		private const int UseTimeNormal   = 28;
		private const int UseTimeWindup   = 46; // 仅首发使用：给自定义特效资源留出加载时间
		private const int UseTimeSkill2   = 22; // 二技能激活：攻速 +25%（28 / 1.25 ≈ 22）
		private const int UseTimeSkill3   = 14; // 三技能激活时的攻击间隔：比普通攻击快一倍
		private const int RecoilTicks     = 10; // 每发命中后枪身上抬-复位的动画时长
		private const float RecoilAngle   = 0.09f; // 上抬的最大弧度（约 5°），幅度更小一些
		private const float Skill3Range   = 850f; // 三技能自动锁定目标的范围（像素）

		private const float Skill3DamageMul     = 2.5f; // 三技能激光：可露希尔 250% 攻击力
		private const int   Skill3BaseTargets   = 3;    // 三技能基础攻击目标数
		private const int   Skill3MaxTargetBonus = 6;   // 三技能额外目标上限（+6）
		private const int   Skill3AttacksPerBonus = 9;  // 每攻击 9 次，攻击目标数 +1

		private static SoundStyle SkillActiveSfx;
		private static SoundStyle ShotSfx;
		private static SoundStyle Skill3ShotSfx; // 三技能特殊攻击音效（每次攻击统一只播一次）
		private static SoundStyle Skill3IdleSfx; // 三技能开启期间每秒一次的待机音效

		// 三技能待机音效计时：开启期间每 60 帧播放一次
		private int _skill3IdleTimer;

		// 技能状态机 & 部署费用发放
		private bool  _skillWasActive;     // 上一帧技能是否开启（用于检测开启瞬间）
		private int   _skillUseCount;      // 已使用过的技能次数（一技能部署费用递增用）
		private int   _gradualTotal;       // 本次技能应逐渐发放的部署费用总点数
		private int   _gradualGranted;     // 本次技能已发放的点数
		private int   _gradualDurationTicks;// 本次技能发放窗口（= 持续时间）
		private float _gradualAccum;       // 逐帧累积的小数部分
		private int   _s3AttackCount;      // 三技能攻击计数（每 9 次 +1 目标）
		private int   _s3TargetBonus;      // 三技能当前额外目标数

		// 首发前摇标记：椭圆枪口特效（BasicEffect 等 GPU 资源）只在第一次绘制时才会真正创建，
		// 这个过程本身很快，但和"射击手感"耦合在一起容易让首发显得卡顿；所以给首发单独加一点
		// 前摇掩盖过去，后续弹药资源已经加载完毕，可以正常连发，不需要每发都等。
		private static bool _hasFiredBefore;

		// 后坐力独立计时：每次 Shoot() 时手动置为 RecoilTicks，之后每帧自减到 0。
		// 注意：实际叠加 itemRotation 的地方在 WeaponPlayer.PostUpdate()（见 ApplyRecoilKick），
		// 不能放在本类的 HoldItem 里——原版 ItemCheck 在调用完 ModItem.HoldItem 之后，
		// 还会执行 ItemCheck_ApplyUseStyle 重新计算持枪瞄准角度，会把 HoldItem 里设置的
		// itemRotation 直接覆盖掉，导致"抬起动作完全不生效"。只有在这之后的 PostUpdate
		// 阶段修改 itemRotation 才不会被原版逻辑冲掉。
		private int _recoilTimer;

		// 每次开火前先销毁上一发还没消失的枪口闪光，保证每一发都是从头重新出现的独立闪光。
		private int _flashProjIndex = -1;

		public override void Load() {
			SkillActiveSfx = new SoundStyle("ArknightsMod/Sounds/SkillActive1") { Volume = 0.5f, MaxInstances = 2 };
			ShotSfx = new SoundStyle("ArknightsMod/Sounds/ClosureScanGunShot") { Volume = 0.6f, MaxInstances = 6, PitchVariance = 0.1f };
			// 三技能攻击音效：与普攻音效同风格，音量略高一点
			Skill3ShotSfx = new SoundStyle("ArknightsMod/Sounds/ClosureSkill3Shot") { Volume = 0.8f, MaxInstances = 4, PitchVariance = 0.08f };
			// 三技能待机音效：同风格，音量略低于两个攻击音效
			Skill3IdleSfx = new SoundStyle("ArknightsMod/Sounds/ClosureSkill3Idle") { Volume = 0.55f, MaxInstances = 2 };
		}

		public override void SetDefaults() {
			Item.damage = EliteDamage[0];
			Item.DamageType = DamageClass.Ranged;
			Item.width = 44;
			Item.height = 26;
			Item.useTime = UseTimeNormal;
			Item.useAnimation = UseTimeNormal;
			Item.knockBack = 3f;
			Item.value = Item.sellPrice(silver: 35);
			Item.rare = ItemRarityID.Green;
			Item.autoReuse = true;
			Item.noMelee = true;
			// 工程部特制扫描枪是自供能装置，不消耗真实弹药；shoot 直接指向自定义能量镖弹幕，
			// 避免因装填的子弹种类（如水晶/伊科尔子弹等自带特殊 OnHit 效果的弹药）残留任何原版行为。
			Item.shoot = ModContent.ProjectileType<Projectiles.Medic.Closure.ClosureScanShotProjectile>();
			Item.useAmmo = AmmoID.None;
			Item.crit = 4;
			Item.shootSpeed = 14f;
			Item.useStyle = ItemUseStyleID.Shoot;
		}

		public override bool AltFunctionUse(Player player) => false;

		public override bool CanUseItem(Player player) {
			var mp = player.GetModPlayer<WeaponPlayer>();

			// 右键：手动激活技能。一技能（下标 0）为自动释放技能，禁止手动开启；仅二/三技能可手动激活。
			if (ArknightsKeybinds.SkillActivatePressed(player)) {
				if (mp.Skill != 0 && mp.StockCount > 0 && !mp.SkillActive) {
					mp.SkillActive = true;
					mp.SkillTimer = 0;
					mp.DelStockCount();
					SoundEngine.PlaySound(SkillActiveSfx, player.Center);
				}
				return false;
			}

			// 首发前摇，仅触发一次；此后正常连发。二技能开启期间攻速 +25%、三技能开启期间攻速更快。
			if (mp.Skill == 2 && mp.SkillActive) {
				Item.useTime = UseTimeSkill3;
				Item.useAnimation = UseTimeSkill3;
			}
			else if (mp.Skill == 1 && mp.SkillActive) {
				Item.useTime = UseTimeSkill2;
				Item.useAnimation = UseTimeSkill2;
			}
			else if (!_hasFiredBefore) {
				Item.useTime = UseTimeWindup;
				Item.useAnimation = UseTimeWindup;
				_hasFiredBefore = true;
			}
			else {
				Item.useTime = UseTimeNormal;
				Item.useAnimation = UseTimeNormal;
			}

			mp.OffensiveRecovery();
			return base.CanUseItem(player);
		}

		// 每帧倒计时；真正叠加 itemRotation 的逻辑在 ApplyRecoilKick（由 WeaponPlayer.PostUpdate 调用）
		public override void HoldItem(Player player) {
			base.HoldItem(player);
			if (_recoilTimer > 0)
				_recoilTimer--;

			var mp = player.GetModPlayer<WeaponPlayer>();
			bool isLocal = player.whoAmI == Main.myPlayer;

			// 跟随宠物「指挥中心」：手持时若不存在则生成（暂无具体功能）
			if (isLocal)
				Projectiles.Medic.Closure.CommandCenterPet.EnsureFor(player);

			// 一技能：自动释放（技力满则自动开启，不可手动控制）
			if (isLocal && mp.Skill == 0 && mp.CurrentSkill != null && mp.CurrentSkill.AutoTrigger
			    && mp.StockCount > 0 && !mp.SkillActive) {
				mp.SkillActive = true;
				mp.SkillTimer = 0;
				mp.DelStockCount();
				SoundEngine.PlaySound(SkillActiveSfx, player.Center);
			}

			// 技能开启瞬间 / 持续期间：产出部署费用（仅本地玩家驱动）
			if (isLocal) {
				bool active = mp.SkillActive;
				if (active && !_skillWasActive)
					OnSkillActivated(player, mp);
				if (active)
					UpdateGradualDeploymentCost(player, mp);
				_skillWasActive = active;
			}

			// 三技能开启期间：每 1 秒播放一次待机音效（仅本地玩家，避免联机重复/嘈杂）
			if (isLocal && mp.Skill == 2 && mp.SkillActive) {
				if (_skill3IdleTimer <= 0) {
					SoundEngine.PlaySound(Skill3IdleSfx, player.Center);
					_skill3IdleTimer = 60;
				}
				_skill3IdleTimer--;
			}
			else {
				_skill3IdleTimer = 0;
			}
		}

		// 技能开启的一瞬：发放立即部署费用、设置逐渐发放的总量与窗口，并重置三技能攻击计数。
		private void OnSkillActivated(Player player, WeaponPlayer mp) {
			int durTicks = mp.CurrentSkill != null ? (int)(mp.CurrentSkill.CurrentLevelData.ActiveTime * 60f) : 0;
			_gradualDurationTicks = Math.Max(1, durTicks);
			_gradualGranted = 0;
			_gradualAccum = 0f;
			var src = player.GetSource_Misc("ClosureDeploymentCost");

			switch (mp.Skill) {
				case 0: // 一技能：逐渐 3 点（每使用一次技能 +1，最多 7）
					_gradualTotal = Math.Min(3 + _skillUseCount, 7);
					// 护盾可视化（蓝色气泡）：套在玩家本人 + 其它玩家 + 自身召唤物上，持续到技能结束。
					Projectiles.Medic.Closure.ClosureShieldProjectile.EnsureFor(player);
					// TODO(援军系统): 护盾的 12 防御数值效果待援军防御/护盾系统接入（此处仅先做可视化）。
					break;
				case 1: // 二技能：立即 10 点 + 逐渐 15 点
					DeploymentCost.SpawnAmount(src, player.Center, 10);
					_gradualTotal = 15;
					// 方形特效 + 焰气飘逸（二/三技能共用）
					Projectiles.Medic.Closure.ClosureSkillSquareEffect.EnsureFor(player);
					// TODO(援军系统): 援军防御 +60%、阻挡数 +1；战术点范围内部署返还 40% 部署费用。
					break;
				case 2: // 三技能：逐渐 18 点
					_gradualTotal = 18;
					_s3AttackCount = 0;
					_s3TargetBonus = 0;
					// 方形特效 + 焰气飘逸（二/三技能共用）
					Projectiles.Medic.Closure.ClosureSkillSquareEffect.EnsureFor(player);
					break;
				default:
					_gradualTotal = 0;
					break;
			}

			_skillUseCount++;
		}

		// 技能持续期间：把 _gradualTotal 点部署费用平摊到持续时间内逐个产出。
		private void UpdateGradualDeploymentCost(Player player, WeaponPlayer mp) {
			if (_gradualTotal <= 0 || _gradualGranted >= _gradualTotal)
				return;

			_gradualAccum += _gradualTotal / (float)_gradualDurationTicks;
			var src = player.GetSource_Misc("ClosureDeploymentCost");
			while (_gradualAccum >= 1f && _gradualGranted < _gradualTotal) {
				_gradualAccum -= 1f;
				_gradualGranted++;
				DeploymentCost.SpawnOrb(src, player.Center + Main.rand.NextVector2Circular(20f, 12f), 1);
			}
		}

		// 枪身贴图小幅度上抬再复位的后坐力动画。必须在原版 ItemCheck_ApplyUseStyle 之后调用
		// （见 WeaponPlayer.PostUpdate），否则会被原版持枪瞄准逻辑覆盖掉。
		public void ApplyRecoilKick(Player player) {
			if (_recoilTimer <= 0)
				return;

			float t = 1f - _recoilTimer / (float)RecoilTicks; // 0→1，随 timer 递减而增大
			float kick = System.MathF.Sin(t * MathHelper.Pi) * RecoilAngle; // 0→峰值→0
			player.itemRotation += kick * -player.direction;
		}

		// 无论装填何种子弹，普通攻击一律发射自定义能量镖弹幕（直线飞行、无重力）；
		// 三技能激活时攻击行为被替代：自动锁定范围内所有可攻击目标，持续发射激光攻击。
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
				Vector2 position, Vector2 velocity,
				int type, int damage, float knockback) {
			WeaponPlayer wp = player.GetModPlayer<WeaponPlayer>();
			bool skill3Active = wp.Skill == 2 && wp.SkillActive; // 技能下标 2 = 三技能

			if (skill3Active) {
				// ── 三技能模式：锁定范围内所有可攻击目标，通过标记弹幕发射激光 ──
				// 攻击音效每次攻击（无论命中几个目标）统一只播一次；且不做枪口上抬后坐力动画。
				SoundEngine.PlaySound(Skill3ShotSfx, position);

				if (player.whoAmI != Main.myPlayer)
					return false;

				// 三技能激光造成可露希尔 250% 攻击力的物理伤害
				int s3Damage = (int)(damage * Skill3DamageMul);

				// 搜索范围内所有可攻击目标
				var targets = new List<int>();
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					if (npc.active && npc.CanBeChasedBy() && !npc.friendly
					    && Vector2.Distance(player.Center, npc.Center) <= Skill3Range) {
						targets.Add(i);
					}
				}

				if (targets.Count == 0)
					return false;

				// 攻击目标数上限随攻击次数增长：基础 Skill3BaseTargets，每攻击 9 次 +1（最多 +6）。
				// 按距离取最近的若干个目标。
				_s3AttackCount++;
				if (_s3AttackCount % Skill3AttacksPerBonus == 0 && _s3TargetBonus < Skill3MaxTargetBonus)
					_s3TargetBonus++;
				int maxTargets = Skill3BaseTargets + _s3TargetBonus;
				if (targets.Count > maxTargets) {
					targets.Sort((a, b) => Vector2.DistanceSquared(player.Center, Main.npc[a].Center)
						.CompareTo(Vector2.DistanceSquared(player.Center, Main.npc[b].Center)));
					targets.RemoveRange(maxTargets, targets.Count - maxTargets);
				}

				// 为每个目标生成或续命标记，并触发 Volley（发射激光）
				foreach (int idx in targets) {
					// 检查是否已经有标记存在
					bool found = false;
					for (int j = 0; j < Main.maxProjectiles; j++) {
						Projectile p = Main.projectile[j];
						if (p.active && p.owner == player.whoAmI
						    && p.ModProjectile is Projectiles.Medic.Closure.ClosureTargetMarkProjectile mark
						    && (int)p.ai[0] == idx) {
							mark.Volley(s3Damage, knockback);
							found = true;
							break;
						}
					}

					// 没有标记则新建
					if (!found) {
						int markIdx = Projectile.NewProjectile(source, Main.npc[idx].Center, Vector2.Zero,
							ModContent.ProjectileType<Projectiles.Medic.Closure.ClosureTargetMarkProjectile>(),
							0, 0f, player.whoAmI, idx);
						if (markIdx >= 0 && markIdx < Main.maxProjectiles) {
							var mark = Main.projectile[markIdx].ModProjectile as Projectiles.Medic.Closure.ClosureTargetMarkProjectile;
							mark?.Volley(s3Damage, knockback);
						}
					}
				}

				return false;
			}

			// ── 普通攻击模式：发射能量镖 ──
			SoundEngine.PlaySound(ShotSfx, position);
			_recoilTimer = RecoilTicks;
			// 枪口闪光：白→双色渐变的椭圆，面朝发射方向，弹幕从其中飞出。
			// 沿发射方向额外前移一段距离，避免被玩家角色/武器贴图挡住看不见。
			// 每发都重新生成一个新的（先销毁上一发还没消失的），不再跨发持续存在。
			if (player.whoAmI == Main.myPlayer) {
				if (_flashProjIndex >= 0 && _flashProjIndex < Main.maxProjectiles) {
					Projectile old = Main.projectile[_flashProjIndex];
					if (old.active && old.owner == player.whoAmI
					    && old.type == ModContent.ProjectileType<Projectiles.Medic.Closure.ClosureMuzzleFlashProjectile>()) {
						old.Kill();
					}
				}
				Vector2 flashPos = position + velocity.SafeNormalize(Vector2.UnitX) * 48f;
				_flashProjIndex = Projectile.NewProjectile(source, flashPos, velocity,
					ModContent.ProjectileType<Projectiles.Medic.Closure.ClosureMuzzleFlashProjectile>(),
					0, 0f, player.whoAmI);
			}
			Projectile.NewProjectile(source, position, velocity,
				ModContent.ProjectileType<Projectiles.Medic.Closure.ClosureScanShotProjectile>(),
				damage, knockback, player.whoAmI);
			return false;
		}
	}
}
