using System.Collections.Generic;
using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.Medic.Ansel;

namespace ArknightsMod.Content.Items.Armor.Medic.Ansel
{
	public class AnselDefault : ArknightsVanityBag
	{
		public override int Rarity => 3;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<AnselHead>(),
				ModContent.ItemType<AnselBody>(),
				ModContent.ItemType<AnselLegs>()
			];
		}
	}
}
