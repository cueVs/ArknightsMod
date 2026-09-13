using System.Collections.Generic;
using ArknightsMod.Content.Items.Armor.Guard.Ulpianus;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Ulpianus
{
	public class UlpianusDefault : ArknightsVanityBag
	{
		public override int Rarity => 6;
		protected override List<int> GetItems()
		{
			return
			[
				ModContent.ItemType<UlpianusHead>(),
				ModContent.ItemType<UlpianusBody>(),
				ModContent.ItemType<UlpianusLegs>(),
			];
		}
	}
}
