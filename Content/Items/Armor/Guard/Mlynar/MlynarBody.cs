using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Mlynar
{
	[AutoloadEquip(EquipType.Body)]
	public class MlynarBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 38,
			LifeBonus = 213,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Mlynar",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<D32Steel>(6)
				.AddIngredient<RMA7024>(5),
		};
	}
}
