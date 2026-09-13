using System.Collections.Generic;
using Terraria.ModLoader;


namespace ArknightsMod.Content.Items.Armor.Guard.LaPluma
{
	internal class LaPlumaDefault:ArknightsVanityBag
	{
		public override int Rarity => 5;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<LaPlumaHead>(),
				ModContent.ItemType<LaPlumaBody>(),
				ModContent.ItemType<LaPlumaLegs>()
			];
		}
	}
}
