using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Matoimaru
{
	[AutoloadEquip(EquipType.Body)]
	public class MatoimaruBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 4;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 8,
			LifeBonus = 101,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Matoimaru",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(40)
				.AddIngredient<Polyester>(3)
				.AddIngredient<ManganeseOre>(2),
		};
	}
}
