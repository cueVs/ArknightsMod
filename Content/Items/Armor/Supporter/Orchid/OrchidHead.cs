using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Supporter.Orchid
{
	[AutoloadEquip(EquipType.Head)]
	public class OrchidHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 94,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Orchid",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Orirock>(2),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Orchid.SetBonus",
		};
	}
}
