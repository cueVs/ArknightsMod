using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Supporter.Deepcolor
{
	internal class DeepcolorDefault : ArknightsVanityBag
	{
		public override int Rarity => 4;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<DeepcolorHead>(),
				ModContent.ItemType<DeepcolorBody>(),
				ModContent.ItemType<DeepcolorLegs>()
			];
		}
	}
}
