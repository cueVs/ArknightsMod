using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content.Projectiles.Rogue.FireworksHand;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l4
{
	public class FireworksHand : ModItem
	{
		public override void SetDefaults() {
			Item.width = 30;
			Item.height = 30;
			Item.accessory = true;
			Item.value = Item.sellPrice(0, 16, 0, 0);
			Item.rare = ItemRarityID.Master;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {

			player.GetModPlayer<FireworksHandPlayer>().EnableFireworksHand = true;
		}
	}

	public class FireworksHandPlayer : ModPlayer
	{
		public bool EnableFireworksHand = false;

		public override void ResetEffects() {
			EnableFireworksHand = false;
		}

		public override void OnConsumeMana(Item item, int manaConsumed) {

		}

		public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {

			if (!EnableFireworksHand)
				return base.Shoot(item, source, position, velocity, type, damage, knockback);


			bool isRanged = item.CountsAsClass(DamageClass.Ranged) || item.useAmmo != AmmoID.None;

			if (isRanged && Main.rand.NextBool(4)) // 25%几率
			{
				int damagef = damage * 2;
				float angleOffset = Main.rand.NextFloat(-0.05f, 0.05f);
				Vector2 newVelocity = velocity.RotatedBy(angleOffset);

	
				Vector2 offsetPos = Main.rand.NextVector2Circular(8f, 8f);
				Vector2 newPosition = position + offsetPos;

				// 发射额外的弹幕
				Projectile.NewProjectile(
					source,
					newPosition,
					newVelocity,
					ModContent.ProjectileType<FireworksHandProj>(),
					damagef,
					knockback,
					Player.whoAmI
				);
			}

			return base.Shoot(item, source, position, velocity, type, damage, knockback);
		}
	}
}