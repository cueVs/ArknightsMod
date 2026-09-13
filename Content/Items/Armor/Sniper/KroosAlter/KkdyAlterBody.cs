using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.KroosAlter
{
	[AutoloadEquip(EquipType.Body)]
	public class KkdyAlterBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 11,
			LifeBonus = 62,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.KkdyAlter",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<IncandescentAlloyBlock>(3)
				.AddIngredient<RMA7012>(3),
		};
	}
}
