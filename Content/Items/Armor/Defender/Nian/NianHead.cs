using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Defender.Nian
{
	[AutoloadEquip(EquipType.Head)]
	public class NianHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 410,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Nian",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<BipolarNanoflake>(6)
				.AddIngredient<PolymerizedGel>(6),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Nian.SetBonus",
		};
	}
}
