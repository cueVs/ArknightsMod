using System;
using ArknightsMod.Content.Buffs;
using System.Collections.Generic;
using ArknightsMod.Common.Particle;
using ArknightsMod.Common.VisualEffects;
using ArknightsMod.Content.Items.Weapons.Guard.Hellagur;
using ArknightsMod.Content.Projectiles.BasePROJ;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Hellagur
{
	/// <summary>赫拉格大太刀的母刀弹幕。ai[0] 是动作，ai[1] 是 0/1/2/3 技能模式，ai[2] 锁存低血攻速。</summary>
	public class HellagurOdachiSwing : BaseSwordHP
	{
		private const int ActionSlash = 0;
		private const int ActionReverse = 1;
		private const int ActionHeavy = 2;
		public const int ActionSkill1 = 3;
		public const int ActionSkill3 = 4;

		private const int EventWindup = 1;
		private const int EventFirstSlash = 2;
		private const int EventSecondSlash = 3;
		private const int EventBloodMoon = 4;

		// 第 3 张单帧 64×64，旋转后的握点 (11,51)、刀尖 (59,3)。
		private static readonly float SpriteGripToTip = new Vector2(48f, -48f).Length();
		private static readonly WeaponSpriteProfile OdachiProfile = new(
			grip: new Vector2(11f / 64f, 51f / 64f),
			tip: new Vector2(59f / 64f, 3f / 64f),
			nativeForwardAngle: -MathHelper.PiOver4,
			combatReach: 92f,
			combatWidth: 29f,
			drawScale: 92f / SpriteGripToTip);

		private static readonly WeaponSpriteProfile FullMoonProfile = new(
			grip: new Vector2(11f / 64f, 51f / 64f),
			tip: new Vector2(59f / 64f, 3f / 64f),
			nativeForwardAngle: -MathHelper.PiOver4,
			combatReach: 124f,
			combatWidth: 35f,
			drawScale: 124f / SpriteGripToTip);

		private readonly HashSet<int> struckTargets = [];
		private const int VisualTrailTicks = 18;
		private const int TrailSamplesPerUpdate = 2;
		private const int MotionUpdatesPerTick = 3;
		private const int VisualTrailSamples = VisualTrailTicks * TrailSamplesPerUpdate * MotionUpdatesPerTick;
		private const float TrailSampleSpacing = 1.35f;
		private const int MaxCurveStepsPerTick = 40;
		private readonly float[] motionAngleTrail = new float[VisualTrailSamples];
		private readonly Vector2[] motionHandTrail = new Vector2[VisualTrailSamples];
		private int motionTrailCount;
		private float previousVisualAngle;
		private float unwrappedVisualAngle;
		private Vector2 previousVisualHand;
		private int visualSweepDirection = 1;
		private float momentum;
		private bool visualAngleReady;
		private int healedThisSwing;
		private bool creditedAttackRecovery;
		private static Asset<Effect> slashTrailEffect;

		private const string SlashTrailEffectPath =
			"ArknightsMod/Content/Projectiles/Guard/Hellagur/HellagurArtAttackTrail";
		private const string SlashTrailMaskPath =
			"ArknightsMod/Content/Projectiles/Guard/Lupine/LupineTrailShape";
		private const string BladeGlowPath =
			"ArknightsMod/Content/Projectiles/Guard/Hellagur/HellagurBladeGlow";
		private const string SlashBodyPath =
			"ArknightsMod/Content/Projectiles/Guard/Hellagur/HellagurSlashBody";
		private const string SlashEdgePath =
			"ArknightsMod/Content/Projectiles/Guard/Hellagur/HellagurSlashEdge";
		private float attachedBladeLight;

		public override void Load() {
			if (!Main.dedServ)
				slashTrailEffect = ModContent.Request<Effect>(SlashTrailEffectPath, AssetRequestMode.ImmediateLoad);
		}

		public override void Unload() => slashTrailEffect = null;

		public override string Texture => HellagurOdachi.WeaponTexturePath;

		protected override WeaponSpriteProfile Profile => SkillMode == HellagurSkillBridge.Skill3
			? FullMoonProfile
			: OdachiProfile;

		protected override int BindingItemType() => ModContent.ItemType<HellagurOdachi>();

		protected override Texture2D GetWeaponTexture() =>
			Main.dedServ ? null : TextureAssets.Item[ModContent.ItemType<HellagurOdachi>()].Value;

		public override void SetDefaults() {
			base.SetDefaults();
			// 每帧进行 3 次真实姿态与碰撞更新。BaseSwordHP 用浮点时间保持总时长不变，
			// 低血量高速挥刀不再只靠绘制端插值来伪装平滑。
			Projectile.extraUpdates = MotionUpdatesPerTick - 1;
		}

		// ai[2] 在生成时锁存生命值提供的攻速，避免联机端因生命同步延迟得到不同动作长度。
		protected override float ActionSpeedScale {
			get {
				float lowHealthScale = Projectile.ai[2] > 0f ? Projectile.ai[2] : 1f;
				return lowHealthScale;
			}
		}

		private static MeleeNode Aim(int duration, float from, float to) => new() {
			Kind = MeleeNodeKind.Aim,
			Duration = duration,
			A0 = from,
			A1 = to,
			Ease = MeleeEase.Smooth,
			LockAim = true,
			Hit = MeleeHitWindow.None
		};

		private static MeleeNode Arc(int duration, float from, float to, MeleeHitWindow hit,
			int eventId = 0, float eventProgress = 0.5f, float reach = 0f) => new() {
			Kind = MeleeNodeKind.Arc,
			Duration = duration,
			A0 = from,
			A1 = to,
			R0 = reach,
			R1 = reach,
			Ease = MeleeEase.Heavy,
			Hit = hit,
			EventId = eventId,
			EventProgress = eventProgress
		};

		// Pause 也显式保留蓄力角，防止基类按默认 A0/A1=0 把刀瞬间弹回瞄准线。
		private static MeleeNode Pause(int duration, float holdAngle, int eventId = 0) => new() {
			Kind = MeleeNodeKind.Pause,
			Duration = duration,
			A0 = holdAngle,
			A1 = holdAngle,
			Ease = MeleeEase.Linear,
			Hit = MeleeHitWindow.None,
			EventId = eventId,
			EventProgress = 0f
		};

		protected override MeleeAction ResolveAction(int actionId) {
			if (SkillMode == HellagurSkillBridge.Skill1)
				return ResolveSkill1();
			if (SkillMode == HellagurSkillBridge.Skill2)
				return ResolveSkill2(actionId);
			if (SkillMode == HellagurSkillBridge.Skill3)
				return ResolveSkill3();

			return actionId switch {
				ActionReverse => new MeleeAction {
					Id = ActionReverse,
					NextCombo = ActionHeavy,
					Nodes = [
						Aim(4, 0f, 2.04f),
						Arc(12, 2.04f, -2.22f, MeleeHitWindow.Window(0.1f, 0.84f, 1f, 1.08f), EventFirstSlash, 0.42f, 8f),
						Pause(4, -2.22f)
					]
				},
				ActionHeavy => new MeleeAction {
					Id = ActionHeavy,
					NextCombo = ActionSlash,
					Nodes = [
						Aim(4, 0f, -2.28f),
						Pause(2, -2.28f, EventWindup),
						Arc(14, -2.28f, 2.52f, MeleeHitWindow.Window(0.08f, 0.88f, 1.25f, 1.28f), EventFirstSlash, 0.44f, 14f),
						Pause(6, 2.52f)
					]
				},
				_ => new MeleeAction {
					Id = ActionSlash,
					NextCombo = ActionReverse,
					Nodes = [
						Aim(4, 0f, -2.04f),
						Arc(12, -2.04f, 2.22f, MeleeHitWindow.Window(0.1f, 0.84f, 1f, 1.08f), EventFirstSlash, 0.42f, 8f),
						Pause(4, 2.22f)
					]
				}
			};
		}

		private static MeleeAction ResolveSkill1() => new() {
			Id = ActionSkill1,
			NextCombo = -1,
			Nodes = [
				Aim(2, 0f, -2.08f),
				Arc(6, -2.08f, 2.18f, MeleeHitWindow.Window(0.08f, 0.86f, 1f, 1.16f), EventFirstSlash, 0.4f, 10f),
				Pause(2, 2.18f),
				Arc(6, 2.18f, -2.22f, MeleeHitWindow.Window(0.06f, 0.88f, 1f, 1.12f), EventSecondSlash, 0.38f, 8f),
				Pause(4, -2.22f)
			]
		};

		private static MeleeAction ResolveSkill2(int actionId) {
			bool reverse = (actionId & 1) != 0;
			float start = reverse ? 2.08f : -2.08f;
			float middle = reverse ? -2.16f : 2.16f;
			float finish = reverse ? 2.2f : -2.2f;
			return new MeleeAction {
				Id = actionId,
				NextCombo = reverse ? ActionSlash : ActionReverse,
				Nodes = [
					Aim(2, 0f, start),
					Arc(6, start, middle, MeleeHitWindow.Window(0.06f, 0.88f, 1f, 1.12f), EventFirstSlash, 0.38f, 8f),
					Pause(2, middle),
					Arc(6, middle, finish, MeleeHitWindow.Window(0.05f, 0.9f, 1f, 1.1f), EventSecondSlash, 0.36f, 8f),
					Pause(4, finish)
				]
			};
		}

		private static MeleeAction ResolveSkill3() => new() {
			Id = ActionSkill3,
			NextCombo = -1,
			Nodes = [
				Aim(2, 0f, -2.42f),
				Pause(2, -2.42f, EventWindup),
				Arc(12, -2.42f, 2.68f, MeleeHitWindow.Window(0.06f, 0.92f, 1f, 1.42f), EventBloodMoon, 0.44f, 18f),
				Pause(4, 2.68f)
			]
		};

		public override void AI() {
			base.AI();
			if (!Projectile.active)
				return;

			if (!visualAngleReady) {
				previousVisualAngle = CurrentAngle;
				unwrappedVisualAngle = CurrentAngle;
				previousVisualHand = HandWorld;
				visualAngleReady = true;
			}

			float angleDelta = MathHelper.WrapAngle(CurrentAngle - previousVisualAngle);
			if (MathF.Abs(angleDelta) > 0.003f)
				visualSweepDirection = angleDelta < 0f ? -1 : 1;
			// extraUpdates 将单次角度变化拆小；动量计算换回每游戏帧等效速度，
			// 避免提高采样密度反而把刀光压暗。
			float updateScale = Projectile.extraUpdates + 1f;
			float angularSpeed = MathF.Abs(angleDelta) * updateScale;
			float targetMomentum = MathHelper.Clamp((angularSpeed - 0.025f) / 0.32f, 0f, 1f);
			float riseResponse = 1f - MathF.Pow(1f - 0.72f, 1f / updateScale);
			float fallResponse = 1f - MathF.Pow(1f - 0.2f, 1f / updateScale);
			momentum = targetMomentum > momentum
				? MathHelper.Lerp(momentum, targetMomentum, riseResponse)
				: MathHelper.Lerp(momentum, 0f, fallResponse);
			// Earth 的可取之处是刀光贴着武器逐渐点亮，而不是凭空悬浮一张大图。
			// 这里只复用该表现思路：亮度跟随本武器自己的挥刀动量，不改变动作或判定。
			float targetBladeLight = Damaging
				? MathHelper.Clamp(0.12f + momentum * 1.08f, 0f, 1f)
				: 0f;
			float bladeResponse = targetBladeLight > attachedBladeLight
				? 1f - MathF.Pow(1f - 0.24f, 1f / updateScale)
				: 1f - MathF.Pow(1f - 0.18f, 1f / updateScale);
			attachedBladeLight = MathHelper.Lerp(attachedBladeLight, targetBladeLight, bladeResponse);
			unwrappedVisualAngle += angleDelta;

			if (Damaging) {
				// 每个真实运动子步再补写两个中间姿态：一个游戏帧共 3 次运动更新、6 个绘制样本。
				// 这同时加密了实际碰撞和可视轨迹，低血提速时也不会出现大块折线或矩形断面。
				float startAngle = unwrappedVisualAngle - angleDelta;
				Vector2 currentHand = HandWorld;
				for (int sample = 1; sample <= TrailSamplesPerUpdate; sample++) {
					for (int i = motionAngleTrail.Length - 1; i > 0; i--) {
						motionAngleTrail[i] = motionAngleTrail[i - 1];
						motionHandTrail[i] = motionHandTrail[i - 1];
					}

					float completion = sample / (float)TrailSamplesPerUpdate;
					motionAngleTrail[0] = MathHelper.Lerp(startAngle, unwrappedVisualAngle, completion);
					motionHandTrail[0] = Vector2.Lerp(previousVisualHand, currentHand, completion);
					motionTrailCount = Math.Min(motionTrailCount + 1, motionAngleTrail.Length);
				}
			}
			else if (motionTrailCount > 0) {
				motionTrailCount = Math.Max(0, motionTrailCount - TrailSamplesPerUpdate);
			}
			previousVisualAngle = CurrentAngle;
			previousVisualHand = HandWorld;
		}

		public override bool? CanHitNPC(NPC target) {
			// 武者普攻/S1/S2均为单目标；S1/S2的第二段仍可再次命中同一目标。
			// 只有「满月」把同时攻击数提升到3。
			int targetCap = SkillMode == HellagurSkillBridge.Skill3 ? 3 : 1;
			if (struckTargets.Count >= targetCap && !struckTargets.Contains(target.whoAmI)) {
				return false;
			}
			return null;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (!Damaging)
				return false;

			// 保留基类当前刀身判定；低血高速挥舞时再检查本tick内四个中间姿态，
			// 防止刀从敌人一侧跨到另一侧、两帧端点都没碰到而漏判。
			if (base.Colliding(projHitbox, targetHitbox) == true)
				return true;

			float reach = SkillMode == HellagurSkillBridge.Skill3 ? 142f : 112f;
			float width = SkillMode == HellagurSkillBridge.Skill3 ? 38f : 32f;
			int poseCount = Math.Min(motionTrailCount, TrailSamplesPerUpdate + 1);
			bool hasLineOfSight = Collision.CanHit(Owner.MountedCenter, 1, 1,
				targetHitbox.TopLeft(), targetHitbox.Width, targetHitbox.Height);
			if (!hasLineOfSight)
				return false;

			for (int i = 0; i < poseCount; i++) {
				Vector2 direction = BaseHeldMeleeSupport.Dir(motionAngleTrail[i]);
				Vector2 hand = motionHandTrail[i] - direction * 6f;
				Vector2 tip = motionHandTrail[i] + direction * reach;
				if (BaseHeldMeleeSupport.CapsuleHits(hand, tip, width, targetHitbox))
					return true;
			}

			// 除了离散刀身，再把相邻姿态上四个半径位置连起来，覆盖整片扇形扫掠面。
			// 这样扩大的是刀真实经过的区域，而不是角色周围无差别的大矩形。
			if (poseCount >= 2) {
				Vector2 newestDirection = BaseHeldMeleeSupport.Dir(motionAngleTrail[0]);
				Vector2 oldestDirection = BaseHeldMeleeSupport.Dir(motionAngleTrail[poseCount - 1]);
				for (int radialStep = 1; radialStep <= 4; radialStep++) {
					float radiusFactor = radialStep / 4f;
					Vector2 newest = motionHandTrail[0] + newestDirection * reach * radiusFactor;
					Vector2 oldest = motionHandTrail[poseCount - 1] + oldestDirection * reach * radiusFactor;
					if (BaseHeldMeleeSupport.CapsuleHits(oldest, newest, width * 0.82f, targetHitbox))
						return true;
				}
			}

			return false;
		}

		protected override void OnActionEvent(int eventId, Vector2 tipWorld) {
			if (eventId == EventWindup) {
				SpawnWindupVfx(tipWorld);
				return;
			}

			if (eventId is EventFirstSlash or EventSecondSlash) {
				SpawnSlashVfx(tipWorld, eventId == EventSecondSlash);
				return;
			}

			if (eventId != EventBloodMoon)
				return;

			SpawnBloodMoonVfx();
			if (SkillMode != HellagurSkillBridge.Skill3 || Projectile.owner != Main.myPlayer)
				return;

			Vector2 direction = BaseHeldMeleeSupport.Dir(AimAngle);
			// 刀气始终从瞄准轴线上出发，不再使用弧顶 TipWorld，避免向斜上/斜下偏移。
			Vector2 origin = Owner.MountedCenter + direction * 20f;
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), origin, direction * 10.5f,
				ModContent.ProjectileType<HellagurBloodSlash>(), 0, 0f,
				Projectile.owner, LowHealthIntensity);
		}

		protected override void OnStrike(NPC target, in NPC.HitInfo hit, int damageDone) {
			if (damageDone <= 0 || target.friendly || target.lifeMax <= 5)
				return;
			target.AddBuff(ModContent.BuffType<WeaponBleedingDebuff>(), 180);

			struckTargets.Add(target.whoAmI);

			SpawnHitVfx(target, SkillMode != HellagurSkillBridge.Normal || ActionId == ActionHeavy,
				BaseHeldMeleeSupport.Dir(CurrentAngle));

			if (Projectile.owner != Main.myPlayer)
				return;

			HellagurCombatPlayer combatPlayer = Owner.GetModPlayer<HellagurCombatPlayer>();
			// 自动二连本身不回技力；完成后仍需两次新的母刀有效命中才能再次就绪。
			combatPlayer.RegisterMotherBladeHit(SkillMode != HellagurSkillBridge.Skill1 && !creditedAttackRecovery);
			creditedAttackRecovery = true;
			int healed = combatPlayer.HealFromMotherBlade(ref healedThisSwing);
			if (healed > 0)
				SpawnHealingVfx(target.Center);
		}

		private void SpawnWindupVfx(Vector2 tipWorld) {
			if (Main.dedServ)
				return;

			SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.28f, Pitch = -0.48f }, HandWorld);
			int count = SkillMode == HellagurSkillBridge.Skill3 ? 18 : 10;
			for (int i = 0; i < count; i++) {
				Vector2 position = Vector2.Lerp(HandWorld, tipWorld, Main.rand.NextFloat());
				Dust dust = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(8f, 8f),
					i % 4 == 0 ? DustID.GoldFlame : DustID.Blood,
					Main.rand.NextVector2Circular(1.2f, 1.2f), 40, new Color(246, 74, 48), Main.rand.NextFloat(0.75f, 1.2f));
				dust.noGravity = true;
			}
		}

		private void SpawnSlashVfx(Vector2 tipWorld, bool secondSlash) {
			if (Main.dedServ)
				return;

			SoundEngine.PlaySound(SoundID.Item1 with {
				Volume = secondSlash ? 0.48f : 0.38f,
				Pitch = secondSlash ? 0.08f : -0.12f
			}, tipWorld);
			int count = SkillMode == HellagurSkillBridge.Normal ? 7 : 12;
			Vector2 tangent = BaseHeldMeleeSupport.Dir(CurrentAngle + (secondSlash ? -MathHelper.PiOver2 : MathHelper.PiOver2));
			for (int i = 0; i < count; i++) {
				Dust dust = Dust.NewDustPerfect(tipWorld + Main.rand.NextVector2Circular(7f, 7f),
					i % 4 == 0 ? DustID.GoldFlame : DustID.Blood,
					tangent * Main.rand.NextFloat(1.8f, 4.8f) + Main.rand.NextVector2Circular(0.8f, 0.8f),
					35, i % 4 == 0 ? new Color(255, 184, 92) : new Color(222, 34, 58),
					Main.rand.NextFloat(0.8f, 1.3f));
				dust.noGravity = true;
			}

			// 借鉴 Briny 大刀挥过时“主体线条多、亮色点缀少、白芯最稀”的配比，
			// 但使用赫拉格自己的血月色板与本项目粒子，不引入参考武器的贴图或代码依赖。
			int streakCount = SkillMode == HellagurSkillBridge.Normal ? 6 : 9;
			for (int i = 0; i < streakCount; i++) {
				float roll = Main.rand.NextFloat();
				Color color = roll < 0.62f
					? new Color(190, 18, 43)
					: roll < 0.9f ? new Color(255, 157, 72) : new Color(255, 238, 192);
				Vector2 position = Vector2.Lerp(HandWorld, tipWorld, Main.rand.NextFloat(0.38f, 1f))
					+ Main.rand.NextVector2Circular(5f, 5f);
				var streak = new DefaultParticle(position,
					tangent * Main.rand.NextFloat(2.8f, 7.2f) + Main.rand.NextVector2Circular(0.45f, 0.45f),
					Main.rand.Next(13, 21), Main.rand.NextFloat(0.35f, 0.72f), color, true) {
					Deformation = new Vector2(0.3f, Main.rand.NextFloat(1.7f, 2.5f))
				};
				streak.Spawn();
			}
		}

		private void SpawnBloodMoonVfx() {
			if (Main.dedServ)
				return;

			SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.68f, Pitch = -0.42f }, Owner.Center);
			Vector2 direction = BaseHeldMeleeSupport.Dir(AimAngle);
			Vector2 center = Owner.MountedCenter + direction * 20f;
			for (int i = 0; i < 28; i++) {
				float angle = MathHelper.TwoPi * i / 28f;
				Vector2 radial = Vector2.UnitX.RotatedBy(angle);
				Dust dust = Dust.NewDustPerfect(center + radial * Main.rand.NextFloat(24f, 38f),
					i % 5 == 0 ? DustID.GoldFlame : DustID.Blood,
					radial * Main.rand.NextFloat(1.5f, 4.2f), 30,
					new Color(255, 84, 48), Main.rand.NextFloat(0.9f, 1.45f));
				dust.noGravity = true;
			}
		}

		// 红的狼群爆发复用同一批短粒子与 dust；此入口只生成视觉，不施加伤害或控制。
		internal static void SpawnHitParticles(Vector2 center, bool empowered, Vector2 slashDirection) {
			if (Main.dedServ || Main.gameMenu)
				return;

			slashDirection = slashDirection.SafeNormalize(Vector2.UnitX);
			Vector2 slashNormal = slashDirection.RotatedBy(MathHelper.PiOver2);
			int glowCount = empowered ? 14 : 9;
			for (int i = 0; i < glowCount; i++) {
				Color color = i % 4 == 0 ? new Color(255, 205, 96) : new Color(232, 42, 45);
				Vector2 burstVelocity = slashNormal * Main.rand.NextFloat(-6.5f, 6.5f)
					+ slashDirection * Main.rand.NextFloat(1.2f, empowered ? 5.5f : 4.2f);
				var glow = new DefaultParticle(center + Main.rand.NextVector2Circular(10f, 10f),
					burstVelocity, empowered ? 28 : 25, Main.rand.NextFloat(0.58f, 1.08f), color, true) {
					Deformation = new Vector2(0.46f, empowered ? 2.05f : 1.82f)
				};
				glow.Spawn();
			}
			for (int i = 0; i < (empowered ? 17 : 11); i++) {
				Dust dust = Dust.NewDustPerfect(center, i % 5 == 0 ? DustID.GoldFlame : DustID.Blood,
					slashNormal * Main.rand.NextFloat(-5.8f, 5.8f)
						+ slashDirection * Main.rand.NextFloat(0.8f, empowered ? 5.2f : 4.1f),
					20, new Color(246, 72, 45), Main.rand.NextFloat(0.98f, 1.58f));
				dust.noGravity = true;
			}

		}

		private static void SpawnHitVfx(NPC target, bool empowered, Vector2 slashDirection) {
			if (Main.dedServ)
				return;

			slashDirection = slashDirection.SafeNormalize(Vector2.UnitX);
			SpawnHitParticles(target.Center, empowered, slashDirection);

			SoundEngine.PlaySound(SoundID.NPCHit1 with {
				Volume = empowered ? 0.34f : 0.25f,
				Pitch = empowered ? -0.2f : -0.06f,
				MaxInstances = 5
			}, target.Center);
			if (Main.LocalPlayer.active
				&& Vector2.DistanceSquared(Main.LocalPlayer.Center, target.Center) < 900f * 900f) {
				ShakeEffectPlayer shake = Main.LocalPlayer.GetModPlayer<ShakeEffectPlayer>();
				shake.screenShakeTime = Math.Max(shake.screenShakeTime, empowered ? 4 : 2);
				shake.screenShakeVelocity = slashDirection * (empowered ? 2.2f : 1.25f);
			}
		}

		private void SpawnHealingVfx(Vector2 hitPosition) {
			if (Main.dedServ)
				return;

			Vector2 towardOwner = (Owner.Center - hitPosition).SafeNormalize(Vector2.UnitY);
			for (int i = 0; i < 6; i++) {
				Dust dust = Dust.NewDustPerfect(hitPosition + Main.rand.NextVector2Circular(8f, 8f),
					i % 3 == 0 ? DustID.GoldFlame : DustID.LifeDrain,
					towardOwner.RotatedByRandom(0.35f) * Main.rand.NextFloat(2.2f, 4.5f),
					45, new Color(255, 184, 100), Main.rand.NextFloat(0.8f, 1.2f));
				dust.noGravity = true;
			}
		}

		// 派生武器共用刀幕几何、着色器和强度，只替换颜色与专属月相显示。
		protected virtual Color BladeVfxColor(Color color) => color;
		protected virtual bool ShowMoonPhaseVfx => true;

		protected override void DrawWeaponVfx(Color lightColor) {
			if (Main.dedServ)
				return;

			SpriteBatch spriteBatch = Main.spriteBatch;
			Texture2D glow = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
			float lowHealth = LowHealthIntensity;
			Color crimson = BladeVfxColor(Color.Lerp(new Color(174, 18, 42), new Color(244, 28, 52), lowHealth));
			Color amber = BladeVfxColor(Color.Lerp(new Color(245, 137, 61), new Color(255, 210, 124), lowHealth));

			BaseHeldMeleeSupport.BeginAdditive(spriteBatch);
			DrawAttachedBladeLight(spriteBatch, crimson, amber, lowHealth);
			DrawLayeredBladeLight(spriteBatch, crimson, amber, lowHealth);

			if (TipTrailCount > 0) {
				BaseHeldMeleeSupport.DrawGlow(glow, TipTrail[0], crimson * 0.9f, 0.38f + lowHealth * 0.12f);
				BaseHeldMeleeSupport.DrawGlow(glow, TipTrail[0], amber * 0.9f, 0.14f + lowHealth * 0.05f);
			}

			if (lowHealth > 0f) {
				float pulse = 0.88f + 0.12f * MathF.Sin((float)Main.GlobalTimeWrappedHourly * 7f);
				BaseHeldMeleeSupport.DrawGlow(glow, HandWorld, crimson * (0.34f * lowHealth),
					(0.3f + 0.18f * lowHealth) * pulse);
			}

			if (ShowMoonPhaseVfx)
				DrawMoonPhase(spriteBatch, glow, crimson, amber);
			BaseHeldMeleeSupport.EndAdditive(spriteBatch);
		}

		private void DrawAttachedBladeLight(SpriteBatch spriteBatch, Color crimson, Color amber, float lowHealth) {
			if (attachedBladeLight <= 0.01f)
				return;

			Texture2D bladeGlow = ModContent.Request<Texture2D>(BladeGlowPath).Value;
			Vector2 direction = BaseHeldMeleeSupport.Dir(CurrentAngle);
			Vector2 normal = new(-direction.Y, direction.X);
			Vector2 position = HandWorld - Main.screenPosition;
			// 素材的白刃从 (5, 64) 向右延伸；以此为握点才能真正贴住当前刀轴。
			Vector2 origin = new(5f, 64f);
			float reachScale = SkillMode == HellagurSkillBridge.Skill3 ? 1.28f : 0.98f;
			float intensity = attachedBladeLight * (0.82f + lowHealth * 0.18f);
			float pulse = 0.94f + 0.06f * MathF.Sin((float)Main.GlobalTimeWrappedHourly * 10f
				+ Projectile.whoAmI * 0.41f);

			// 两道很窄的暗红外辉先托住刀身，中心再从血红过渡到暖白。
			for (int side = -1; side <= 1; side += 2) {
				spriteBatch.Draw(bladeGlow, position + normal * side * 1.6f, null,
					crimson * (intensity * 0.2f), CurrentAngle, origin,
					reachScale * 1.035f, SpriteEffects.None, 0f);
			}
			spriteBatch.Draw(bladeGlow, position, null,
				Color.Lerp(crimson, amber, 0.48f) * (intensity * 0.62f), CurrentAngle, origin,
				reachScale * pulse, SpriteEffects.None, 0f);
			spriteBatch.Draw(bladeGlow, position + direction * 1.5f, null,
				BladeVfxColor(new Color(255, 236, 206)) * (intensity * intensity * 0.42f), CurrentAngle, origin,
				reachScale * 0.985f, SpriteEffects.None, 0f);
		}

		private void DrawLayeredBladeLight(SpriteBatch spriteBatch, Color crimson, Color amber, float lowHealth) {
			if (!Damaging || momentum <= 0.025f || motionTrailCount < TrailSamplesPerUpdate)
				return;

			Texture2D softCrescent = ModContent.Request<Texture2D>(SlashBodyPath).Value;
			Texture2D sharpCrescent = ModContent.Request<Texture2D>(SlashEdgePath).Value;
			Texture2D flare = ModContent.Request<Texture2D>(
				"ArknightsMod/Content/Textures/flare_01").Value;
			float power = MathHelper.Clamp(momentum * 1.42f, 0f, 1f);
			float skillScale = SkillMode == HellagurSkillBridge.Skill3 ? 1.2f
				: SkillMode == HellagurSkillBridge.Normal ? 1f : 1.08f;
			float reach = SkillMode == HellagurSkillBridge.Skill3 ? 142f : 116f;
			SpriteEffects flip = visualSweepDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			// 当前刀锋是一张完整的柔和月牙，不依赖历史顶点拼出轮廓；下面的着色器刀幕
			// 负责重量与残影，这一层只负责 Fran 式的连续材质、热刃和细腻收口。
			DrawBladeLightPose(spriteBatch, softCrescent, sharpCrescent,
				HandWorld, CurrentAngle, reach, crimson, amber,
				power * (0.74f + lowHealth * 0.16f), skillScale, flip);

			int echoIndex = Math.Min(motionTrailCount - 1,
				TrailSamplesPerUpdate * MotionUpdatesPerTick * 2);
			if (echoIndex > 0) {
				DrawBladeLightPose(spriteBatch, softCrescent, sharpCrescent,
					motionHandTrail[echoIndex], motionAngleTrail[echoIndex], reach,
					BladeVfxColor(new Color(112, 5, 22)), crimson,
					power * 0.22f, skillScale * 0.97f, flip);
			}

			Vector2 direction = BaseHeldMeleeSupport.Dir(CurrentAngle);
			Vector2 tip = HandWorld + direction * reach;
			float flarePulse = 0.88f + 0.12f * MathF.Sin((float)Main.GlobalTimeWrappedHourly * 9f
				+ Projectile.whoAmI * 0.37f);
			spriteBatch.Draw(flare, tip - Main.screenPosition, null,
				Color.Lerp(crimson, amber, 0.5f) * (power * 0.34f), CurrentAngle,
				flare.Size() * 0.5f, new Vector2(0.16f, 0.055f) * flarePulse,
				SpriteEffects.None, 0f);
		}

		private void DrawBladeLightPose(SpriteBatch spriteBatch, Texture2D softCrescent,
			Texture2D sharpCrescent, Vector2 handWorld, float angle, float reach,
			Color bodyColor, Color edgeColor, float opacity, float scale, SpriteEffects flip) {
			Vector2 direction = BaseHeldMeleeSupport.Dir(angle);
			Vector2 center = handWorld + direction * reach * 0.53f - Main.screenPosition;
			// 新刀光的长轴原生朝下，因此直接跟随刀轴旋转即可让弧面横切挥舞方向。
			float rotation = angle;
			// 保留原图的弧形比例，只让暗红主体略宽、暖色刃缘更窄。
			Vector2 softScale = new(0.34f, 0.48f);
			Vector2 edgeScale = new(0.22f, 0.43f);

			spriteBatch.Draw(softCrescent, center, null, bodyColor * (opacity * 0.38f),
				rotation, softCrescent.Size() * 0.5f, softScale * scale, flip, 0f);
			spriteBatch.Draw(sharpCrescent, center + direction * 3f, null,
				edgeColor * (opacity * 0.52f), rotation, sharpCrescent.Size() * 0.5f,
				edgeScale * scale, flip, 0f);
			spriteBatch.Draw(sharpCrescent, center + direction * 5f, null,
				BladeVfxColor(new Color(255, 241, 204)) * (opacity * 0.22f), rotation, sharpCrescent.Size() * 0.5f,
				new Vector2(0.13f, 0.38f) * scale, flip, 0f);
		}

		protected override void DrawWeaponVfxBehind(Color lightColor) {
			if (Main.dedServ || slashTrailEffect?.Value == null)
				return;

			DrawRefinedBladeTrail();
		}

		private void DrawRefinedBladeTrail() {
			if (motionTrailCount < 2 || momentum <= 0.018f)
				return;
			List<MotionTrailSample> curve = BuildSmoothMotionTrail();
			if (curve.Count < 2)
				return;

			float skillScale = SkillMode == HellagurSkillBridge.Skill3 ? 1.18f
				: SkillMode == HellagurSkillBridge.Normal ? 1f : 1.08f;
			float power = MathHelper.Clamp(momentum * (SkillMode == HellagurSkillBridge.Skill3 ? 1.22f : 1f), 0f, 1f);
			float lowHealth = LowHealthIntensity;
			var shadowVertices = new List<TrailMaker.CustomVertexInfo>(curve.Count * 2);
			var lightVertices = new List<TrailMaker.CustomVertexInfo>(curve.Count * 2);
			var edgePositions = new List<Vector2>(curve.Count);
			var edgeStrengths = new List<float>(curve.Count);

			// 真实姿态只保留每tick一个；绘制时按约2像素弧长自适应重采样，再以
			// Catmull-Rom连续曲线生成刀幕。这样提高视觉更新密度而不改变攻击速度与伤害时序。
			for (int i = 0; i < curve.Count; i++) {
				MotionTrailSample sample = curve[i];
				Vector2 hand = sample.Hand;
				Vector2 direction = BaseHeldMeleeSupport.Dir(sample.Angle);
				float reach = SkillMode == HellagurSkillBridge.Skill3 ? 142f : 116f;
				float history = i / (float)Math.Max(1, curve.Count - 1); // 0 新刀锋，1 旧尾迹
				float frontTaper = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(history / 0.13f, 0f, 1f));
				float tailTaper = 1f - MathHelper.SmoothStep(0f, 1f,
					MathHelper.Clamp((history - 0.56f) / 0.44f, 0f, 1f));
				float envelope = MathF.Sqrt(MathHelper.Clamp(frontTaper * tailTaper, 0f, 1f));
				float strength = MathF.Pow(1f - history, 0.62f) * envelope * power;
				Vector2 center = hand + direction * reach * 0.68f;
				float halfWidth = reach * 0.30f * envelope * skillScale;
				Vector2 inner = center - direction * halfWidth;
				Vector2 outer = center + direction * halfWidth;
				edgePositions.Add(outer);
				edgeStrengths.Add(strength);

				Color shadowInner = Color.Lerp(new Color(2, 0, 3), new Color(38, 0, 9), 1f - history)
					* (0.72f * strength);
				Color shadowOuter = Color.Lerp(new Color(9, 0, 7), new Color(72, 2, 16), 1f - history)
					* (0.6f * strength);
				shadowVertices.Add(new TrailMaker.CustomVertexInfo(inner, BladeVfxColor(shadowInner), new Vector3(history, 0f, 1f)));
				shadowVertices.Add(new TrailMaker.CustomVertexInfo(outer, BladeVfxColor(shadowOuter), new Vector3(history, 1f, 1f)));

				float hotHead = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(history / 0.34f, 0f, 1f));
				Color outerHead = Color.Lerp(new Color(255, 224, 164), new Color(232, 32, 52), hotHead);
				outerHead = Color.Lerp(outerHead, new Color(72, 2, 22),
					MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((history - 0.45f) / 0.55f, 0f, 1f)));
				Color innerLight = Color.Lerp(new Color(150, 12, 31), new Color(28, 0, 14), history)
					* ((0.34f + lowHealth * 0.12f) * strength);
				Color outerLight = outerHead * ((0.7f + lowHealth * 0.18f) * strength);
				// Briny 的大挥砍读感来自宽轮廓、较窄主体、极窄亮边；这里保留这个比例关系，
				// 但几何、材质与血月配色全部由赫拉格自己生成。
				float lightHalfWidth = halfWidth * 0.62f;
				lightVertices.Add(new TrailMaker.CustomVertexInfo(center - direction * lightHalfWidth,
					BladeVfxColor(innerLight), new Vector3(history, 0.04f, 1f)));
				lightVertices.Add(new TrailMaker.CustomVertexInfo(center + direction * lightHalfWidth,
					BladeVfxColor(outerLight), new Vector3(history, 0.96f, 1f)));
			}

			if (shadowVertices.Count < 4 || lightVertices.Count < 4)
				return;

			var edgeVertices = new List<TrailMaker.CustomVertexInfo>(edgePositions.Count * 2);
			for (int i = 0; i < edgePositions.Count; i++) {
				Vector2 previous = edgePositions[Math.Max(0, i - 1)];
				Vector2 next = edgePositions[Math.Min(edgePositions.Count - 1, i + 1)];
				Vector2 tangent = (next - previous).SafeNormalize(BaseHeldMeleeSupport.Dir(CurrentAngle));
				Vector2 normal = new(-tangent.Y, tangent.X);
				float history = i / (float)Math.Max(1, edgePositions.Count - 1);
				float strength = edgeStrengths[i];
				float width = MathHelper.Lerp(0.45f, 2.8f + lowHealth * 0.9f, strength);
				Color edgeColor = Color.Lerp(new Color(255, 241, 204), new Color(245, 137, 61),
					MathHelper.SmoothStep(0f, 1f, history)) * (strength * (0.72f + lowHealth * 0.18f));
				edgeVertices.Add(new TrailMaker.CustomVertexInfo(edgePositions[i] - normal * width,
					BladeVfxColor(edgeColor), new Vector3(history, 0.42f, 1f)));
				edgeVertices.Add(new TrailMaker.CustomVertexInfo(edgePositions[i] + normal * width,
					BladeVfxColor(edgeColor), new Vector3(history, 0.58f, 1f)));
			}

			DrawBladeTrailPass(shadowVertices, lightVertices, edgeVertices);
		}

		private List<MotionTrailSample> BuildSmoothMotionTrail() {
			var result = new List<MotionTrailSample>(motionTrailCount * 20);
			if (motionTrailCount < 2)
				return result;

			float reach = SkillMode == HellagurSkillBridge.Skill3 ? 142f : 116f;
			for (int segment = 0; segment < motionTrailCount - 1; segment++) {
				int i0 = Math.Max(0, segment - 1);
				int i1 = segment;
				int i2 = segment + 1;
				int i3 = Math.Min(motionTrailCount - 1, segment + 2);

				float a0 = motionAngleTrail[i0];
				float a1 = motionAngleTrail[i1];
				float a2 = motionAngleTrail[i2];
				float a3 = motionAngleTrail[i3];
				Vector2 h0 = motionHandTrail[i0];
				Vector2 h1 = motionHandTrail[i1];
				Vector2 h2 = motionHandTrail[i2];
				Vector2 h3 = motionHandTrail[i3];

				float estimatedPixels = MathF.Abs(a2 - a1) * reach + Vector2.Distance(h1, h2);
				int steps = Math.Clamp((int)MathF.Ceiling(estimatedPixels / TrailSampleSpacing), 2, MaxCurveStepsPerTick);
				for (int step = 0; step < steps; step++) {
					float t = step / (float)steps;
					float angle = MathHelper.CatmullRom(a0, a1, a2, a3, t);
					// 停刀或反向节点不允许样条越过相邻真实姿态，避免曲线末端产生小钩。
					angle = MathHelper.Clamp(angle, MathF.Min(a1, a2), MathF.Max(a1, a2));
					Vector2 hand = Vector2.CatmullRom(h0, h1, h2, h3, t);
					result.Add(new MotionTrailSample(hand, angle));
				}
			}

			result.Add(new MotionTrailSample(motionHandTrail[motionTrailCount - 1],
				motionAngleTrail[motionTrailCount - 1]));
			return result;
		}

		private readonly struct MotionTrailSample {
			public readonly Vector2 Hand;
			public readonly float Angle;

			public MotionTrailSample(Vector2 hand, float angle) {
				Hand = hand;
				Angle = angle;
			}
		}

		private static void DrawBladeTrailPass(List<TrailMaker.CustomVertexInfo> shadowVertices,
			List<TrailMaker.CustomVertexInfo> lightVertices,
			List<TrailMaker.CustomVertexInfo> edgeVertices) {
			Effect effect = slashTrailEffect.Value;
			Texture2D mask = ModContent.Request<Texture2D>(SlashTrailMaskPath).Value;
			GraphicsDevice device = Main.graphics.GraphicsDevice;
			BlendState oldBlend = device.BlendState;
			DepthStencilState oldDepth = device.DepthStencilState;
			RasterizerState oldRasterizer = device.RasterizerState;
			SamplerState oldSampler0 = device.SamplerStates[0];
			SamplerState oldSampler1 = device.SamplerStates[1];

			Main.spriteBatch.End();
			try {
				Matrix projection = Matrix.CreateOrthographicOffCenter(
					0f, Main.screenWidth, Main.screenHeight, 0f, 0f, 1f);
				Matrix model = Matrix.CreateTranslation(
						new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0f))
					* Main.GameViewMatrix.ZoomMatrix;

				device.DepthStencilState = DepthStencilState.None;
				device.RasterizerState = RasterizerState.CullNone;
				device.SamplerStates[0] = SamplerState.LinearWrap;
				device.SamplerStates[1] = SamplerState.LinearWrap;
				effect.Parameters["uWorldViewProjection"]?.SetValue(model * projection);
				effect.Parameters["uImage0"]?.SetValue(mask);
				effect.Parameters["uImage1"]?.SetValue(mask);
				effect.Parameters["uTime"]?.SetValue((float)Main.GlobalTimeWrappedHourly * 0.32f);
				effect.Parameters["uOpacity"]?.SetValue(1f);

				device.BlendState = BlendState.AlphaBlend;
				effect.CurrentTechnique.Passes["TrailPass"].Apply();
				device.DrawUserPrimitives(PrimitiveType.TriangleStrip, shadowVertices.ToArray(), 0,
					shadowVertices.Count - 2);

				device.BlendState = BlendState.Additive;
				effect.CurrentTechnique.Passes["TrailPass"].Apply();
				device.DrawUserPrimitives(PrimitiveType.TriangleStrip, lightVertices.ToArray(), 0,
					lightVertices.Count - 2);

				if (edgeVertices.Count >= 4) {
					effect.CurrentTechnique.Passes["TrailPass"].Apply();
					device.DrawUserPrimitives(PrimitiveType.TriangleStrip, edgeVertices.ToArray(), 0,
						edgeVertices.Count - 2);
				}
			}
			finally {
				device.BlendState = oldBlend;
				device.DepthStencilState = oldDepth;
				device.RasterizerState = oldRasterizer;
				device.SamplerStates[0] = oldSampler0;
				device.SamplerStates[1] = oldSampler1;
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
					Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
					null, Main.GameViewMatrix.TransformationMatrix);
			}
		}

		private void DrawMoonPhase(SpriteBatch spriteBatch, Texture2D glow, Color crimson, Color gold) {
			if (!Damaging && SkillMode == HellagurSkillBridge.Normal)
				return;

			Texture2D crescent = ModContent.Request<Texture2D>(SlashEdgePath).Value;
			Texture2D moon = ModContent.Request<Texture2D>(
				"ArknightsMod/Content/Textures/circle_03").Value;
			Texture2D flare = ModContent.Request<Texture2D>(
				"ArknightsMod/Content/Textures/flare_01").Value;
			Vector2 direction = BaseHeldMeleeSupport.Dir(CurrentAngle);
			Vector2 normal = new(-direction.Y, direction.X);
			Vector2 centerWorld = HandWorld + direction * (SkillMode == HellagurSkillBridge.Skill3 ? 66f : 50f);
			Vector2 center = centerWorld - Main.screenPosition;
			float rotation = CurrentAngle;
			float pulse = 0.92f + 0.08f * MathF.Sin((float)Main.GlobalTimeWrappedHourly * 8f + Projectile.whoAmI);
			float motionPower = Damaging ? MathHelper.Clamp(momentum * 1.35f, 0.18f, 1f) : 0.22f;
			Vector2 crescentOrigin = crescent.Size() * 0.5f;

			switch (SkillMode) {
				case HellagurSkillBridge.Skill1:
					spriteBatch.Draw(crescent, center, null, crimson * (0.68f * motionPower), rotation,
						crescentOrigin, new Vector2(0.15f, 0.34f) * pulse, SpriteEffects.None, 0f);
					spriteBatch.Draw(crescent, center + direction * 4f, null, gold * (0.5f * motionPower), rotation,
						crescentOrigin, new Vector2(0.1f, 0.26f) * pulse, SpriteEffects.None, 0f);
					break;
				case HellagurSkillBridge.Skill2:
					for (int side = -1; side <= 1; side += 2) {
						Vector2 offset = normal * side * 8f;
						SpriteEffects effects = side < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
						spriteBatch.Draw(crescent, center + offset, null, crimson * (0.58f * motionPower),
							rotation + side * 0.16f, crescentOrigin, new Vector2(0.14f, 0.36f) * pulse, effects, 0f);
					}
					spriteBatch.Draw(flare, center, null, gold * (0.38f * motionPower), CurrentAngle,
						flare.Size() * 0.5f, new Vector2(0.14f, 0.055f), SpriteEffects.None, 0f);
					break;
				case HellagurSkillBridge.Skill3:
					BaseHeldMeleeSupport.DrawGlow(glow, centerWorld, crimson * 0.5f, 0.58f * pulse);
					BaseHeldMeleeSupport.DrawGlow(glow, centerWorld, gold * 0.2f, 0.3f * pulse);
					spriteBatch.Draw(moon, center, null, crimson * (0.62f * motionPower),
						(float)Main.GlobalTimeWrappedHourly * 0.5f, moon.Size() * 0.5f,
						0.17f * pulse, SpriteEffects.None, 0f);
					spriteBatch.Draw(crescent, center + direction * 8f, null, gold * (0.54f * motionPower),
						rotation, crescentOrigin, new Vector2(0.18f, 0.42f) * pulse, SpriteEffects.None, 0f);
					break;
				default:
					spriteBatch.Draw(crescent, center, null, crimson * (0.3f * motionPower), rotation,
						crescentOrigin, new Vector2(0.12f, 0.28f), SpriteEffects.None, 0f);
					break;
			}
		}

		private float LowHealthIntensity {
			get {
				float storedScale = Projectile.ai[2] > 0f ? Projectile.ai[2] : 1f;
				float inferredLifeRatio = 1f - (storedScale - 1f) * 0.7f;
				return MathHelper.Clamp((0.7f - inferredLifeRatio) / 0.4f, 0f, 1f);
			}
		}

		private Player Owner => Main.player[Projectile.owner];
	}
}
