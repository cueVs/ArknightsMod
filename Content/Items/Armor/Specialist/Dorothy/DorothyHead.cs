using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Specialist.Dorothy
{
	[AutoloadEquip(EquipType.Head)]
	public class DorothyHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 150,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Dorothy",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<BipolarNanoflake>(6)
				.AddIngredient<CuttingFluidSolution>(5),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Dorothy.SetBonus",
		};
	}
}
