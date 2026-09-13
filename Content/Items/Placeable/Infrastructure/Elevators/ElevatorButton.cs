using ArknightsMod.Content.Tiles.Infrastructure.Elevators;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Elevators
{
	public class ElevatorButton : ModItem
	{
		// 我求求你们别再用中文类名了好吗好的
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 48;
			Item.maxStack = 99;

			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 10;
			Item.useAnimation = 15;
			Item.useTurn = true;
			Item.autoReuse = true;

			Item.consumable = true;
			Item.value = Item.buyPrice(silver: 10);
			Item.rare = ItemRarityID.Blue;
			Item.createTile = ModContent.TileType<ElevatorButtonTile>();
		}
	}
}
