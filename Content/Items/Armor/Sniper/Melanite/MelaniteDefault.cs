using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.Melanite
{
	internal class MelaniteDefault:ArknightsVanityBag
	{
		public override int Rarity => 5;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<MelaniteHead>(),
				ModContent.ItemType<MelaniteBody>(),
				ModContent.ItemType<MelaniteLegs>()
			];
		}
	
	}
}
