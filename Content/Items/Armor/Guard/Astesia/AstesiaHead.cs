using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Astesia
{
	[AutoloadEquip(EquipType.Head)]
	public class AstesiaHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 191,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Astesia",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<ManganeseTrihydrate>(8)
				.AddIngredient<SugarPack>(12),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Astesia.SetBonus",
		};
	}
}
