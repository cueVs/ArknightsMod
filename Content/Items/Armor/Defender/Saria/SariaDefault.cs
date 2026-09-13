using ArknightsMod.Content.Items.Armor.Defender.Saria;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Defender.Saria
{
	internal class SariaDefault : ArknightsVanityBag
	{
		public override int Rarity => 6;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<SariaHead>(),
				ModContent.ItemType<SariaBody>(),
				ModContent.ItemType<SariaLegs>()
			];
		}
	}
}
