using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Defender.Spot
{
	[AutoloadEquip(EquipType.Body)]
	public class SpotBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 35,
			LifeBonus = 92,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Spot",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<OrirockCube>(3),
		};
	}
}
