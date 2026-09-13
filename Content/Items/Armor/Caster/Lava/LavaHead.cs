using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Caster.Lava
{
	[AutoloadEquip(EquipType.Head)]
	public class LavaHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 114,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Lava",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<SugarSubstitute>(1),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Lava.SetBonus",
		};
	}
}
