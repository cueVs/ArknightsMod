using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Vanguard.Texas
{
	[AutoloadEquip(EquipType.Legs)]
	public class TexasLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 9,
			LifeBonus = 98,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Texas",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<GrindstonePentahydrate>(3)
				.AddIngredient<LoxicKohl>(4),
		};
	}
}
