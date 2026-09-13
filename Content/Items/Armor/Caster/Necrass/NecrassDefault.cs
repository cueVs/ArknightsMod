using System.Collections.Generic;
using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.Caster.Necrass;

namespace ArknightsMod.Content.Items.Armor.Caster.Necrass
{
	public class NecrassDefault : ArknightsVanityBag
	{
		public override int Rarity => 5;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<NecrassHead>(),
				ModContent.ItemType<NecrassBody>(),
				ModContent.ItemType<NecrassLegs>()
			];
		}
	}
}
