using System.Collections.Generic;
using Terraria.ModLoader;


namespace ArknightsMod.Content.Items.Armor.Defender.Beagle
{
	internal class BeagleDefault : ArknightsVanityBag
	{
		public override int Rarity => 3;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<BeagleHead>(),
				ModContent.ItemType<BeagleBody>(),
				ModContent.ItemType<BeagleLegs>()
			];
		}
	}
}
