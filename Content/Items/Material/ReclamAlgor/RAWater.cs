using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace ArknightsMod.Content.Items.Material.ReclamAlgor
{
	public class RAWater : ModItem
	{
		public override void SetStaticDefaults() {
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1000;
		}
		public override void SetDefaults() {
			Item.rare = ItemRarityID.Green;
			Item.height = 32;
			Item.width = 32;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
			Item.value = Item.sellPrice(6, 0, 0, 00);
		}
	}
}