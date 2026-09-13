using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Matoimaru
{
	[AutoloadEquip(EquipType.Head)]
	public class MatoimaruHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 4;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 202,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Matoimaru",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(40)
				.AddIngredient<Device>(1)
				.AddIngredient<SugarPack>(10),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Matoimaru.SetBonus",
		};
	}
}
