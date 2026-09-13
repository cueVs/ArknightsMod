using System.Collections.Generic;
using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.Caster.Lava;

namespace ArknightsMod.Content.Items.Armor.Caster.Lava
{
	public class LavaDefault : ArknightsVanityBag
	{
		public override int Rarity => 3;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<LavaHead>(),
				ModContent.ItemType<LavaBody>(),
				ModContent.ItemType<LavaLegs>()
			];
		}
	}
}
