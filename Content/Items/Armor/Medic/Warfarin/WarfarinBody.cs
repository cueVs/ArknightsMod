using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Medic.Warfarin
{
	[AutoloadEquip(EquipType.Body)]
	public class WarfarinBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 10,
			LifeBonus = 76,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Warfarin",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<SugarLump>(3)
				.AddIngredient<RMA7012>(3),
		};
	}
}
