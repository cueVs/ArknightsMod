using ArknightsMod.Content.Items.Armor.Guard.Lappland;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Lappland
{
	internal class LapplandDefault: ArknightsVanityBag
	{
		public override int Rarity => 5;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<LapplandHead>(),
				ModContent.ItemType<LapplandBody>(),
				ModContent.ItemType<LapplandLegs>()
			];
		}
	}
}
