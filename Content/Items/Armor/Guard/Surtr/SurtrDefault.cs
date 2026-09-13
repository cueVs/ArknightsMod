using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Surtr
{
	internal class SurtrDefault:ArknightsVanityBag
	{
		public override int Rarity => 6;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<SurtrHead>(),
				ModContent.ItemType<SurtrBody>(),
				ModContent.ItemType<SurtrLegs>()
			];
		}
	}
}
