using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Chen
{
	[AutoloadEquip(EquipType.Body)]
	public class ChenBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 30,
			LifeBonus = 144,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Chen",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<D32Steel>(6)
				.AddIngredient<PolyesterLump>(6),
		};
	}
}
