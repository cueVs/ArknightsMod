using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Caster.Haze
{
	[AutoloadEquip(EquipType.Body)]
	public class HazeBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 4;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 5,
			LifeBonus = 44,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Haze",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(40)
				.AddIngredient<Polyester>(3)
				.AddIngredient<OrironCluster>(2),
		};
	}
}
