using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Utage
{
	[AutoloadEquip(EquipType.Body)]
	public class UtageBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 4;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 16,
			LifeBonus = 98,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Utage",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(40)
				.AddIngredient<Polyketon>(3)
				.AddIngredient<ManganeseOre>(2),
		};
	}
}
