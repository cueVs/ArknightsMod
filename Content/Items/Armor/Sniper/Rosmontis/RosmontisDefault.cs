using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.Rosmontis
{
	internal class RosmontisDefault : ArknightsVanityBag
	{
		public override ObtainTypes ObtainType => ObtainTypes.Limited_Celebration;
		public override int Rarity => 6;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<RosmontisHead>(),
				ModContent.ItemType<RosmontisBody>(),
				ModContent.ItemType<RosmontisLegs>()
			];
		}
	}
}
