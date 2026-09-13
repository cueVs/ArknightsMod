using ArknightsMod.Content.Items.Armor.Supporter.Radian;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Supporter.Radian
{
	public class RaidianDefault : ArknightsVanityBag
	{
		public override int Rarity => 6;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<RaidianHead>(),
				ModContent.ItemType<RaidianBody>(),
				ModContent.ItemType<RaidianLegs>()
			];
		}
	}
}
