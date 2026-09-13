using ArknightsMod.Content.Items.Armor.Guard.Melantha;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Melantha
{
	public class MelanthaDefault : ArknightsVanityBag
	{
		public override int Rarity => 3;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<MelanthaHead>(),
				ModContent.ItemType<MelanthaBody>(),
				ModContent.ItemType<MelanthaLegs>()
			];
		}
	}
}
