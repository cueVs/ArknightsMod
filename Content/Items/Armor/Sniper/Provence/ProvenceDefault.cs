using ArknightsMod.Content.Items.Armor.Sniper.Provence;
using System.Collections.Generic;
using Terraria.ModLoader;
namespace ArknightsMod.Content.Items.Armor.Sniper.Provence
{
	internal class ProvenceDefault : ArknightsVanityBag
	{
		public override int Rarity => 5;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<ProvenceHead>(),
				ModContent.ItemType<ProvenceBody>(),
				ModContent.ItemType<ProvenceLegs>()
			];
		}
	}
}
