using ArknightsMod.Content.Items.Armor.Sniper.Wisadel;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.Wisadel
{
	public class WisadelDefault : ArknightsVanityBag
	{
		public override ObtainTypes ObtainType => ObtainTypes.Limited_Celebration;
		public override int Rarity => 6;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<WisadelHead>(),
				ModContent.ItemType<WisadelBody>(),
				ModContent.ItemType<WisadelLegs>()
			];
		}
	}
}
