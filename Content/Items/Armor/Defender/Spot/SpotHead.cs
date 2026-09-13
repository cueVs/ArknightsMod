using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Defender.Spot
{
	[AutoloadEquip(EquipType.Head)]
	public class SpotHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 184,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Spot",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Diketon>(1),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Spot.SetBonus",
		};
	}
}
