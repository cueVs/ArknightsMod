using ArknightsMod.Content.Items.Armor.Specialist.Dorothy;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Specialist.Dorothy
{
	internal class DorothyDefault:ArknightsVanityBag
	{
		public override int Rarity => 6;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<DorothyHead>(),
				ModContent.ItemType<DorothyBody>(),
				ModContent.ItemType<DorothyLegs>()
			];
		}
	}
}
