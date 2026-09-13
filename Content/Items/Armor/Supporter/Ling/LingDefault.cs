using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Supporter.Ling
{
	internal class LingDefault : ArknightsVanityBag
	{
		public override ObtainTypes ObtainType => ObtainTypes.Limited_Festival;
		public override int Rarity => 6;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<LingHead>(),
				ModContent.ItemType<LingBody>(),
				ModContent.ItemType<LingLegs>()
			];
		}
	}
}
