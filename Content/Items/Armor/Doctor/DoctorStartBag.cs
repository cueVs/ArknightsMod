using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Doctor
{
	public class DoctorStartBag : ArknightsVanityBag
	{
		public override ObtainTypes ObtainType => ObtainTypes.NoGacha;
		public override int Rarity => 1;
		protected override List<int> GetItems() {
			return new List<int>
			{
				ModContent.ItemType<DoctorHood>(),
				ModContent.ItemType<DoctorJacket>(),
				ModContent.ItemType<DoctorPants>()
			};
		}
	}
}
