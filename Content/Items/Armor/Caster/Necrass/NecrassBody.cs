using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Caster.Necrass
{
	[AutoloadEquip(EquipType.Body)]
	public class NecrassBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 9,
			LifeBonus = 83,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Necrass",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<WhiteHorseKohl>(3)
				.AddIngredient<Aketon>(5),
		};
	}
}
