using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Specialist.Manticore
{
	[AutoloadEquip(EquipType.Body)]
	public class ManticoreBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 28,
			LifeBonus = 82,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Manticore",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<OrironBlock>(3)
				.AddIngredient<SugarPack>(1),
		};
	}
}
