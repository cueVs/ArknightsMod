using System;
using System.Collections.Generic;
using ArknightsMod.Players;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using ArknightsMod.Content.Items.Weapons.Caster._12F;
using ArknightsMod.Content.Items.Weapons.Caster.Durin;
using ArknightsMod.Content.Items.Weapons.Caster.Goldenglow;
using ArknightsMod.Content.Items.Weapons.Caster.Haze;
using ArknightsMod.Content.Items.Weapons.Caster.Lava;
using ArknightsMod.Content.Items.Weapons.Caster.Steward;
using ArknightsMod.Content.Items.Weapons.Caster.Valarqvin;
using ArknightsMod.Content.Items.Weapons.Defender.Durnar;
using ArknightsMod.Content.Items.Weapons.Defender.Nian;
using ArknightsMod.Content.Items.Weapons.Guard.Chen;
using ArknightsMod.Content.Items.Weapons.Guard.Mousse;
using ArknightsMod.Content.Items.Weapons.Guard.Oblivionis;
using ArknightsMod.Content.Items.Weapons.Guard.Surtr;
using ArknightsMod.Content.Items.Weapons.Medic.ReedFlameShadow;
using ArknightsMod.Content.Items.Weapons.Sniper.Shirayuki;
using ArknightsMod.Content.Items.Weapons.Specialist.Scene;
using ArknightsMod.Content.Items.Weapons.Specialist.TexasAlter;
using ArknightsMod.Content.Items.Weapons.Supporter.Beanstalk;
using ArknightsMod.Content.Items.Weapons.Supporter.Deepcolor;
using ArknightsMod.Content.Items.Weapons.Supporter.Earthspirit;
using ArknightsMod.Content.Items.Weapons.Supporter.Orchid;
using ArknightsMod.Content.Items.Weapons.Supporter.Pramanix;
using ArknightsMod.Content.Items.Weapons.Vanguard.Texas;

namespace ArknightsMod.Systems.Gameplay.Damage
{
	/// <summary>
	/// 一把本模组武器的法伤规则。两个判定分开写是因为同一把武器的"直接命中"和"打出的弹幕"
	/// 未必同时算法伤（例如稀音的摄像机：普攻弹幕算法伤，技能召唤的摄影车不算）。
	/// </summary>
	public sealed class ArtsWeaponRule
	{
		/// <summary>物品直接命中（近战挥砍/戳刺，不经过弹幕）算不算法伤。</summary>
		public Func<Player, bool> DirectHit = _ => false;

		/// <summary>这把武器打出的某一发弹幕算不算法伤。在弹幕<b>生成那一刻</b>求值，见 ArtsProjectileMarker。</summary>
		public Func<Player, Projectile, bool> ProjectileHit = (_, _) => false;
	}

	/// <summary>
	/// 法术伤害登记表。
	///
	/// 分成两套是因为原版武器和本模组武器的判定方式根本不同：
	/// ● 原版武器：伤害类型固定不变，按物品/弹幕 ID 查表就够了。
	/// ● 本模组武器：法伤与否经常取决于"当前选的哪个技能、技能开没开、打出的是哪一发弹幕"，
	///   光看类型分不出来——比如地灵的法杖直接借用原版的 <see cref="ProjectileID.MagicMissile"/> 当普攻弹幕，
	///   要是把这个弹幕 ID 全局登记成法伤，连原版的魔法飞弹都会跟着变成法伤；白雪的手里剑
	///   普攻和二技能共用同一种弹幕，只有二技能期间才算法伤。所以模组武器改成
	///   "按手持武器查规则 + 规则里带条件判断"。
	/// </summary>
	public static class ArtsWeaponRegistry
	{
		// ── 原版武器（伤害类型固定，按 ID 查表）────────────────────────────────

		// 只有这 4 件在原版 Item.SetDefaults 里没设 noMelee=true——它们的挥砍本身就有伤害判定，
		// 除了下面 VanillaArtsProjectileTypes 里各自对应的剑气/弹幕以外，物品本身的挥砍命中也要算。
		// （核对方式：反编译源码里逐个查了 case 段的 noMelee/shoot 字段，不是猜的。）
		public static readonly HashSet<int> VanillaArtsItemTypes = new() {
			ItemID.InfluxWaver,    // 波涌之刃
			ItemID.LightsBane,     // 魔光剑
			ItemID.EnchantedSword, // 附魔剑
			ItemID.Frostbrand,     // 寒霜剑（霜印剑）
		};

