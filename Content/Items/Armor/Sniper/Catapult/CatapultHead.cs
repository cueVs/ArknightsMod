using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.Catapult
{
	[AutoloadEquip(EquipType.Head)]
	public class CatapultHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 115,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Catapult",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Ester>(1),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Catapult.SetBonus",
		};
	}
}
