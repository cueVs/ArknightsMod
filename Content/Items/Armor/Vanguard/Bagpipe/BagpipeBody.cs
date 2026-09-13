using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Vanguard.Bagpipe
{
	[AutoloadEquip(EquipType.Body)]
	public class BagpipeBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 6;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 29,
			LifeBonus = 124,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Bagpipe",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(60)
				.AddIngredient<D32Steel>(6)
				.AddIngredient<WhiteHorseKohl>(7),
		};
	}
}
