using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Entelechia
{
	[AutoloadEquip(EquipType.Head)]
	public class EntelechiaHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 258,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Entelechia",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<CrystallineElectronicUnit>(6)
				.AddIngredient<GrindstonePentahydrate>(1),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Entelechia.SetBonus",
		};
	}
}
