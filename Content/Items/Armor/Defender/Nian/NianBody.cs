using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Defender.Nian
{
	[AutoloadEquip(EquipType.Body)]
	public class NianBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 60,
			LifeBonus = 205,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Nian",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<PolymerizationPreparation>(6)
				.AddIngredient<PolymerizedGel>(7),
		};
	}
}
