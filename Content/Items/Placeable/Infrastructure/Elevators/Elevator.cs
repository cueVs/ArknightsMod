using ArknightsMod.Content.Tiles.Infrastructure.Elevators;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Elevators
{
	public class Elevator : ModItem
	{
		// 我求求你们别再用中文类名了好吗好的
		public override void SetDefaults()
		{
			Item.width = 16;
			Item.height = 16;
			Item.maxStack = 99;

			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 10;
			Item.useAnimation = 15;
			Item.useTurn = true;
			Item.autoReuse = true;

			Item.consumable = true;
			Item.value = Item.buyPrice(silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.createTile = ModContent.TileType<ElevatorTile>();
		}
	}
}
