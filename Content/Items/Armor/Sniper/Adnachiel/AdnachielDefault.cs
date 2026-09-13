using System.Collections.Generic;
using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.Sniper.Adnachiel;

namespace ArknightsMod.Content.Items.Armor.Sniper.Adnachiel
{
	public class AdnachielDefault : ArknightsVanityBag
	{
		public override int Rarity => 3;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<AdnachielHead>(),
				ModContent.ItemType<AdnachielBody>(),
				ModContent.ItemType<AdnachielLegs>()
			];
		}
	}
}
