using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Melantha
{
	[AutoloadEquip(EquipType.Body)]
	public class MelanthaBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 6,
			LifeBonus = 70,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Melantha",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Polyester>(2),
		};
	}
}
