using System.Collections.Generic;
using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.Defender.Cardigan;

namespace ArknightsMod.Content.Items.Armor.Defender.Cardigan
{
	public class CardiganDefault : ArknightsVanityBag
	{
		public override int Rarity => 3;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<CardiganHead>(),
				ModContent.ItemType<CardiganBody>(),
				ModContent.ItemType<CardiganLegs>()
			];
		}
	}
}
