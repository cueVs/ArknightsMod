using ArknightsMod.Common.UI;
using ArknightsMod.Content;
using ArknightsMod.Content.Items.Weapons.Specialist.Scene;
using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Content.Items.Weapons.Guard.Lupine;
using ArknightsMod.Content.Items.Weapons.Guard.Oblivionis;
using ArknightsMod.Content.Items.Weapons.Caster.Lava;
using ArknightsMod.Content.Items.Weapons.Caster.Haze;
using ArknightsMod.Content.Items.Weapons.Defender.Beagle;
using ArknightsMod.Content.Items.Weapons.Defender.Nian;
using ArknightsMod.Content.Items.Weapons.Defender.NoirCorne;
using ArknightsMod.Content.Items.Weapons.Guard.Chen;
using ArknightsMod.Content.Items.Weapons.Guard.SilverAsh;
using ArknightsMod.Content.Items.Weapons.Guard.Thorns;
using ArknightsMod.Content.Items.Weapons.Guard.Surtr;
using ArknightsMod.Content.Items.Weapons.Sniper.Exusiai;
using ArknightsMod.Content.Items.Weapons.Sniper.Jessica;
using ArknightsMod.Content.Items.Weapons.Sniper.Kroos;
using ArknightsMod.Content.Items.Weapons.Sniper.KroosAlter;
using ArknightsMod.Content.Items.Weapons.Sniper.Pozemka;
using ArknightsMod.Content.Items.Weapons.Sniper.Schwarz;
using ArknightsMod.Content.Items.Weapons.Sniper.Shirayuki;
using ArknightsMod.Content.Items.Weapons.Sniper.Typhon;
using ArknightsMod.Content.Items.Weapons.Vanguard.Bagpipe;
using ArknightsMod.Content.Items.Weapons.Caster.Haze;
using ArknightsMod.Content.Items.Weapons.Medic.Closure;
using ArknightsMod.Systems.Gameplay.Skill;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ArknightsMod.Players
{
	public class WeaponPlayer : ModPlayer
	{
		public int defenseBonus = 0;
		protected override bool CloneNewInstances => true;
		public SkillData CurrentSkill => SkillData[Skill];
		public int SkillCount { get; private set; }
		public readonly SkillData[] SkillData = new SkillData[3];

		// 技能资源管理
		public int SkillCharge;
		public int SkillChargeMax;
		public bool SkillActive;
		public int SkillTimer;
		public int SP;
		public int StockCount;
		public int Div;
		public int Skill;
		public bool SummonMode;
		public bool SkillInitialize = true;

		// 隐德来希 S1「玫影觅迹」：下次普攻强化（175% 伤害 + 连续两段刀光）
		public bool EntelechiaEmpoweredNext;
		// 第二段刀光延迟发射（一前一后分两次）
		public int EntelechiaSecondShotDelay;
		public Microsoft.Xna.Framework.Vector2 EntelechiaPendingVel;
		public int EntelechiaPendingDamage;
		public float EntelechiaPendingKb;

		// SP恢复加成系统
		public float SPRegenMultiplier { get; set; } = 1f;
		private float spRegenFraction;

		/// <summary>
		/// 全局技力恢复速度倍率：开启配置开关时为 2（二倍速），关闭时为 1（原速）。
		/// 只影响"技力条恢复的快慢"（自然回复/受击回复/攻击回复/饰品加速），不影响任何数值——
		/// 技能所需技力、技力上限等仍按 CSV/武器数据里 1 倍速的原始数值走，
		/// 部署费用吸收、套装/技能直接赠送技力等"技力返还"效果同样不受这个倍率影响
		/// （它们不经过这里，直接改 SP 或 SkillCharge，参见 <see cref="AbsorbDeploymentCost"/>
		/// 与 OperatorSPHelper.TryGainSP）。
		/// </summary>
		public static int GlobalSPRegenSpeedMultiplier =>
			ModContent.GetInstance<Common.Configs.SkillChargeConfig>().DoubleSPRegenSpeed ? 2 : 1;

		/// <summary>
		/// 技能开启后，"消耗"技力（即倒数持续时间）的速度倍率：开启配置时为 2（二倍速），
		/// 关闭时为 1（原速）。用在 UpdateActiveSkill/StrikeSkill 里，把用来判断"技能是否
		/// 该结束"的时长阈值按这个倍率缩小——阈值越小，同样每帧 +1 的 SkillTimer 越快到达，
		/// 表现就是持续时间变成原来的 1/2。CSV/武器数据里的持续时间数值本身不受影响。
		/// </summary>
		public static float ActiveDurationMultiplier =>
			ModContent.GetInstance<Common.Configs.SkillChargeConfig>().DoubleSkillDurationConsumption ? 0.5f : 1f;

		private bool chargeReady;
		private bool chargeOpen;
		private bool hasNearbyEnemy;
		private int initChargeTimer;

		// 位置信息
		public float mousePositionX;
		public float mousePositionY;
		public float playerPositionX;
		public float playerPositionY;

		// 武器状态
		public bool HoldBagpipeSpear = false;
		public bool HoldChenSword = false;
		public bool HoldKroosCrossbow = false;
		public bool HoldChenSword_Item = false;
		public bool HoldSilverAshWeapon = false;
		public bool HoldBeagleWeapon = false;
		public bool HoldShirayuki_Shuriken = false;
		public bool HoldLava_Dagger = false;
		public bool HoldThornsWeapon = false;
		public bool HoldKroosAlterCrossbow = false;
		public bool HoldExusiaiVector = false;
		public bool HoldPozemkaCrossbow = false;
		public bool HoldNianWeapon = false;
		public bool HoldNoirShield = false;
		public bool HoldSchwarzBow = false;
		public bool HoldTyphonBow = false;
		public bool HoldHazeMagicBook = false;
		public bool HoldSurtrLaevatain = false;
		public bool HoldJessicaGun = false;

		private int oldHeld;
		private int oldSkill;
		private readonly Dictionary<int, int> selectedSkillByItemType = [];

		// 技能数据结构
		public int HowManySkills = 0;
		public List<int?> InitialSP = [null, null, null];
		public List<int?> InitialSPs1List = [null, null, null, null, null, null, null, null, null, null];
		public List<int?> InitialSPs2List = [null, null, null, null, null, null, null, null, null, null];
		public List<int?> InitialSPs3List = [null, null, null, null, null, null, null, null, null, null];
		public List<int?> MaxSP = [null, null, null];
		public List<int?> MaxSPs1List = [null, null, null, null, null, null, null, null, null, null];
		public List<int?> MaxSPs2List = [null, null, null, null, null, null, null, null, null, null];
		public List<int?> MaxSPs3List = [null, null, null, null, null, null, null, null, null, null];
		public List<float> SkillActiveTime = [0, 0, 0];
		public List<float> SkillActiveTimeS1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
		public List<float> SkillActiveTimeS2List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
		public List<float> SkillActiveTimeS3List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
		public List<int> SkillLevel = [0, 0, 0];
		public List<int> StockMax = [0, 0, 0];
		public List<int> StockMaxS1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
		public List<int> StockMaxS2List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
		public List<int> StockMaxS3List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
		public List<bool> AutoTrigger = [false, false, false];
		public List<bool> ChargeTypeIsPerSecond = [false, false, false];
		public string IconName = "";
		public List<bool> ShowSummonIconBySkills = [false, false, false];
		public override void UpdateDead() {
			// 死亡等价于整批干员重新部署：背包里每一把技能武器都要恢复自己的初始技力资格，
			// 不能只让复活后最先拿起的那一把吃到玩家级重置标记。
			foreach (Item item in Player.inventory) {
				if (item.ModItem is UpgradeWeaponBase weapon)
					Array.Fill(weapon.chargeReady, true);
			}
			ClearSkillRuntime();
			ClearLoadedSkillIdentity();
			chargeReady = true;
			chargeOpen = true;
		}

		//======================================================================
		//新添入的，用于更新
		public override void PostUpdate() {
			UnderAttack = false;

			// S1 第二段刀光：倒计时结束后发射（更大、正常速度的放大刀光）
			if (EntelechiaSecondShotDelay > 0) {
				if (--EntelechiaSecondShotDelay == 0 && Player.whoAmI == Main.myPlayer) {
					int id = Projectile.NewProjectile(Player.GetSource_ItemUse(Player.HeldItem),
						Player.Center, EntelechiaPendingVel,
						ModContent.ProjectileType<Content.Projectiles.Guard.Entelechia.EntelechiaScytheBladeWaveProjectile>(),
						EntelechiaPendingDamage, EntelechiaPendingKb, Player.whoAmI, 1f, 1.6f, 1f);
					if (id >= 0 && id < Main.maxProjectiles)
						Main.projectile[id].scale = 1.5f;
				}
			}
			if (!Player.dead && (HowManySkills > 0 || SkillCount > 0)) {
				if (CurrentSkill?.ChargeType == SkillChargeType.Auto && CurrentSkill.UsesCustomCharge != true) {
					AccessoriesAutoCharge();
				}
				else if (CurrentSkill == null && ChargeTypeIsPerSecond[Skill]) {
					AccessoriesAutoCharge();
				}
			}

			// 提丰 S3：技能持续期间挥弓动画结束后，原版仍会对 Shoot 武器套用「朝向鼠标」的持弓旋转，
			// 与自定义蓄力角衔接时会像收起后又扭向瞄准方向；非挥弓帧保持略向下的收起角（与 TyphonBow UseStyle 末段一致）。
			if (!Player.dead && Skill == 2 && SkillActive && Player.itemAnimation <= 0
			    && Player.HeldItem.ModItem is TyphonBow) {
				Player.itemRotation = TyphonBow.GetS3SkillIdleItemRotation(Player);
			}

			// 可露希尔·扫描枪开火后坐力：必须在 PostUpdate（原版 ItemCheck_ApplyUseStyle 之后）
			// 才能叠加 itemRotation，否则在 HoldItem 里设置会被随后的原版持枪瞄准逻辑覆盖掉，
			// 表现为"抬起动作完全不生效"。
			if (Player.HeldItem.ModItem is ClosureScanGun closureGun) {
				closureGun.ApplyRecoilKick(Player);
			}
		}
		//=======================================================================
		public void InitSkill(bool giveCharge) {
			SkillData skill = CurrentSkill;

			if (skill == null) {
				// if(HowManySkills>0)
				// 	Main.NewText($"[{GetType()}] 错误: 当前技能数据mp.CurrentSkill为null", Color.Red);
				return;
			}

			SkillLevelData data = skill.CurrentLevelData;
			Div = skill.ChargeType == SkillChargeType.Auto ? 60 : 1;
			int initSP = data.InitSP;
			int maxSP = data.MaxSP;
			if (giveCharge) {
				if (initSP == maxSP) {
					SkillCharge = 0;
					StockCount = 1;
					SP = StockCount == data.MaxStack ? maxSP : 0;
				}
				else {
					SkillCharge = initSP * Div;
					StockCount = 0;
					SP = initSP;
				}
			}
			else {
				SkillCharge = 0;
				SP = 0;
				StockCount = 0;
			}

			SkillChargeMax = maxSP * Div;
			SkillTimer = 0;
			SkillActive = false;
			SummonMode = false;
		}

		/// <summary>
		/// 原子地切换当前武器技能：旧技能在同一帧结束，新技能资源也在同一帧完成初始化，
		/// 避免 UI 先改索引、下一帧才清状态造成的“旧技能效果套到新技能”串态。
		/// </summary>
		public bool TrySelectSkill(UpgradeWeaponBase weapon, int skill, bool force = false) {
			if (weapon == null || skill < 0 || skill >= SkillData.Length || SkillData[skill] == null)
				return false;
			if (!force && (SkillCount <= skill || Skill == skill))
				return false;

			Skill = skill;
			selectedSkillByItemType[weapon.Item.type] = skill;
			oldSkill = skill;
			InitSkill(weapon.chargeReady[skill]);
			weapon.chargeReady[skill] = false;
			return true;
		}

		private void ClearSkillRuntime() {
			SkillCharge = 0;
			SkillChargeMax = 0;
			SkillActive = false;
			SkillTimer = 0;
			SP = 0;
			StockCount = 0;
			Div = 1;
			SummonMode = false;
		}

		private void ClearLoadedSkillIdentity() {
			oldHeld = 0;
			oldSkill = -1;
			SkillCount = 0;
			for (int i = 0; i < SkillData.Length; i++)
				SkillData[i] = null;
		}

		// 将当前技能技力填满至可释放状态
		public void DevFillSkillCharge()
		{
			if (CurrentSkill == null || SkillActive)
				return;

			SkillLevelData data = CurrentSkill.CurrentLevelData;
			StockCount = data.MaxStack;
			SkillCharge = 0;
			SP = data.MaxSP;
			chargeOpen = true;

			if (Player.HeldItem.ModItem is UpgradeWeaponBase ark)
				ark.chargeReady = [true, true, true];
		}

		/// <summary>
		/// 部署费用是否可以被当前玩家吸收：必须手持本模组的（有技力系统的）武器，
		/// 且当前技能未处于开启状态（技力自然回复时）。技能开启期间、或手持其它武器/无技能武器时不可吸收；
		/// 另外——技力已满（技能就绪、无处可加）时也不吸收，让部署费用继续环绕等待。
		/// </summary>
		public bool CanAbsorbDeploymentCost() {
			if (!Player.active || Player.dead || Player.HeldItem.ModItem is not UpgradeWeaponBase
				|| CurrentSkill == null || SkillActive)
				return false;
			// 已攒满一个可用技能（技力满）：不再吸收，否则会白白吞掉部署费用
			if (StockCount >= CurrentSkill.CurrentLevelData.MaxStack)
				return false;
			return true;
		}

		/// <summary>
		/// 吸收部署费用：直接把充能加到当前技能的技力条上（不以物品形式出现在背包）。
		/// 沿用 Div 的换算关系——Div 单位的 SkillCharge 对应 1 点可见技力（与 <see cref="AutoCharge"/> 等一致）。
		/// </summary>
		public bool AbsorbDeploymentCost(int points) {
			if (!CanAbsorbDeploymentCost())
				return false;

			SkillLevelData data = CurrentSkill.CurrentLevelData;
			if (StockCount >= data.MaxStack)
				return false;

			SkillCharge += points * Div;
			while (SkillCharge >= SkillChargeMax && StockCount < data.MaxStack) {
				SkillCharge -= SkillChargeMax;
				StockCount++;
			}
			SP = StockCount >= data.MaxStack ? data.MaxSP : SkillCharge / Div;
			if (StockCount >= data.MaxStack)
				SkillCharge = 0;
			return true;
		}

		public void SetSkill(int skill) {
			if (SkillInitialize) {
				Skill = skill;
				if (ChargeTypeIsPerSecond[Skill])
					Div = 60;
				else
					Div = 1;

				SkillCharge = InitialSP[Skill] != null ? (int)InitialSP[Skill] * Div : 0;
				SP = InitialSP[Skill] != null ? (int)InitialSP[Skill] : 0;
				SkillChargeMax = MaxSP[Skill] != null ? (int)MaxSP[Skill] * Div : 0;
				SkillTimer = 0;
				StockCount = 0;
				SkillActive = false;

				if (InitialSP[Skill] == MaxSP[Skill]) {
					SkillCharge = 0;
					StockCount++;
					SP = StockCount == StockMax[Skill] ? (int)MaxSP[Skill] : 0;
				}

				SummonMode = false;
				SkillInitialize = false;
			}
		}

		public override void ResetEffects() {

			defenseBonus = 0;
			SPRegenMultiplier = 1f; // 重置SP恢复倍率（修改后的）
			AccessoriesChargeFraction = 0f; // 新添加，用于重置技力藏的加成

			// 更新武器状态
			HoldBagpipeSpear = Player.HeldItem.ModItem is BagpipeSpear;
			HoldExusiaiVector = Player.HeldItem.ModItem is ExusiaiVector;
			HoldKroosCrossbow = Player.HeldItem.ModItem is KroosCrossbow;
			HoldChenSword_Item = Player.HeldItem.ModItem is ChenSword_Item;
			HoldSilverAshWeapon = Player.HeldItem.ModItem is SilverAshWeapon;
			HoldBeagleWeapon = Player.HeldItem.ModItem is BeagleWeapon;
			HoldNoirShield = Player.HeldItem.ModItem is NoirShield;
			HoldThornsWeapon = Player.HeldItem.ModItem is ThornsWeapon;
			HoldShirayuki_Shuriken = Player.HeldItem.ModItem is Shirayuki_Shuriken;
			HoldLava_Dagger = Player.HeldItem.ModItem is Lava_Dagger;
			HoldKroosAlterCrossbow = Player.HeldItem.ModItem is KroosAlterCrossbow;
			HoldPozemkaCrossbow = Player.HeldItem.ModItem is PozemkaCrossbow;
			HoldNianWeapon = Player.HeldItem.ModItem is NianWeapon;
			HoldSchwarzBow = Player.HeldItem.ModItem is SchwarzBow;
			HoldTyphonBow  = Player.HeldItem.ModItem is TyphonBow;
			HoldHazeMagicBook  = Player.HeldItem.ModItem is HazeMagicBook;
			HoldSurtrLaevatain  = Player.HeldItem.ModItem is SurtrLaevatain;
			HoldJessicaGun  = Player.HeldItem.ModItem is JessicaGun;
			// 基于武器的技能系统
			hasNearbyEnemy = false;
			// 旧版武器支持
			HowManySkills = 0;
			SetAllSkillsData();

			// 死亡期间不重新初始化手中武器。UpdateDead 已清空运行态；复活后的第一帧
			// 再按当前物品的 InitSP 正常部署，技能不会跨死亡暂停后续上。
			if (Player.dead)
				return;

			Item item = Player.HeldItem;
			if (item.ModItem is UpgradeWeaponBase ark) {
				if (chargeOpen) {
					foreach (var npc in Main.ActiveNPCs) {
						if (npc.CanBeChasedBy(Player) && npc.Distance(Player.MountedCenter) < 35 * 16) {
							hasNearbyEnemy = true;
							initChargeTimer = 0;
							break;
						}
					}
					if (!hasNearbyEnemy && ++initChargeTimer >= GetRestoreTime()) {
						initChargeTimer = 0;
						// 仅解锁自然回复（关闭冻结），不要置 chargeReady——
						// chargeReady 会被 Propagate 成 ark.chargeReady，进而在下方
						// else if 分支把 InitSP 预灌进 SkillCharge，导致技能结束后
						// 技力不从 0 而直接从初始技力开始回复。
						chargeOpen = false;
					}
				}
				if (chargeReady) {
					chargeReady = false;
					chargeOpen = false;
					ark.chargeReady = [true, true, true];
					if (Player == Main.LocalPlayer)
						CombatText.NewText(Player.Hitbox.Modified(0, -48, 0, 0), Microsoft.Xna.Framework.Color.Gold,
							Language.GetTextValue("Mods.ArknightsMod.Skills.ChargeReady"), true);
				}
				int type = item.type;
				if (type != oldHeld) {
					oldSkill = -1;
					oldHeld = type;
					SkillCount = 0;

					for (int i = 0; i < 3; i++) {
						SkillData data = ark.GetSkillData(i);
						SkillData[i] = data;
						SkillCount += data == null ? 0 : 1;
					}

					Skill = selectedSkillByItemType.TryGetValue(type, out int selected)
						&& selected >= 0 && selected < SkillData.Length && SkillData[selected] != null
						? selected
						: 0;
					selectedSkillByItemType[type] = Skill;

					if (!Main.dedServ && Player.whoAmI == Main.myPlayer)
						SelectSkills.ChangeSkillSlot(ark);
				}

				if (oldSkill != Skill) {
					oldSkill = Skill;
					// 切换武器（oldHeld 改变时 oldSkill 被置 -1）或切换技能槽（mp.Skill 改变）时，
					// 一律按"初始就绪"授予初始技力 InitSP，使玩家每次换武器/换技能都从初始技力起步。
					// 不再依赖 ark.chargeReady（该标志只用于 DevFillSkillCharge / 重生等少数场景），
					// 否则正常游玩中切换不会给初始技力，且与上一处冻结解锁污染初始技力的问题同源。
					InitSkill(true);
					ark.chargeReady[Skill] = false;
				}
				else if (ark.chargeReady[Skill] && StockCount == 0) {
					ark.chargeReady[Skill] = false;

					// 脱战/复活补回“初始技力下限”时，数值条、库存和内部 tick 必须一起更新。
					// 只写 SkillCharge 会出现条上仍显示旧 SP，InitSP == MaxSP 时也不会变成可释放库存。
					SkillData data = ark.GetSkillData(Skill);
					if (data != null) {
						SkillLevelData level = data.CurrentLevelData;
						if (level.InitSP >= level.MaxSP) {
							SkillCharge = 0;
							StockCount = Math.Min(level.MaxStack, 1);
							SP = level.MaxSP;
						}
						else {
							SkillCharge = Math.Max(SkillCharge, level.InitSP * Div);
							SP = SkillCharge / Math.Max(1, Div);
						}
					}
				}
			}
			else if (oldHeld != 0) {
				// 定时技能不能靠换到普通物品来冻结倒计时；离开技能武器即清场。
				// 煌 S2 的“本次生命永久”由其专属 ModPlayer 显式恢复，不依赖这份公共残留状态。
				ClearSkillRuntime();
				ClearLoadedSkillIdentity();
			}
		}

		// 释放技能后，如果附近 560px 内没有可仇恨的敌人，自然回复会被冻结，
		// 直到出现敌人、或等够这个时长才解锁（见 ResetEffects 里 chargeOpen/hasNearbyEnemy 的判定）。
		// 该冻结窗口是自动充能型技能（如隐德来希 S1）放完技能后无敌人时回充被挡的"死区"。
		// 原 5 秒过长，对瞬发/自动释放技能感知明显；现缩短到 12 帧（0.2 秒），既保留极短的节奏间隔
		// 又不致明显卡住回充。这是全局值，作用于所有武器，不是某个武器专属的。
		private int GetRestoreTime() {
			return 12;
		}

		public void TryAutoCharge() {
			if (SceneCameraSkills.BlocksSkillCharge(Player)) // 稀音技能持续期间冻结充能
				return;
			// 标记为专属充能的技能由武器自己的部署状态驱动；否则公共条和专属暖机同时推进，
			// 切换快捷栏后会出现两套进度分叉。
			if (CurrentSkill?.UsesCustomCharge == true)
				return;
			if (chargeOpen && !hasNearbyEnemy)
				return;
			if (CurrentSkill?.ChargeType == SkillChargeType.Auto)
				AutoCharge();
		}
		/// <summary>
		/// 玩家受击
		/// </summary>
		public bool UnderAttack = false;

		public override void PreUpdate()
		{
			if (Player.lifeRegen < 0)
				UnderAttack = true;
		}

		/// <summary>
		/// 统一技能开启热键的核心：ProcessTriggers 只在本地客户端跑，是自定义 ModKeybind
		/// 唯一能读到"当前是否按住"的地方。各武器的 CanUseItem/Shoot 里已经改成检查
		/// ArknightsKeybinds.SkillActivatePressed(player)，但那些钩子本身只在玩家真的
		/// 左键/右键点击、原版判定"这次点击要不要交给物品使用"时才会被调用——单纯按热键
		/// 并不会触发它们。这里在按住热键时把 Player.controlUseItem 设成 true，相当于
		/// "伪造一次左键点击"，把它接入原版本来就有的点击处理管线，武器那边的判定才有
		/// 机会跑起来。之所以伪造左键而不是右键：这样武器代码里剩下的
		/// player.altFunctionUse==2（比如摄影车部署、浮游信标）不会被误触发——两者互不干扰。
		///
		/// 只在手持本模组的技能武器时才伪造点击，避免误把其它物品（工具、纯输出武器、
		/// 别的模组物品）也顺带触发一次使用。LupineScarlet/OblivionisSword 没有继承
		/// UpgradeWeaponBase（历史遗留），单独列出来。
		/// </summary>
		public override void ProcessTriggers(Terraria.GameInput.TriggersSet triggersSet) {
			if (!ArknightsKeybinds.SkillActivatePressed(Player))
				return;

			if (Player.HeldItem.ModItem is UpgradeWeaponBase
				or LupineScarlet
				or OblivionisSword) {
				Player.controlUseItem = true;
			}
		}

		public void TryHurtCharge()
		{
			if (CurrentSkill?.ChargeType == SkillChargeType.Hurt)
				HurtCharge();
		}

		/// <summary>
		/// 受击回复技力
		/// </summary>
		public void HurtCharge()
		{
			if (CurrentSkill != null)
			{
				SkillLevelData data = CurrentSkill.CurrentLevelData;
				if (!SkillActive && StockCount < data.MaxStack)
				{
					// 受击回复类型 Div=1，SkillCharge 和 SP 一一对应；倍率大于 1 时用 >= 而非 ==
					// 判满，避免（比如 MaxSP 为奇数时）两点两点跳过精确等于 SkillChargeMax 那一格。
					int gain = GlobalSPRegenSpeedMultiplier;
					SP += gain;
					SkillCharge += gain;
					if (SkillCharge >= SkillChargeMax)
					{
						SkillCharge=0;
						SP = ++StockCount == data.MaxStack ? data.MaxSP : 0;
					}
				}
			}
		}
		public override void OnHurt(Player.HurtInfo info)
		{
			UnderAttack = true;
			TryHurtCharge();
		}
		public void AutoCharge() {
			if (SceneCameraSkills.BlocksSkillCharge(Player)) // 稀音技能持续期间冻结充能
				return;
			// 自然回复类型 Div=60（SkillCharge 每 60 tick = 1 点可见技力）。倍率 x2 时每 tick
			// 累加 2 点 SkillCharge，SP 直接按 SkillCharge/Div 换算——和原来"每 60 tick 视觉 +1"
			// 的模运算在倍率为 1 时完全等价，但同时天然支持任意倍率，不会有跳格风险。
			int gain = GlobalSPRegenSpeedMultiplier;
			if (CurrentSkill != null) {
				SkillLevelData data = CurrentSkill.CurrentLevelData;
				if (!SkillActive && StockCount < data.MaxStack) {
					SkillCharge += gain;

					if (SkillCharge >= SkillChargeMax) {
						SkillCharge = 0;
						SP = ++StockCount == data.MaxStack ? data.MaxSP : 0;
					}
					else {
						SP = SkillCharge / Div;
					}
				}
			}
			else {
				// 旧版自动充能逻辑
				if (!SkillActive && StockCount < StockMax[Skill]) {
					SkillCharge += gain;

					if (SkillCharge >= SkillChargeMax) {
						SkillCharge = 0;
						StockCount += 1;
						SP = StockCount == StockMax[Skill] ? (int)MaxSP[Skill] : 0;
					}
					else {
						SP = SkillCharge / Div;
					}
				}
			}
		}
		//============================================================================
		private float AccessoriesChargeFraction;
		public void AccessoriesAutoCharge() {

			if (SceneCameraSkills.BlocksSkillCharge(Player)) // 稀音技能持续期间冻结充能
				return;

			if (chargeOpen && !hasNearbyEnemy)
				return;



			if (CurrentSkill != null) {
				SkillLevelData data = CurrentSkill.CurrentLevelData;
				if (!SkillActive && StockCount < data.MaxStack) {

					// 饰品/套装给的额外回复速度加成也按同一倍率放大，保持"相对加成比例不变，
					// 只是整体更快"——这里 while 循环每次只消耗掉整数 1 点 AccessoriesChargeFraction、
					// SkillCharge 每次只 +1，不受倍率影响，所以内部判满逻辑不需要改。
					float extraCharge = (SPRegenMultiplier - 1f) * GlobalSPRegenSpeedMultiplier;
					AccessoriesChargeFraction += extraCharge;


					while (AccessoriesChargeFraction >= 1f) {
						AccessoriesChargeFraction -= 1f;


						SkillCharge++;
						if (SkillCharge % 60 == 0) {
							SP++;
						}


						if (SkillCharge >= SkillChargeMax) {
							SkillCharge = 0;
							StockCount++;
							SP = StockCount == data.MaxStack ? data.MaxSP : 0;

							if (StockCount >= data.MaxStack) {
								break;
							}
						}
					}
				}
			}
			else {
				// 旧版武器支持
				if (!SkillActive && StockCount < StockMax[Skill]) {
					float extraCharge = (SPRegenMultiplier - 1f) * GlobalSPRegenSpeedMultiplier;
					AccessoriesChargeFraction += extraCharge;

					while (AccessoriesChargeFraction >= 1f) {
						AccessoriesChargeFraction -= 1f;

						SkillCharge++;
						if (SkillCharge % 60 == 0) {
							SP++;
						}

						if (SkillCharge >= SkillChargeMax) {
							SkillCharge = 0;
							StockCount++;
							SP = StockCount == StockMax[Skill] ? (int)MaxSP[Skill] : 0;

							if (StockCount >= StockMax[Skill]) {
								break;
							}
						}
					}
				}
			}
		}
		//===============================================================================
		public void OffensiveRecovery() {
			if (SceneCameraSkills.BlocksSkillCharge(Player)) // 稀音技能持续期间冻结充能
				return;

			// 攻击回复类型同样 Div=1，理由同 HurtCharge：倍率大于 1 时必须用 >= 而不是 ==
			// 判满，否则 MaxSP 为奇数时会永远跳过精确等于 SkillChargeMax 的那一格，技能卡死攒不满。
			int gain = GlobalSPRegenSpeedMultiplier;

			if (CurrentSkill != null) {
				SkillLevelData data = CurrentSkill.CurrentLevelData;
				if (!SkillActive && StockCount < data.MaxStack) {
					SkillCharge += gain;
					SP += gain;
				}

				if (SkillCharge >= SkillChargeMax) {
					SkillCharge = 0;
					SP = ++StockCount == data.MaxStack ? data.MaxSP : 0;
				}
			}
			else {
				// 旧版攻击恢复逻辑
				if (!SkillActive && StockCount < StockMax[Skill]) {
					SkillCharge += gain;
					if (SkillCharge != 0)
						SP += gain;
				}

				if (SkillCharge >= SkillChargeMax) {
					SkillCharge = 0;
					StockCount += 1;
					SP = StockCount == StockMax[Skill] ? (int)MaxSP[Skill] : 0;
				}
			}
		}

		public void UpdateActiveSkill() {
			if (SkillActive) {
				// 永久技能没有持续时间倒计时；其关闭/重置条件由技能自身处理。
				if (CurrentSkill?.IsPermanent == true)
					return;

				// 阈值本身乘倍率缩小（而不是让 SkillTimer 跳着加），并且统一用 >= 判断——
				// 倍率是 0.5 时阈值大概率不再是整数，用 == 精确相等会出现永远判不到的情况，
				// 参照之前修 SP 二倍速时踩过的同一个坑（见 HurtCharge 的注释）。
				if (CurrentSkill != null) {
					if (CurrentSkill.AutoUpdateActive
						&& ++SkillTimer >= CurrentSkill.CurrentLevelData.ActiveTime * 60 * ActiveDurationMultiplier)
						SkillActive = false;
				}
				else {
					SkillTimer++;
					if (SkillTimer >= SkillActiveTime[Skill] * 60 * ActiveDurationMultiplier)
						SkillActive = false;
				}
			}
		}
		public void UpdateActiveSkill2() {
			CurrentSkill.AutoUpdateActive = false;
		}

		public void StrikeSkill() {
			if (SkillActive) {
				SkillTimer++;
				if (SkillTimer >= 10 * ActiveDurationMultiplier)
					SkillActive = false;
			}
		}


		public void DelStockCount() {
			if (CurrentSkill != null) {
				if (StockCount-- == CurrentSkill.CurrentLevelData.MaxStack)
					SP = 0;
			}
			else {
				if (StockCount == StockMax[Skill])
					SP = 0;
				StockCount -= 1;
			}
			chargeOpen = true;
			// 消耗了一层库存即代表该技能已被实际使用：清除"初始就绪"标记，
			// 否则下方 ResetEffects 的 `else if (ark.chargeReady[Skill] && StockCount == 0)`
			// 会在技能结束后（单层技能 StockCount 立刻归零）把 InitSP 预灌进 SkillCharge，
			// 导致技力不从 0 而是从初始技力开始回复（复活后尤为明显）。
			if (Player.HeldItem.ModItem is UpgradeWeaponBase ark)
				ark.chargeReady[Skill] = false;
		}

		public void SetSkillData() {
			if (HowManySkills < 1) {
				InitialSPs1List = [null, null, null, null, null, null, null, null, null, null];
				MaxSPs1List = [null, null, null, null, null, null, null, null, null, null];
			}
			if (HowManySkills < 2) {
				InitialSPs2List = [null, null, null, null, null, null, null, null, null, null];
				MaxSPs2List = [null, null, null, null, null, null, null, null, null, null];
			}
			if (HowManySkills < 3) {
				InitialSPs3List = [null, null, null, null, null, null, null, null, null, null];
				MaxSPs3List = [null, null, null, null, null, null, null, null, null, null];
			}

			InitialSP = [
				InitialSPs1List[SkillLevel[0] - 1],
				InitialSPs2List[SkillLevel[1] - 1],
				InitialSPs3List[SkillLevel[2] - 1]
			];

			MaxSP = [
				MaxSPs1List[SkillLevel[0] - 1],
				MaxSPs2List[SkillLevel[1] - 1],
				MaxSPs3List[SkillLevel[2] - 1]
			];

			SkillActiveTime = [
				SkillActiveTimeS1List[SkillLevel[0] - 1],
				SkillActiveTimeS2List[SkillLevel[1] - 1],
				SkillActiveTimeS3List[SkillLevel[2] - 1]
			];

			StockMax = [
				StockMaxS1List[SkillLevel[0] - 1],
				StockMaxS2List[SkillLevel[1] - 1],
				StockMaxS3List[SkillLevel[2] - 1]
			];
		}

		public void SetAllSkillsData() {
			if (HoldBagpipeSpear) {
				IconName = "BagpipeSpear";
				HowManySkills = 3;
				SkillLevel = [10, 10, 10];
				ChargeTypeIsPerSecond = [true, true, true];
				AutoTrigger = [false, true, false];
				ShowSummonIconBySkills = [false, false, false];

				InitialSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 15];
				InitialSPs2List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				InitialSPs3List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 25];
				MaxSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 33];
				MaxSPs2List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 4];
				MaxSPs3List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 40];
				SkillActiveTimeS1List = [0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 35f];
				SkillActiveTimeS2List = [0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.5f];
				SkillActiveTimeS3List = [0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 20f];
				StockMaxS1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 1];
				StockMaxS2List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 3];
				StockMaxS3List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 1];
				SetSkillData();
			}
			else if (HoldChenSword) {
				IconName = "ChenSword";
				HowManySkills = 2;
				SkillLevel = [10, 10, 10];
				ChargeTypeIsPerSecond = [false, false, false];
				AutoTrigger = [true, false, false];
				ShowSummonIconBySkills = [false, false, false];

				InitialSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				InitialSPs2List = [10, 10, 10, 10, 10, 10, 10, 13, 16, 20];
				MaxSPs1List = [7, 7, 7, 6, 6, 6, 5, 5, 5, 4];
				MaxSPs2List = [40, 40, 40, 38, 38, 38, 36, 34, 32, 30];
				SkillActiveTimeS1List = [0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f];
				SkillActiveTimeS2List = [1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f];
				StockMaxS1List = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1];
				StockMaxS2List = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1];
				SetSkillData();
			}
			else if (HoldKroosCrossbow) {
				IconName = "KroosCrossbow";
				HowManySkills = 1;
				SkillLevel = [7, 7, 7];
				ChargeTypeIsPerSecond = [false, true, false];
				AutoTrigger = [true, false, false];
				ShowSummonIconBySkills = [false, false, false];

				InitialSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				MaxSPs1List = [0, 0, 0, 0, 0, 0, 4, 0, 0, 0];
				SkillActiveTimeS1List = [0f, 0f, 0f, 0f, 0f, 0f, 0.2f, 0f, 0f, 0f];
				StockMaxS1List = [0, 0, 0, 0, 0, 0, 1, 0, 0, 0];
				SetSkillData();
			}
			else if (HoldChenSword_Item) {
				IconName = "ChenSword_Item";
				HowManySkills = 1;
				SkillLevel = [10, 10, 10];
				ChargeTypeIsPerSecond = [false, true, false];
				AutoTrigger = [true, false, false];
				ShowSummonIconBySkills = [false, false, false];

				InitialSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				MaxSPs1List = [0, 0, 0, 0, 0, 0, 4, 0, 0, 0];
				SkillActiveTimeS1List = [0f, 0f, 0f, 0f, 0f, 0f, 0.2f, 0f, 0f, 0f];
				StockMaxS1List = [0, 0, 0, 0, 0, 0, 1, 0, 0, 0];
				SetSkillData();
			}
			else if (HoldSilverAshWeapon) {
				IconName = "SilverAshWeapon";
				HowManySkills = 1;
				SkillLevel = [10, 10, 10];
				ChargeTypeIsPerSecond = [false, true, false];
				AutoTrigger = [true, false, false];
				ShowSummonIconBySkills = [false, false, false];

				InitialSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				MaxSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				SkillActiveTimeS1List = [0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f];
				StockMaxS1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				SetSkillData();
			}
			else if (HoldBeagleWeapon) {
				IconName = "BeagleWeapon";
				HowManySkills = 1;
				SkillLevel = [7, 7, 7];
				ChargeTypeIsPerSecond = [false, true, false];
				AutoTrigger = [true, false, false];
				ShowSummonIconBySkills = [false, false, false];

				InitialSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				MaxSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				SkillActiveTimeS1List = [0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f];
				StockMaxS1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				SetSkillData();
			}
			else if (HoldNoirShield) {
				IconName = "NoirShield";
				HowManySkills = 0;
				SkillLevel = [7, 7, 7];
				ChargeTypeIsPerSecond = [false, false, false];
				AutoTrigger = [true, false, false];
				ShowSummonIconBySkills = [false, false, false];

				InitialSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				MaxSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				SkillActiveTimeS1List = [0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f];
				StockMaxS1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				SetSkillData();
			}
			else if (HoldThornsWeapon) {
				IconName = "ThornsWeapon";
				HowManySkills = 1;
				SkillLevel = [10, 10, 10];
				ChargeTypeIsPerSecond = [false, true, false];
				AutoTrigger = [true, false, false];
				ShowSummonIconBySkills = [false, false, false];

				InitialSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				MaxSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				SkillActiveTimeS1List = [0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f];
				StockMaxS1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				SetSkillData();
			}
			else if (HoldShirayuki_Shuriken) {
				IconName = "Shirayuki_Shuriken";
				HowManySkills = 1;
				SkillLevel = [10, 10, 10];
				ChargeTypeIsPerSecond = [false, true, false];
				AutoTrigger = [true, false, false];
				ShowSummonIconBySkills = [false, false, false];

				InitialSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				MaxSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				SkillActiveTimeS1List = [0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f];
				StockMaxS1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				SetSkillData();
			}
			else if (HoldLava_Dagger) {
				IconName = "Lava_Dagger";
				HowManySkills = 1;
				SkillLevel = [7, 7, 7];
				ChargeTypeIsPerSecond = [false, true, false];
				AutoTrigger = [true, false, false];
				ShowSummonIconBySkills = [false, false, false];

				InitialSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				MaxSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				SkillActiveTimeS1List = [0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f];
				StockMaxS1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				SetSkillData();
			}
			else if (HoldKroosAlterCrossbow) {
				IconName = "KroosAlterCrossbow";
				HowManySkills = 1;
				SkillLevel = [10, 10, 10];
				ChargeTypeIsPerSecond = [false, true, false];
				AutoTrigger = [true, false, false];
				ShowSummonIconBySkills = [false, false, false];

				InitialSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				MaxSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				SkillActiveTimeS1List = [0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f];
				StockMaxS1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				SetSkillData();
			}
			else if (HoldExusiaiVector) {
				IconName = "ExusiaiVector";
				HowManySkills = 3;
				SkillLevel = [10, 10, 10];
				ChargeTypeIsPerSecond = [false, true, false];
				AutoTrigger = [true, false, false];
				ShowSummonIconBySkills = [false, false, false];

				InitialSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				InitialSPs2List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 25];
				InitialSPs3List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 20];
				MaxSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 4];
				MaxSPs2List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 35];
				MaxSPs3List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 30];
				SkillActiveTimeS1List = [0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.2f];
				SkillActiveTimeS2List = [0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 15f];
				SkillActiveTimeS3List = [0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 15f];
				StockMaxS1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 1];
				StockMaxS2List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 1];
				StockMaxS3List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 1];
				SetSkillData();
			}
			else if (HoldPozemkaCrossbow) {
				IconName = "PozemkaCrossbow";
				HowManySkills = 3;
				SkillLevel = [10, 10, 10];
				ChargeTypeIsPerSecond = [false, true, true];
				AutoTrigger = [true, false, false];
				ShowSummonIconBySkills = [true, true, true];

				InitialSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
				InitialSPs2List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 9];
				InitialSPs3List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 23];
				MaxSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 20];
				MaxSPs2List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 9];
				MaxSPs3List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 35];
				SkillActiveTimeS1List = [0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 30f];
				SkillActiveTimeS2List = [0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.4f];
				SkillActiveTimeS3List = [0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 30f];
				StockMaxS1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 1];
				StockMaxS2List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 2];
				StockMaxS3List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 1];
				SetSkillData();
			}
			else if (HoldNianWeapon) {
				IconName = "NianWeapon";
				HowManySkills = 3;
				SkillLevel = [10, 10, 10];
				ChargeTypeIsPerSecond = [true, true, true];
				AutoTrigger = [false, false, false];
				ShowSummonIconBySkills = [false, false, false];

				InitialSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 10];
				InitialSPs2List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 35];
				InitialSPs3List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 70];
				MaxSPs1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 30];
				MaxSPs2List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 50];
				MaxSPs3List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 85];
				SkillActiveTimeS1List = [0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 30f];
				SkillActiveTimeS2List = [0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 35f];
				SkillActiveTimeS3List = [0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 45f];
				StockMaxS1List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 1];
				StockMaxS2List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 1];
				StockMaxS3List = [0, 0, 0, 0, 0, 0, 0, 0, 0, 1];
				SetSkillData();
			}

			else if (HoldTyphonBow) {
				IconName = "TyphonBow";
				HowManySkills = 3;
				SkillLevel = new() { 10, 10, 10 };
				ChargeTypeIsPerSecond = new() { true, true, true };
				AutoTrigger = new() { false, false, false };
				ShowSummonIconBySkills = new() { false, false, false };
				InitialSPs1List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 15 };
				InitialSPs2List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 42 };
				InitialSPs3List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 25 };
				MaxSPs1List     = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 35 };
				MaxSPs2List     = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 50 };
				MaxSPs3List     = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 40 };
				SkillActiveTimeS1List = new() { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 35f };
				SkillActiveTimeS2List = new() { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 20f };
				SkillActiveTimeS3List = new() { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 30f };
				StockMaxS1List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
				StockMaxS2List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
				StockMaxS3List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
				SetSkillData();
			}
			else if (HoldSchwarzBow) {
				IconName = "SchwarzBow";
				HowManySkills = 3;
				SkillLevel = new() { 10, 10, 10 };
				ChargeTypeIsPerSecond = new() { false, true, true };
				AutoTrigger = new() { true, false, false };
				ShowSummonIconBySkills = new() { false, false, false };

				InitialSPs1List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
				InitialSPs2List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 20 };
				InitialSPs3List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 12 };
				MaxSPs1List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 };
				MaxSPs2List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 30 };
				MaxSPs3List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 25 };
				SkillActiveTimeS1List = new() { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0.4f };
				SkillActiveTimeS2List = new() { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 40f };
				SkillActiveTimeS3List = new() { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 25f };
				StockMaxS1List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
				StockMaxS2List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
				StockMaxS3List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
				SetSkillData();
			}

			else if (HoldHazeMagicBook) {
				IconName = "HazeMagicBook";
				HowManySkills = 2;
				SkillLevel = new() { 10, 10, 10 };
				ChargeTypeIsPerSecond = new() { true, true, true };
				AutoTrigger = new() { false, false, false };
				ShowSummonIconBySkills = new() { false, false, false };
				InitialSPs1List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 15 };//夜烟这里的所有技能数据我都瞎填的
				InitialSPs2List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 42 };
				InitialSPs3List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 25 };
				MaxSPs1List     = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 35 };
				MaxSPs2List     = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 50 };
				MaxSPs3List     = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 40 };
				SkillActiveTimeS1List = new() { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 35f };
				SkillActiveTimeS2List = new() { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 20f };
				SkillActiveTimeS3List = new() { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 30f };
				StockMaxS1List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
				StockMaxS2List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
				StockMaxS3List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
				SetSkillData();
			}

			else if (HoldSurtrLaevatain) {
				IconName = "SurtrLaevatain";
				HowManySkills = 2;
				SkillLevel = new() { 10, 10, 10 };
				ChargeTypeIsPerSecond = new() { false, true, true };
				AutoTrigger = new() { true, false, false };
				ShowSummonIconBySkills = new() { false, false, false };
				InitialSPs1List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 15 };//42这里的所有技能数据我都瞎填的
				InitialSPs2List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 42 };
				InitialSPs3List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 25 };
				MaxSPs1List     = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 35 };
				MaxSPs2List     = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 50 };
				MaxSPs3List     = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 40 };
				SkillActiveTimeS1List = new() { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 35f };
				SkillActiveTimeS2List = new() { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 20f };
				SkillActiveTimeS3List = new() { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 30f };
				StockMaxS1List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
				StockMaxS2List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
				StockMaxS3List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
				SetSkillData();
			}

			else if (HoldJessicaGun) {
				IconName = "JessicaGun";
				HowManySkills = 2;
				SkillLevel = new() { 10, 10, 10 };
				ChargeTypeIsPerSecond = new() { false, true, false };
				AutoTrigger = new() { false, false, false };
				ShowSummonIconBySkills = new() { false, false, false };

				InitialSPs1List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
				InitialSPs2List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
				MaxSPs1List     = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
				MaxSPs2List     = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
				SkillActiveTimeS1List = new() { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
				SkillActiveTimeS2List = new() { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
				StockMaxS1List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
				StockMaxS2List = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
				SetSkillData();
			}

			if (HowManySkills > 0) {
				SetSkill(Skill);
			}
		}
	}
}
