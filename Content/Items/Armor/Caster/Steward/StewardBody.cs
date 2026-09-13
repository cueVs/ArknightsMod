using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Caster.Steward
{
	[AutoloadEquip(EquipType.Body)]
	public class StewardBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 7,
			LifeBonus = 55,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Steward",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<OrirockCube>(3),
		};
	}
}
