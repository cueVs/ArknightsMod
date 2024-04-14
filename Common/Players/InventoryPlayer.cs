using ArknightsMod.Content.Items;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Common.Players
{
	public class InventoryPlayer : ModPlayer
	{
		public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath) {
			if (mediumCoreDeath) {
				return new[] {
					new Item(ModContent.ItemType<Content.Items.Consumables.StartBag>()),
					new Item(ModContent.ItemType<Content.Items.Placeable.Furniture.AnniversaryWheel>())
				};
			}
			return new[] {
				new Item(ModContent.ItemType<Content.Items.Consumables.StartBag>()),
				new Item(ModContent.ItemType<Content.Items.Placeable.Furniture.AnniversaryWheel>())
			};
		}
	}
}