using ArknightsMod.Content.Items.Armor.Specialist.TexasAlter;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Specialist.TexasAlter
{
	public class TexalterDefault : ArknightsVanityBag
	{
		public override ObtainTypes ObtainType => ObtainTypes.Limited_Celebration;
		public override int Rarity => 6;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<TexalterHead>(),
				ModContent.ItemType<TexalterBody>(),
				ModContent.ItemType<TexalterLegs>()
			];
		}
	}
}
