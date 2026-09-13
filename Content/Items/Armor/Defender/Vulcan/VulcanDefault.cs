using ArknightsMod.Content.Items.Armor.Defender.Vulcan;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Defender.Vulcan
{
	internal class VulcanDefault : ArknightsVanityBag
	{
		public override int Rarity => 5;
		protected override List<int> GetItems()
		{
			return
			[
				ModContent.ItemType<VulcanHead>(),
				ModContent.ItemType<VulcanBody>(),
				ModContent.ItemType<VulcanLegs>()
			];
		}
	}
}
