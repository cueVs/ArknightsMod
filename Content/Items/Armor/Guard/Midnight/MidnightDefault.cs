using System.Collections.Generic;
using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.Guard.Midnight;

namespace ArknightsMod.Content.Items.Armor.Guard.Midnight
{
	public class MidnightDefault : ArknightsVanityBag
	{
		public override int Rarity => 3;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<MidnightHead>(),
				ModContent.ItemType<MidnightBody>(),
				ModContent.ItemType<MidnightLegs>()
			];
		}
	}
}
