using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Specialist.Rope
{
	[AutoloadEquip(EquipType.Body)]
	public class RopeBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 4;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 15,
			LifeBonus = 50,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Rope",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(40)
				.AddIngredient<PolyesterPack>(6),
		};
	}
}
