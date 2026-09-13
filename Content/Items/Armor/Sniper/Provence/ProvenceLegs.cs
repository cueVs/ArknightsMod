using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.Provence
{
	[AutoloadEquip(EquipType.Legs)]
	public class ProvenceLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 6,
			LifeBonus = 84,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Provence",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<GrindstonePentahydrate>(3)
				.AddIngredient<LoxicKohl>(4),
		};
	}
}
