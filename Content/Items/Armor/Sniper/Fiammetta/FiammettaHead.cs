using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.Fiammetta
{
	[AutoloadEquip(EquipType.Head)]
	public class FiammettaHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 193,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Fiammetta",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<CrystallineElectronicUnit>(6)
				.AddIngredient<GrindstonePentahydrate>(4),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Fiammetta.SetBonus",
		};
	}
}
