using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Specialist.Rope
{
	[AutoloadEquip(EquipType.Head)]
	public class RopeHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 4;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 101,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Rope",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(40)
				.AddIngredient<OrirockCluster>(6),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Rope.SetBonus",
		};
	}
}
