using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria.ModLoader;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.Rosmontis
{
	[AutoloadEquip(EquipType.Legs)]
	public class RosmontisLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 7,
			LifeBonus = 97,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Rosmontis",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<CrystallineElectronicUnit>(6)
				.AddIngredient<GrindstonePentahydrate>(4),
		};
	}
}
