using System.Collections.Generic;
using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.Medic.Hibiscus;

namespace ArknightsMod.Content.Items.Armor.Medic.Hibiscus
{
	public class HibiscusDefault : ArknightsVanityBag
	{
		public override int Rarity => 3;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<HibiscusHead>(),
				ModContent.ItemType<HibiscusBody>(),
				ModContent.ItemType<HibiscusLegs>()
			];
		}
	}
}
