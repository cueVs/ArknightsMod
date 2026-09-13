using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Medic.Kaltsit
{
	[AutoloadEquip(EquipType.Legs)]
	public class KaltsitLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 7,
			LifeBonus = 102,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Kaltsit",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<CrystallineElectronicUnit>(6)
				.AddIngredient<GrindstonePentahydrate>(4),
		};
	}
}
