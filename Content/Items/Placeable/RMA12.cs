﻿using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable
{
	public class RMA12 : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;

			// This ore can spawn in slime bodies like other pre-boss ores. (copper, tin, iron, etch)
			// It will drop in amount from 3 to 13.
			// ItemID.Sets.OreDropsFromSlime[Type] = (3, 13);
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.MnOre>());
			Item.width = 26;
			Item.height = 26;
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}