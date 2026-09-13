using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Astesia
{
	[AutoloadEquip(EquipType.Body)]
	public class AstesiaBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 25,
			LifeBonus = 96,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Astesia",
			Materials = recipe => recipe
				.AddIngredient<PolyesterLump>(3)
				.AddIngredient<OrirockCluster>(4),
		};
	}
}
