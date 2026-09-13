using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Caster.Steward
{
	[AutoloadEquip(EquipType.Head)]
	public class StewardHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 110,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Steward",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Diketon>(1),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Steward.SetBonus",
		};
	}
}
