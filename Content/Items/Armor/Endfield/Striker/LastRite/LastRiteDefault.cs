using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Endfield.Striker.LastRite
{
	public class LastRiteDefault : ArknightsVanityBag
	{
		public override ObtainTypes ObtainType => ObtainTypes.EndfieldDefault;
		public override int Rarity => 6;
		protected override List<int> GetItems()
		{
			return new List<int>
			{
				ModContent.ItemType<LastRiteHead>(),
				ModContent.ItemType<LastRiteBody>(),
				ModContent.ItemType<LastRiteLegs>(),
			};
		}
	}
}
