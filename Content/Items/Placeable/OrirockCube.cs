﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable
{
	public class OrirockCube : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;

			// This ore can spawn in slime bodies like other pre-boss ores. (copper, tin, iron, etch)
			// It will drop in amount from 3 to 13.
			// ItemID.Sets.OreDropsFromSlime[Type] = (3, 13);
		}

		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.OrirockCube>());
			Item.width = 42;
			Item.height = 42;
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}