using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ArknightsMod.Common.Damageclasses
{
	public class SpellDamage : DamageClass
	{
		// 是否使用标准暴击计算（默认true）
		public override bool UseStandardCritCalcs => true;

		// 伤害类型名称（用于本地化）
	}
		public class SpellDamageConfig : ModSystem
	{
		// 存储所有法术武器和弹幕（静态可全局访问）
		public static HashSet<int> SpellWeapons { get; } = new() {
			

		};
		public static HashSet<int> SpellProjectiles { get; } = new() {
			

		
			//法师
			ProjectileID.AmethystBolt,
			ProjectileID.DiamondBolt,
			ProjectileID.EmeraldBolt,
			ProjectileID.RubyBolt,
			ProjectileID.SapphireBolt,
			ProjectileID.TopazBolt,
			ProjectileID.MagicMissile,//魔法导弹
			ProjectileID.ThunderStaffShot,
			ProjectileID.LastPrismLaser,
			ProjectileID.NebulaBlaze1,
			ProjectileID.NebulaBlaze2,
			ProjectileID.Blizzard,
			ProjectileID.WaterStream,
			ProjectileID.GreenLaser,
			ProjectileID.WaterBolt,
			ProjectileID.WandOfFrostingFrost,
			ProjectileID.WandOfSparkingSpark,
			ProjectileID.SpiritFlame,
			ProjectileID.VilethornBase,//魔刺
			ProjectileID.VilethornTip,//魔刺
			ProjectileID.BallofFrost,
			//战士
			ProjectileID.PurpleLaser,//激光步枪
			ProjectileID.IceBolt,//冰雪刃
			ProjectileID.EnchantedBoomerang,
			ProjectileID.EnchantedBeam,
			ProjectileID.NightsEdge,//永夜刃
			ProjectileID.FrostBoltSword,
			ProjectileID.Cascade,//喷琉球
			ProjectileID.HiveFive,//蜂巢球
			ProjectileID.Starfury,//星怒
			ProjectileID.LightsBane,//魔光
			//射手
			ProjectileID.BeeArrow,//蜜蜂箭
			ProjectileID.Hellwing,//地狱之翼
			ProjectileID.JestersArrow,
			ProjectileID.MagicDagger,//魔法飞刀			
			ProjectileID.DemonScythe,//恶魔镰刀
			ProjectileID.StarCannonStar,
			ProjectileID.FairyQueenRangedItemShot,
			ProjectileID.BlackBolt,
			ProjectileID.FrostArrow,
			//召唤
			ProjectileID.DD2FlameBurstTowerT1Shot,
			ProjectileID.DD2FlameBurstTowerT2Shot,
			ProjectileID.DD2FlameBurstTowerT3Shot,//艾特你丫喷火
			ProjectileID.DD2LightningAuraT1,
			ProjectileID.DD2LightningAuraT2,
			ProjectileID.DD2LightningAuraT3,
			ProjectileID.HoundiusShootiusFireball,
			ProjectileID.BabySlime,
			ProjectileID.AbigailMinion,//阿比
			ProjectileID.MiniRetinaLaser,//小激光眼的激光
			ProjectileID.ImpFireball,

			




		};

		// 添加带自动标记的注册方法
		public static void RegisterSpellWeapon(int itemId, bool addTooltip = true) {
			SpellWeapons.Add(itemId);
			if (addTooltip)
				CachedTooltipItems.Add(itemId);
		}

		public static void RegisterSpellProjectile(int projectileId) {
			SpellProjectiles.Add(projectileId);
		}

		// 缓存需要添加提示的物品（优化性能）
		private static HashSet<int> CachedTooltipItems { get; } = new();

		// 检查物品是否需要显示法术提示
		public static bool ShouldAddSpellTooltip(Item item) {
			return CachedTooltipItems.Contains(item.type) ||
				   (item.shoot > 0 && SpellProjectiles.Contains(item.shoot));
		}
	}
	public class SpellTooltipGlobalItem : GlobalItem
	{
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			// 检查是否是发射法术弹幕的武器
			if (item.shoot > 0 && SpellDamageConfig.SpellProjectiles.Contains(item.shoot)) {
				// 添加自定义工具提示行
				tooltips.Add(new TooltipLine(Mod, "SpellDamageNote", "弹幕造成法术伤害") {
					OverrideColor = Color.Blue // 可选：设置颜色
				});
			}
		}
		private void AddSpellDamageTooltip(Item item, List<TooltipLine> tooltips) {
			// 查找或创建合适的插入位置
			int insertIndex = tooltips.FindLastIndex(t => t.Name.StartsWith("Tooltip")) + 1;
			if (insertIndex <= 0)
				insertIndex = tooltips.Count;

			// 创建统一格式的提示
			var spellLine = new TooltipLine(Mod, "SpellDamage",
				GetSpellDamageText(item));

			// 设置绿色文本并插入
			spellLine.OverrideColor = new (100, 255, 100);
			tooltips.Insert(insertIndex, spellLine);
		}

		private string GetSpellDamageText(Item item) {
			bool isPureSpell = item.DamageType == DamageClass.Magic;
			bool hasProjectile = item.shoot > 0 && SpellDamageConfig.SpellProjectiles.Contains(item.shoot);

			if (isPureSpell && hasProjectile)
				return "◇ 造成纯粹法术伤害";
			else if (hasProjectile)
				return "◇ 弹幕造成法术伤害";
			else
				return "◇ 攻击附带法术伤害";
		}
	}
}
