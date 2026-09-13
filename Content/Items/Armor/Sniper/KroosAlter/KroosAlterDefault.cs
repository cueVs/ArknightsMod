using System.Collections.Generic;
using Terraria.ModLoader;
namespace ArknightsMod.Content.Items.Armor.Sniper.KroosAlter
{
	internal class KroosAlterDefault:ArknightsVanityBag
	{
		public override int Rarity => 5;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<KkdyAlterHead>(),
				ModContent.ItemType<KkdyAlterBody>(),
				ModContent.ItemType<KkdyAlterLegs>()
			];
		}
	}
}
