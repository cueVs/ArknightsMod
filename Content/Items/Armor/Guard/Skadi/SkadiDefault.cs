using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Skadi
{
	internal class SkadiDefault : ArknightsVanityBag
	{
		public override int Rarity => 6;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<SkadiHead>(),
				ModContent.ItemType<SkadiBody>(),
				ModContent.ItemType<SkadiLegs>()
			];
		}
	}
}
