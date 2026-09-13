using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.Fartooth
{
	internal class FartoothDefault : ArknightsVanityBag
	{
		public override int Rarity => 6;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<FartoothHead>(),
				ModContent.ItemType<FartoothBody>(),
				ModContent.ItemType<FartoothLegs>()
			];
		}
	}
}
