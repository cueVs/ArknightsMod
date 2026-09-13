using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Medic.ReedFlameShadow
{
	[AutoloadEquip(EquipType.Legs)]
	public class ReedFlameShadowLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 3,
			LifeBonus = 83,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.ReedFlameShadow",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<ManganeseTrihydrate>(3)
				.AddIngredient<IntegratedDevice>(2),
		};
	}
}
