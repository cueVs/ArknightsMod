using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Caster.Mornia
{
	[AutoloadEquip(EquipType.Head)]
	public class MorniaHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 4;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 8,
			LifeBonus = 100,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Mornia",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(40)
				.AddIngredient<Aketon>(3)
				.AddIngredient<RefinedSolvent>(2),
			OnHelmetActive = MorniaSetPlayer.OnHelmetActive,
			OnFullSetActive = MorniaSetPlayer.OnFullSetActive,
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Mornia.SetBonus",
		};
	}
}
