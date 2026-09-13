using System.Collections.Generic;
using ArknightsMod.Content.Items.Armor.Defender.Nian;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Defender.Nian
{
	public class NianDefault : ArknightsVanityBag
	{
		public override ObtainTypes ObtainType => ObtainTypes.Limited_Festival;
		public override int Rarity => 6;
		protected override List<int> GetItems()
		{
			return
			[
				ModContent.ItemType<NianHead>(),
				ModContent.ItemType<NianBody>(),
				ModContent.ItemType<NianLegs>(),
			];
		}
	}
}
