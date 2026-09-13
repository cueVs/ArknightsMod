using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.Melanite
{
	[AutoloadEquip(EquipType.Body)]
	public class MelaniteBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 16,
			LifeBonus = 83,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Melanite",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<IncandescentAlloyBlock>(3)
				.AddIngredient<LoxicKohl>(4),
		};
	}
}
