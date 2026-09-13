using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Utage
{
	internal class UtageDefault : ArknightsVanityBag
	{
		public override int Rarity => 4;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<UtageHead>(),
				ModContent.ItemType<UtageBody>(),
				ModContent.ItemType<UtageLegs>()
			];
		}
	}
}
