using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Defender.Saria
{
	[AutoloadEquip(EquipType.Head)]
	public class SariaHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 315,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Saria",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<BipolarNanoflake>(6)
				.AddIngredient<ManganeseTrihydrate>(5),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Saria.SetBonus",
		};
	}
}
