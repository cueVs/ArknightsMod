using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Medic.Ansel
{
	[AutoloadEquip(EquipType.Body)]
	public class AnselBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 8,
			LifeBonus = 57,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Ansel",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Device>(1),
		};
	}
}
