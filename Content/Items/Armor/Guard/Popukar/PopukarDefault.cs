using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Popukar
{
	public class PopukarDefault : ArknightsVanityBag
	{
		public override int Rarity => 3;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<PopukarHead>(),
				ModContent.ItemType<PopukarBody>(),
				ModContent.ItemType<PopukarLegs>()
			];
		}
	}
}
