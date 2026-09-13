using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Specialist.Mortis
{
	[AutoloadEquip(EquipType.Body)]
	public class MortisBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 25,
			LifeBonus = 111,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Mortis",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<RMA7024>(3)
				.AddIngredient<IntegratedDevice>(2),
		};
	}
}
