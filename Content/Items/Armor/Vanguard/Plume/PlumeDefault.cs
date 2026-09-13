using System.Collections.Generic;
using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.Vanguard.Plume;

namespace ArknightsMod.Content.Items.Armor.Vanguard.Plume
{
	public class PlumeDefault : ArknightsVanityBag
	{
		public override int Rarity => 3;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<PlumeHead>(),
				ModContent.ItemType<PlumeBody>(),
				ModContent.ItemType<PlumeLegs>()
			];
		}
	}
}
