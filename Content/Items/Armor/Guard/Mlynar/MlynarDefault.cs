using System.Collections.Generic;
using ArknightsMod.Content.Items.Armor.Guard.Mlynar;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Mlynar
{
	public class MlynarDefault : ArknightsVanityBag
	{
		public override int Rarity => 6;
		protected override List<int> GetItems()
		{
			return
			[
				ModContent.ItemType<MlynarHead>(),
				ModContent.ItemType<MlynarBody>(),
				ModContent.ItemType<MlynarLegs>(),
			];
		}
	}
}
