using Terraria;
using Terraria.ModLoader;
using ArknightsMod.Players;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l2
{
	public class VanillaSauceSoda : ModItem
	{
		public override void SetStaticDefaults() {

		}

		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 24;
			Item.accessory = true;
			Item.value = Item.sellPrice(0, 3, 0, 0);
			Item.rare =1;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetModPlayer<WeaponPlayer>().SPRegenMultiplier += 1.2f;
		}


	}
}