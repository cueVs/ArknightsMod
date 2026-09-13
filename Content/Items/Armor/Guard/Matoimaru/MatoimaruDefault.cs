using ArknightsMod.Content.Items.Armor.Guard.Matoimaru;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Matoimaru
{
	public class MatoimaruDefault : ArknightsVanityBag
	{
		public override int Rarity => 4;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<MatoimaruHead>(),
				ModContent.ItemType<MatoimaruBody>(),
				ModContent.ItemType<MatoimaruLegs>()
			];
		}
	}
}
