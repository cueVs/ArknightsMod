using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Supporter.Orchid
{
	[AutoloadEquip(EquipType.Body)]
	public class OrchidBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 6,
			LifeBonus = 47,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Orchid",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Polyester>(2),
		};
	}
}
