using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Caster.Haze
{
	[AutoloadEquip(EquipType.Head)]
	public class HazeHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 4;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 89,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Haze",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(40)
				.AddIngredient<Polyester>(1)
				.AddIngredient<RMA7012>(8),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Haze.SetBonus",
		};
	}
}
