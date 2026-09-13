using System.Collections.Generic;
using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.Supporter.Orchid;

namespace ArknightsMod.Content.Items.Armor.Supporter.Orchid
{
	public class OrchidDefault : ArknightsVanityBag
	{
		public override int Rarity => 3;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<OrchidHead>(),
				ModContent.ItemType<OrchidBody>(),
				ModContent.ItemType<OrchidLegs>()
			];
		}
	}
}
