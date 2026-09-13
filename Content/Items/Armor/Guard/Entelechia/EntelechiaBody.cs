using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Entelechia
{
	[AutoloadEquip(EquipType.Body)]
	public class EntelechiaBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 34,
			LifeBonus = 129,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Entelechia",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<PolymerizationPreparation>(6)
				.AddIngredient<RMA7024>(6),
		};
	}
}
