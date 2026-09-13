using System.Collections.Generic;
using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.Caster.Steward;

namespace ArknightsMod.Content.Items.Armor.Caster.Steward
{
	public class StewardDefault : ArknightsVanityBag
	{
		public override int Rarity => 3;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<StewardHead>(),
				ModContent.ItemType<StewardBody>(),
				ModContent.ItemType<StewardLegs>()
			];
		}
	}
}
