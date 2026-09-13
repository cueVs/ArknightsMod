using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Defender.Spot
{
	public class SpotDefault : ArknightsVanityBag
	{
		public override int Rarity => 3;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<SpotHead>(),
				ModContent.ItemType<SpotBody>(),
				ModContent.ItemType<SpotLegs>()
			];
		}
	}
}
