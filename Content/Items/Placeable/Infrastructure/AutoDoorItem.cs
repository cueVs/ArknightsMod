using ArknightsMod.Content.Tiles.Infrastructure;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure
{
	public class AutoDoorItem : ModItem
	{
		// 暂时使用关门贴图作为物品图标
		public override string Texture => "ArknightsMod/Assets/Tiles/Infrastructure/AutoDoor_Close";

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<AutoDoorClosedTile>());
			Item.width    = 14;
			Item.height   = 28;
			Item.value    = Item.sellPrice(0, 0, 5, 0);
			Item.rare     = ItemRarityID.White;
			Item.maxStack = 99;
		}
	}
}
