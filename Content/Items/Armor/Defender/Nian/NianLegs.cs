using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Defender.Nian
{
	[AutoloadEquip(EquipType.Legs)]
	public class NianLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 20,
			LifeBonus = 205,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Nian",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<D32Steel>(6)
				.AddIngredient<IncandescentAlloyBlock>(6),
		};
	}
}
