using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Oblivionis
{
	[AutoloadEquip(EquipType.Head)]
	public class OblivionisHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 236,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Oblivionis",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<PolymerizationPreparation>(6)
				.AddIngredient<OrironBlock>(5),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Oblivionis.SetBonus",
		};
	}
}
