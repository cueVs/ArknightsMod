using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content.Projectiles.Medic.ReedFlameShadow;
using ArknightsMod.Content.Tiles.Infrastructure;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Weapons.Medic.ReedFlameShadow
{
	// 焰影苇草的法杖 —— 医疗干员，火焰系普攻。
	// 目前只做普攻 + 特效，未接入技力条/技能系统；日后要接入的话把基类换成
	// UpgradeWeaponBase / ExpansionWeaponBase 即可，普攻逻辑不用动。
	public class ReedFlameShadowStaff : ModItem
	{
		public override void SetDefaults() {
			Item.width = 66;
			Item.height = 66;

			Item.damage = 42;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 8;
			Item.knockBack = 3.5f;
			Item.crit = 6;

			Item.useTime = 26;
			Item.useAnimation = 26;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.autoReuse = true;
			Item.noMelee = true;

			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = SoundID.Item34 with { Volume = 0.5f, Pitch = 0.35f };

			Item.shoot = ModContent.ProjectileType<ReedFlameShadowFlame>();
			Item.shootSpeed = 11f;
		}

		// 从杖头而不是玩家中心发射，观感上"火从杖尖出来"
		public override Vector2? HoldoutOffset() => new Vector2(-4f, -2f);

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
				Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			// 杖尖位置：沿瞄准方向从手部往外推一段
			Vector2 muzzle = position + Vector2.Normalize(velocity) * 34f;

			// 轻微散射，连发时不会每一发完全重叠
			Vector2 vel = velocity.RotatedByRandom(MathHelper.ToRadians(2.5f));

			Projectile.NewProjectile(source, muzzle, vel, type, damage, knockback, player.whoAmI);

			// 杖尖起手火花
			if (!Main.dedServ) {
				for (int i = 0; i < 8; i++) {
					Dust d = Dust.NewDustPerfect(muzzle,
						Main.rand.NextBool(3) ? DustID.GoldFlame : DustID.Torch,
						vel * Main.rand.NextFloat(0.05f, 0.25f) + Main.rand.NextVector2Circular(1.6f, 1.6f),
						100, default, Main.rand.NextFloat(0.9f, 1.6f));
					d.noGravity = true;
				}
			}

			return false;
		}

		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Orundum>(50)
			.AddIngredient<OrirockConcentration>(8)
			.AddIngredient<LoxicKohl>(6)
			.AddTile(ModContent.TileType<FactoryTile>())
			.Register();
		}
	}
}
