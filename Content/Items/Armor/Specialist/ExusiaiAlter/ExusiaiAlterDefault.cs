using ArknightsMod.Content.Items.Armor;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Specialist.ExusiaiAlter
{
	public class ExusiaiAlterDefault : ArknightsVanityBag
	{
		public override ObtainTypes ObtainType => ObtainTypes.Limited_Celebration;
		public override int Rarity => 6;
		protected override List<int> GetItems() {
			return new List<int>
			{
				ModContent.ItemType<ExusiaiAlterHead>(),
				ModContent.ItemType<ExusiaiAlterBody>(),
				ModContent.ItemType<ExusiaiAlterLegs>()
			};
		}
	}
}