using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Midnight
{
	[AutoloadEquip(EquipType.Head)]
	public class MidnightHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 166,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Midnight",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<OrironShard>(1),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Midnight.SetBonus",
		};
	}
}
