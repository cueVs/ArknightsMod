using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Endfield.Guard.Endministrator
{
	public class EndminFemaleDefault : ArknightsVanityBag
	{
		public override ObtainTypes ObtainType => ObtainTypes.NoGacha;
		public override int Rarity => 6;
		protected override List<int> GetItems()
		{
			return new List<int>
			{
				ModContent.ItemType<EndminFemaleHead>(),
				ModContent.ItemType<EndminFemaleBody>(),
				ModContent.ItemType<EndminFemaleLegs>(),
				ModContent.ItemType<EndminMask>(),
			};
		}
	}
}
