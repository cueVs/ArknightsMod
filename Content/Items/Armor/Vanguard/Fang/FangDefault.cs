using System.Collections.Generic;
using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.Vanguard.Fang;

namespace ArknightsMod.Content.Items.Armor.Vanguard.Fang
{
	public class FangDefault : ArknightsVanityBag
	{
		public override int Rarity => 3;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<FangHead>(),
				ModContent.ItemType<FangBody>(),
				ModContent.ItemType<FangLegs>()
			];
		}
	}
}
