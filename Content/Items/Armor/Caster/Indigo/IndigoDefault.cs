using ArknightsMod.Content.Items.Armor.Caster.Indigo;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Caster.Indigo
{
	public class IndigoDefault : ArknightsVanityBag
	{
		public override int Rarity => 4;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<IndigoHead>(),
				ModContent.ItemType<IndigoBody>(),
				ModContent.ItemType<IndigoLegs>()
			];
		}
	}
}
