using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Specialist.TexasAlter
{
	[AutoloadEquip(EquipType.Head)]
	public class TexalterHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 160,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Texalter",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<NucleicCrystalSinter>(6)
				.AddIngredient<KetonColloid>(4),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Texalter.SetBonus",
		};
	}
}
