using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Endfield.Striker.Yvonne
{
	public class YvonneDefault : ArknightsVanityBag
	{
		public override ObtainTypes ObtainType => ObtainTypes.EndfieldDefault;
		public override int Rarity => 6;
		protected override List<int> GetItems()
		{
			return
			[
				ModContent.ItemType<YvonneHead>(),
				ModContent.ItemType<YvonneBody>(),
				ModContent.ItemType<YvonneLegs>(),
			];
		}
	}
}
