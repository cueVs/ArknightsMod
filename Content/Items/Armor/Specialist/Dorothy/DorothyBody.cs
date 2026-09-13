using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Specialist.Dorothy
{
	[AutoloadEquip(EquipType.Body)]
	public class DorothyBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 13,
			LifeBonus = 75,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Dorothy",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<PolymerizationPreparation>(6)
				.AddIngredient<IncandescentAlloyBlock>(6),
		};
	}
}
