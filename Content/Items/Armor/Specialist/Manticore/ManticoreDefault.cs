using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Specialist.Manticore
{
	internal class ManticoreDefault:ArknightsVanityBag
	{
		public override int Rarity => 5;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<ManticoreHead>(),
				ModContent.ItemType<ManticoreBody>(),
				ModContent.ItemType<ManticoreLegs>()
			];
		}
	}
}
