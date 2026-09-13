using ArknightsMod.Content.Items.Armor.Defender.Mudrock;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Defender.Mudrock
{
	public class MudrockDefault : ArknightsVanityBag
	{
		public override int Rarity => 6;
		public override ObtainTypes ObtainType => ObtainTypes.NoGacha;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<MudrockHead>(),
				ModContent.ItemType<MudrockBody>(),
				ModContent.ItemType<MudrockLegs>()
			];
		}
	}
}
