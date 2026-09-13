using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Caster.Mostima
{
	internal class MostimaDefault:ArknightsVanityBag
	{
		public override int Rarity => 6;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<MostimaHead>(),
				ModContent.ItemType<MostimaBody>(),
				ModContent.ItemType<MostimaLegs>()
			];
		}
	}
}
