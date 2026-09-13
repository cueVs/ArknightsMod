using ArknightsMod.Content.Items.Armor.Specialist.Mortis;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Specialist.Mortis
{
	public class MortisDefault : ArknightsVanityBag
	{
		public override ObtainTypes ObtainType => ObtainTypes.Limited_CrossOver;
		public override int Rarity => 5;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<MortisHead>(),
				ModContent.ItemType<MortisBody>(),
				ModContent.ItemType<MortisLegs>()
			];
		}
	}
}
