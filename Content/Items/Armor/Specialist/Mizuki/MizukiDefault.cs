using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Specialist.Mizuki
{
	internal class MizukiDefault : ArknightsVanityBag
	{
		public override int Rarity => 6;

		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<MizukiHead>(),
				ModContent.ItemType<MizukiBody>(),
				ModContent.ItemType<MizukiLegs>()
			];
		}
	}
}
