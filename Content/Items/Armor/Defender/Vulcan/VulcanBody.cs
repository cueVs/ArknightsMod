using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Defender.Vulcan
{
	[AutoloadEquip(EquipType.Body)]
	public class VulcanBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 44,
			LifeBonus = 205,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Vulcan",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<WhiteHorseKohl>(3)
				.AddIngredient<Aketon>(5),
		};
	}
}
