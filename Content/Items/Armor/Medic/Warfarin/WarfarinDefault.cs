using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Medic.Warfarin
{
	internal class WarfarinDefault:ArknightsVanityBag
	{
		public override int Rarity => 5;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<WarfarinHead>(),
				ModContent.ItemType<WarfarinBody>(),
				ModContent.ItemType<WarfarinLegs>()
			];
		}
	}
}
