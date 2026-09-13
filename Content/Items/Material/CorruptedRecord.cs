using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Material
{
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class CorruptedRecord : ModItem
	{
		public override void SetDefaults() {
			Item.rare = ItemRarityID.Blue;
			Item.height = 20;
			Item.width = 20;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;

		}
	}
}