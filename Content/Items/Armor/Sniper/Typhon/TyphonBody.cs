using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.Typhon
{
	[AutoloadEquip(EquipType.Body)]
	public class TyphonBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 8,
			LifeBonus = 85,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Typhon",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<D32Steel>(6)
				.AddIngredient<WhiteHorseKohl>(7),
		};
	}
}
