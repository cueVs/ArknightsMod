using ArknightsMod.Content.Items.Armor.Guard.Chen;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Chen
{
	public class ChenDefault : ArknightsVanityBag
	{
		public override int Rarity => 6;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<ChenHead>(),
				ModContent.ItemType<ChenBody>(),
				ModContent.ItemType<ChenLegs>()
			];
		}
	}
}
