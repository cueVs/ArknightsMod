using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l4
{
	public class BlazeChainsaw : ModItem
	{
		public override void SetDefaults() {
			Item.width = 30;
			Item.height = 30;
			Item.value = Item.sellPrice(0, 16, 0, 0);
			Item.rare = ItemRarityID.Master;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<BlazeChainsawPlayer>().hasChainsawAccessory = true;
		}
	}

	public class BlazeChainsawPlayer : ModPlayer
	{
		public bool hasChainsawAccessory = false;

		public override void ResetEffects() {
			hasChainsawAccessory = false;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (!hasChainsawAccessory)
				return;

			float distance = Vector2.Distance(Player.Center, target.Center);

			if (distance >= 650f)
				return;

			float multiplier = distance <= 100f ? 2f :
				1f + (1f - (distance - 100f) / 550f);

			modifiers.FinalDamage *= multiplier;
		}
	}
}