		// 现代 Terraria 里大部分"剑挥出去"的伤害其实是这把剑自己生成的弹幕在打，不是物品自带的
		// 近战碰撞框（noMelee=true）。下面这些武器基本都属于这一类，弹幕名字经常和物品名不一样
		// （比如 附魔剑 打的是 EnchantedBeam，北极 打的是 NorthPoleWeapon），一样是查反编译源码核对的。
		public static readonly HashSet<int> VanillaArtsProjectileTypes = new() {
			ProjectileID.IceSickle,       // 冰雪镰刀（回旋镖类）
			ProjectileID.Meowmere,        // 彩虹喵之刃（纯法术弹幕）
			ProjectileID.CorruptYoyo,     // 抑郁球
			ProjectileID.Cascade,         // 喷流球
			ProjectileID.HelFire,         // 狱火球
			ProjectileID.Kraken,          // 克拉肯球
			ProjectileID.MushroomSpear,   // 蘑菇长矛（noMelee，纯弹幕）
			ProjectileID.DarkLance,       // 暗黑长枪（noMelee，纯弹幕）
			ProjectileID.NorthPoleWeapon, // 北极（noMelee，纯弹幕）
			ProjectileID.TheHorsemansBlade,// 无头骑士剑（noMelee，纯弹幕）
			ProjectileID.TrueNightsEdge,  // 真永夜刃（noMelee，纯弹幕）
			ProjectileID.NightsEdge,      // 永夜刃（noMelee，纯弹幕）
			ProjectileID.CobaltNaginata,  // 钴薙刀（noMelee，纯弹幕）
			ProjectileID.MonkStaffT2,     // 恐怖关刀（noMelee，纯弹幕）
			ProjectileID.LightsBane,      // 魔光剑的剑气（挥砍本身也算，见上）
			ProjectileID.InfluxWaver,     // 波涌之刃的光刃（挥砍本身也算，见上）
			ProjectileID.EnchantedBeam,   // 附魔剑的剑气（挥砍本身也算，见上）
			ProjectileID.FrostBoltSword,  // 寒霜剑的冰刃（挥砍本身也算，见上）
		};

		// ── 本模组武器（按手持武器查规则，规则里带条件）──────────────────────

		private static readonly Dictionary<int, ArtsWeaponRule> ModRules = new();

		/// <summary>取本模组武器的法伤规则；不是登记在案的法伤武器则返回 null。</summary>
		public static ArtsWeaponRule GetRule(int itemType) =>
			ModRules.TryGetValue(itemType, out ArtsWeaponRule rule) ? rule : null;

		internal static void Clear() => ModRules.Clear();

		/// <summary>整把武器的所有攻击都算法伤（术师、术战者、咒愈师这类）。</summary>
		private static void RegisterAlways<T>() where T : ModItem =>
			ModRules[ModContent.ItemType<T>()] = new ArtsWeaponRule {
				DirectHit     = _ => true,
				ProjectileHit = (_, _) => true,
			};

		private static void Register<T>(Func<Player, bool> directHit,
			Func<Player, Projectile, bool> projectileHit) where T : ModItem =>
			ModRules[ModContent.ItemType<T>()] = new ArtsWeaponRule {
				DirectHit     = directHit     ?? (_ => false),
				ProjectileHit = projectileHit ?? ((_, _) => false),
			};

		/// <summary>当前选中的是第 skillIndex 个技能（0 起）且技能正在生效中。</summary>
		private static bool SkillActive(Player player, int skillIndex) {
			WeaponPlayer wp = player.GetModPlayer<WeaponPlayer>();
			return wp.SkillActive && wp.Skill == skillIndex;
		}

		/// <summary>任意技能正在生效中（不区分是哪个）。</summary>
		private static bool AnySkillActive(Player player) =>
			player.GetModPlayer<WeaponPlayer>().SkillActive;

