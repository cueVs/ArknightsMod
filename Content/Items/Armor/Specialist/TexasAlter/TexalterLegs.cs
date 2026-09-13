using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Specialist.TexasAlter
{
	[AutoloadEquip(EquipType.Legs)]
	public class TexalterLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 8,
			LifeBonus = 80,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Texalter",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<BipolarNanoflake>(6)
				.AddIngredient<GrindstonePentahydrate>(5),
		};
	}
}
