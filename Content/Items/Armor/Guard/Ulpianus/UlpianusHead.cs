using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Ulpianus
{
	[AutoloadEquip(EquipType.Head)]
	public class UlpianusHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 652,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Ulpianus",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<PolymerizationPreparation>(6)
				.AddIngredient<TransmutedSaltAgglomerate>(6),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Ulpianus.SetBonus",
		};
	}
}
