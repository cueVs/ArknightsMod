using System.Collections.Generic;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Caster.Haze
{
	internal class HazeDefault:ArknightsVanityBag
	{
		public override int Rarity => 4;
		protected override List<int> GetItems() {
			return
			[
				ModContent.ItemType<Armor.Caster.Haze.HazeHead>(),
				ModContent.ItemType<Armor.Caster.Haze.HazeBody>(),
				ModContent.ItemType<Armor.Caster.Haze.HazeLegs>()
			];
		}
	}
}
