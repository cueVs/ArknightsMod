using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.Adnachiel
{
	[AutoloadEquip(EquipType.Head)]
	public class AdnachielHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 108,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Adnachiel",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Orirock>(2),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Adnachiel.SetBonus",
		};
	}
}
