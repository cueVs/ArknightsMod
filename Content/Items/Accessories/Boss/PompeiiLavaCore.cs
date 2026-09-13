using ArknightsMod.Content.Projectiles.Accessories;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Boss
{
	// 庞贝的熔岩核心 —— 饰品。
	//   - 防御力 +2
	//   - 获得黑曜石皮药水效果（免疫岩浆与火焰块，免疫"着火了!"）
	//   - 单次受到伤害超过 PompeiiLavaCorePlayer.ExplosionThreshold 点时，
	//     以自身为中心引爆一次范围伤害，并对命中的敌人附加"着火了!"；
	//     每次触发后有 PompeiiLavaCorePlayer.ExplosionCooldown 帧（5 秒）冷却
	public class PompeiiLavaCore : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults() {
			Item.width = 36;
			Item.height = 38;
			Item.accessory = true;
			Item.defense = 2;
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 5);
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			// 黑曜石皮药水（BuffID.ObsidianSkin）在原版里就是设置这三个字段，
			// 这里直接复刻同样的效果，好处是不占用一个 buff 槽、也不会被"清除buff"类效果驱散。
			// 若希望玩家能在 buff 栏看到图标，可改成 player.AddBuff(BuffID.ObsidianSkin, 2)。
			player.lavaImmune = true;                     // 岩浆免疫
			player.fireWalk = true;                       // 火焰块（地狱石/陨石）免疫
			player.buffImmune[BuffID.OnFire] = true;      // 免疫"着火了!"

			player.GetModPlayer<PompeiiLavaCorePlayer>().Equipped = true;
		}
	}

	public class PompeiiLavaCorePlayer : ModPlayer
	{
		// 触发爆炸所需的单次伤害阈值（点）。
		public const int ExplosionThreshold = 40;

		// 爆炸基础伤害（未计入近战加成）。
		public const int BaseExplosionDamage = 200;

		// 触发一次爆炸后的冷却时间（帧）。60 帧 = 1 秒，300 帧 = 5 秒。
		public const int ExplosionCooldown = 60 * 5;

		public bool Equipped;

		// 冷却计时器：>0 时不允许再次触发。
		// 注意不要放在 ResetEffects 里清零——ResetEffects 每帧都会重新执行，
		// 会导致这个计时器刚被减到还没归零就又被清成 0，冷却形同虚设。
		// 递减放在 PostUpdate 里，且不依赖 Equipped（脱下后冷却继续走，
		// 而不是脱下再穿上就直接刷新成可用状态）。
		private int cooldownTimer;

		public override void ResetEffects() {
			Equipped = false;
		}

		public override void PostUpdate() {
			if (cooldownTimer > 0)
				cooldownTimer--;
		}

		public override void OnHurt(Player.HurtInfo info) {
			if (!Equipped)
				return;

			if (cooldownTimer > 0)
				return;

			// info.Damage      = 防御减免「后」实际掉的血
			// info.SourceDamage = 防御减免「前」的原始伤害
			// 这里按"实际受到的伤害"判定：防御越高越不容易触发，符合"被打重了才炸"的直觉。
			// 若策划要按原始伤害判定（高防玩家也能稳定触发），把下面这行换成 info.SourceDamage 即可。
			if (info.Damage <= ExplosionThreshold)
				return;

			cooldownTimer = ExplosionCooldown;

			// 弹幕由本地玩家生成后自动联机同步，其他客户端不要重复生成
			if (Player.whoAmI != Main.myPlayer)
				return;

			// 「爆炸伤害享受近战伤害加成」：手动生成的弹幕不会自动套用伤害类加成，
			// 需要在这里显式用玩家的近战加成把基础伤害算出来。
			// 弹幕本身的 DamageType 也设为 Melee（见弹幕类），那只影响暴击与近战专属钩子，
			// 不会再乘一次加成，所以不存在双重计算。
			int damage = (int)Player.GetDamage(DamageClass.Melee).ApplyTo(BaseExplosionDamage);

			Projectile.NewProjectile(
				Player.GetSource_Accessory(new Item(ModContent.ItemType<PompeiiLavaCore>()), "PompeiiLavaCore"),
				Player.Center,
				Vector2.Zero,
				ModContent.ProjectileType<PompeiiLavaCoreExplosion>(),
				damage,
				0f,
				Player.whoAmI);
		}
	}
}
