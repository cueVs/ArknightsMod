using ArknightsMod.Content.Items.Armor.Guard.Oblivionis;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Oblivionis
{
	public class OblivionisDefault : ArknightsVanityBag
	{
		public override ObtainTypes ObtainType => ObtainTypes.Limited_CrossOver;
		public override int Rarity => 6;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<OblivionisHead>(),
				ModContent.ItemType<OblivionisBody>(),
				ModContent.ItemType<OblivionisLegs>()
			];
		}
	}
}
