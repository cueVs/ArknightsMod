using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l4
{
	public class ScoutsScope : ModItem
	{
		public override void SetDefaults() {
			Item.width = 30;
			Item.height = 30;
			Item.value = Item.sellPrice(0, 16, 0, 0);
			Item.rare = ItemRarityID.Master;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<ScoutsScopePlayer>().hasScopeAccessory = true;
		}
	}

	public class ScoutsScopePlayer : ModPlayer
	{
		public bool hasScopeAccessory = false;

		public override void ResetEffects() {
			hasScopeAccessory = false;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (!hasScopeAccessory)
				return;
			float distance = Vector2.Distance(Player.Center, target.Center);
			if (distance >= 800f) {
				modifiers.FinalDamage *= 2f;
				return;
			}
			float multiplier = 1f + (distance / 800f);
			modifiers.FinalDamage *= multiplier;
		}
	}
}