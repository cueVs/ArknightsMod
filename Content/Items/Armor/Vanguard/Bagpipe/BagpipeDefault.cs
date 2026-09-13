using System.Collections.Generic;
using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.Vanguard.Bagpipe;

namespace ArknightsMod.Content.Items.Armor.Vanguard.Bagpipe
{
	public class BagpipeDefault : ArknightsVanityBag
	{
		public override int Rarity => 6;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<BagpipeHead>(),
				ModContent.ItemType<BagpipeBody>(),
				ModContent.ItemType<BagpipeLegs>()
			];
		}
	}
}
