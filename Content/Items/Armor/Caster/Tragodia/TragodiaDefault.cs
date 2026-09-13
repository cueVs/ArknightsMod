using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Caster.Tragodia
{
	public class TragodiaDefault : ArknightsVanityBag
	{
		public override int Rarity => 5;

		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<TragodiaHead>(),
				ModContent.ItemType<TragodiaBody>(),
				ModContent.ItemType<TragodiaLegs>(),
			];
		}
	}
}
