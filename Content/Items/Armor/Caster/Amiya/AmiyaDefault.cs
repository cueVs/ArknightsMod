using System.Collections.Generic;
using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.Caster.Amiya;

namespace ArknightsMod.Content.Items.Armor.Caster.Amiya
{
	public class AmiyaDefault : ArknightsVanityBag
	{
		public override int Rarity => 5;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<AmiyaHead>(),
				ModContent.ItemType<AmiyaBody>(),
				ModContent.ItemType<AmiyaLegs>()
			];
		}
	}
}
