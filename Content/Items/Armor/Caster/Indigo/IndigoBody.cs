using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Caster.Indigo
{
	[AutoloadEquip(EquipType.Body)]
	public class IndigoBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 4;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 9,
			LifeBonus = 72,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Indigo",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(40)
				.AddIngredient<OrirockCube>(4)
				.AddIngredient<ManganeseOre>(2),
		};
	}
}
