using System.Collections.Generic;
using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.Medic.ReedFlameShadow;

namespace ArknightsMod.Content.Items.Armor.Medic.ReedFlameShadow
{
	public class ReedFlameShadowDefault : ArknightsVanityBag
	{
		public override int Rarity => 5;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<ReedFlameShadowHead>(),
				ModContent.ItemType<ReedFlameShadowBody>(),
				ModContent.ItemType<ReedFlameShadowLegs>()
			];
		}
	}
}
