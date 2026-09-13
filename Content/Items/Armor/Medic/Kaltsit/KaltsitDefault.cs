using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Medic.Kaltsit
{
	internal class KaltsitDefault: ArknightsVanityBag
	{
		public override int Rarity => 6;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<KaltsitHead>(),
				ModContent.ItemType<KaltsitBody>(),
				ModContent.ItemType<KaltsitLegs>()
			];
		}
	}
}
