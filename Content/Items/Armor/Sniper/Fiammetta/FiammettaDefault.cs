using ArknightsMod.Content.Items.Armor.Sniper.Fiammetta;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.Fiammetta
{
	public class FiammettaDefault : ArknightsVanityBag
	{
		public override int Rarity => 6;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<FiammettaHead>(),
				ModContent.ItemType<FiammettaBody>(),
				ModContent.ItemType<FiammettaLegs>()
			];
		}
	}
}
