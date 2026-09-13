using System.Collections.Generic;
using ArknightsMod.Content.Items.Armor.Vanguard.Texas;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Vanguard.Texas
{
	public class TexasDefault : ArknightsVanityBag
	{
		public override int Rarity => 5;
		protected override List<int> GetItems()
		{
			return
			[
				ModContent.ItemType<TexasHead>(),
				ModContent.ItemType<TexasBody>(),
				ModContent.ItemType<TexasLegs>(),
			];
		}
	}
}