		internal static void BuildRules() {
			Clear();

			// ── 术师：职业特性，全部攻击都是法术伤害 ──
			RegisterAlways<_12FWand>();          // 12F的法杖
			RegisterAlways<DurinWand>();         // 杜林的法杖
			RegisterAlways<GoldenglowWand>();    // 澄闪的法杖
			RegisterAlways<HazeMagicBook>();     // 夜烟的魔法书
			RegisterAlways<Lava_Dagger>();       // 炎熔的匕首
			RegisterAlways<StewardStaff>();      // 史都华德的法杖
			RegisterAlways<ValarqvinWeapon>();   // 凛视的法杖
			RegisterAlways<BeanstalkOrb>();      // 远山的水晶球
			RegisterAlways<SaintBell>();         // 圣女的铃铛

			// ── 术战者 / 咒愈师：同样是全部攻击都算法伤 ──
			RegisterAlways<MousseGlove>();          // 慕斯的手套（术战者）
			RegisterAlways<SurtrLaevatain>();       // 史尔特尔的莱万汀（术战者）
			RegisterAlways<ReedFlameShadowStaff>(); // 焰影苇草的法杖（咒愈师）

			// ── 地灵的法杖：普攻是法伤，一技能是"强化普攻"也算法伤；
			//    二技能期间 CanUseItem 直接 return false 打不出攻击，所以实际打出去的
			//    每一下都属于上面两种，等价于"全部法伤"。 ──
			RegisterAlways<EarthspiritWand>();

			// ── 梓兰的伞：同上，普攻 + 一技能强化普攻都算法伤 ──
			RegisterAlways<OrchidUmbrellla>();

			// ── 坚雷的盾与警棍（驭法铁卫）：只有技能开启期间的普攻才是法伤 ──
			Register<DN_Weapon>(
				directHit:     AnySkillActive,
				projectileHit: (p, _) => AnySkillActive(p));

			// ── 白雪的手里剑：普攻和二技能共用同一种弹幕，只有二技能期间算法伤 ──
			Register<Shirayuki_Shuriken>(
				directHit:     null,
				projectileHit: (p, _) => SkillActive(p, 1));

			// ── 稀音的摄像机：只有普攻（左键打出的弹丸）是法伤；
			//    技能/右键召唤出来的摄影车是独立召唤物，不算。 ──
			int sceneCameraBullet = ModContent.ProjectileType<Content.Projectiles.Specialist.Scene.SceneCameraBullet>();
			Register<SceneCamera>(
				directHit:     null,
				projectileHit: (_, proj) => proj.type == sceneCameraBullet);

			// ── 深海色的速写：只有"普攻"算法伤。左右键对调后，普攻＝左键打出的 LOGO 攻击；
			//    右键部署的触手是独立召唤物，它自己打出的伤害不算。 ──
			int deepcolorLogo = ModContent.ProjectileType<Content.Projectiles.Supporter.Deepcolor.DeepcolorSketchLogoAttack>();
			Register<DeepcolorSketch>(
				directHit:     null,
				projectileHit: (_, proj) => proj.type == deepcolorLogo);

			// ── 下面这几把：按"对应技能生效期间，这把武器打出的全部伤害都算法伤"处理 ──

			// 陈的赤霄：二技能（下标 1）
			Register<ChenSword_Item>(
				directHit:     p => SkillActive(p, 1),
				projectileHit: (p, _) => SkillActive(p, 1));

			// 德克萨斯的剑：二技能（下标 1）。这把没有任何弹幕，全靠直接命中。
			Register<ArtsBlade>(
				directHit:     p => SkillActive(p, 1),
				projectileHit: (p, _) => SkillActive(p, 1));

			// 旋律之主双剑：三个技能都有法伤手段，任一技能生效期间都算
			Register<OblivionisSword>(
				directHit:     AnySkillActive,
				projectileHit: (p, _) => AnySkillActive(p));

			// 年的剑：一技能（下标 0）法伤、二技能（下标 1）反伤法伤
			Register<NianWeapon>(
				directHit:     p => SkillActive(p, 0) || SkillActive(p, 1),
				projectileHit: (p, _) => SkillActive(p, 0) || SkillActive(p, 1));

			// 缄默德克萨斯的蓝莓与黑巧：三个物品都只在技能生效期间算法伤，普攻不算
			Register<Blueberry>(
				directHit:     AnySkillActive,
				projectileHit: (p, _) => AnySkillActive(p));
			Register<DarkChocolate>(
				directHit:     AnySkillActive,
				projectileHit: (p, _) => AnySkillActive(p));
			Register<BlueberryDarkChocolate>(
				directHit:     AnySkillActive,
				projectileHit: (p, _) => AnySkillActive(p));
		}
	}

	/// <summary>
	/// 给每一发弹幕打上"这发算不算法术伤害"的标记。
	///
	/// 判定放在弹幕<b>生成那一刻</b>而不是命中时，有两个原因：
	/// 1. 命中时玩家可能已经换了武器、或技能已经结束，那时再回头查"当时是什么状态"就失真了——
	///    一发在二技能期间打出去的手里剑，飞行途中技能结束了也该照样算法伤。
	/// 2. 生成时能拿到"是谁打出来的"（手持武器），命中时拿不到。
	/// </summary>
	public class ArtsProjectileMarker : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		public bool IsArtsDamage;

		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			// 原版法伤武器：弹幕类型固定，直接查表。
			if (ArtsWeaponRegistry.VanillaArtsProjectileTypes.Contains(projectile.type)) {
				IsArtsDamage = true;
				return;
			}

			if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
				return;

			Player owner = Main.player[projectile.owner];
			if (!owner.active)
				return;

			ArtsWeaponRule rule = ArtsWeaponRegistry.GetRule(owner.HeldItem.type);
			if (rule != null)
				IsArtsDamage = rule.ProjectileHit(owner, projectile);
		}
	}

	/// <summary>登记表要用 ModContent.ItemType，只能等内容加载完再建。</summary>
	public class ArtsWeaponRegistrySystem : ModSystem
	{
		public override void PostSetupContent() => ArtsWeaponRegistry.BuildRules();

		public override void Unload() => ArtsWeaponRegistry.Clear();
	}
}
