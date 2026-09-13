using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Defender.Cardigan
{
	[AutoloadEquip(EquipType.Head)]
	public class CardiganHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 243,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Cardigan",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<OrironShard>(1),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Cardigan.SetBonus",
		};
	}
}
