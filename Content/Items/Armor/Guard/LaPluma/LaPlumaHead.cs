using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.LaPluma
{
	[AutoloadEquip(EquipType.Head)]
	public class LaPlumaHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 225,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.LaPluma",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<KetonColloid>(7)
				.AddIngredient<ManganeseOre>(13),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.LaPluma.SetBonus",
		};
	}
}
