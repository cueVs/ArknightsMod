using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.Adnachiel
{
	[AutoloadEquip(EquipType.Body)]
	public class AdnachielBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 10,
			LifeBonus = 54,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Adnachiel",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Polyester>(2),
		};
	}
}
