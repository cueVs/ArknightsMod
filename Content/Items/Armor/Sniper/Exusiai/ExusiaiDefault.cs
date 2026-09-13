using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.Exusiai
{
	internal class ExusiaiDefault:ArknightsVanityBag
	{
		public override int Rarity => 6;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<ExusiaiHead>(),
				ModContent.ItemType<ExusiaiBody>(),
				ModContent.ItemType<ExusiaiLegs>()
			];
		}
	}
}
