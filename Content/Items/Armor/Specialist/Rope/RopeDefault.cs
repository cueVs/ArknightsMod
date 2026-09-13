using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Specialist.Rope
{
	// 暗索的三合一时装袋（Gacha / 掉落获得：头、身、腿各一件）。
	internal class RopeDefault : ArknightsVanityBag
	{
		public override int Rarity => 4;

		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<RopeHead>(),
				ModContent.ItemType<RopeBody>(),
				ModContent.ItemType<RopeLegs>()
			];
		}
	}
}
