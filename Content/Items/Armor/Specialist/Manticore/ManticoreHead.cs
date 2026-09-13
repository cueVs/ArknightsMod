using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Specialist.Manticore
{
	[AutoloadEquip(EquipType.Head)]
	public class ManticoreHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 163,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Manticore",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<ManganeseTrihydrate>(8)
				.AddIngredient<SugarPack>(12),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Manticore.SetBonus",
		};
	}
}
