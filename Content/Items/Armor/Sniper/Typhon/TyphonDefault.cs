using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.Typhon
{
	public class TyphonDefault : ArknightsVanityBag
	{
		public override int Rarity => 6;
		protected override List<int> GetItems()
		{
			return
			[
				ModContent.ItemType<TyphonHead>(),
				ModContent.ItemType<TyphonBody>(),
				ModContent.ItemType<TyphonLegs>()
			];
		}
	}
}
