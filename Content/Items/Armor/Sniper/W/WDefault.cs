using ArknightsMod.Content.Items.Armor.Sniper.W;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.W
{
	public class WDefault : ArknightsVanityBag
	{
		public override ObtainTypes ObtainType => ObtainTypes.Limited_Celebration;
		public override int Rarity => 6;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<WHead>(),
				ModContent.ItemType<WBody>(),
				ModContent.ItemType<WLegs>()
			];
		}
	}
}
