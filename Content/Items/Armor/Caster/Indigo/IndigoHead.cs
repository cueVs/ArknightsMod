using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Caster.Indigo
{
	[AutoloadEquip(EquipType.Head)]
	public class IndigoHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 4;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 144,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Indigo",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(40)
				.AddIngredient<Oriron>(1)
				.AddIngredient<RMA7012>(7),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Indigo.SetBonus",
		};
	}
}
