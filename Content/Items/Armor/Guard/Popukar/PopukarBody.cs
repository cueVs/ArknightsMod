using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Popukar
{
	[AutoloadEquip(EquipType.Body)]
	public class PopukarBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 19,
			LifeBonus = 93,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Popukar",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Oriron>(2),
		};
	}
}
