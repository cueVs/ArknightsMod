using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Astesia
{
	// 星极的三合一时装袋（Gacha / 掉落获得：头、身、腿各一件）。
	internal class AstesiaDefault : ArknightsVanityBag
	{
		public override int Rarity => 5;

		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<AstesiaHead>(),
				ModContent.ItemType<AstesiaBody>(),
				ModContent.ItemType<AstesiaLegs>()
			];
		}
	}
}
