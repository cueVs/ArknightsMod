using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Sniper.Melanite
{
	[AutoloadEquip(EquipType.Legs)]
	public class MelaniteLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 5,
			LifeBonus = 83,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Melanite",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<TransmutedSaltAgglomerate>(3)
				.AddIngredient<PolyesterPack>(3),
		};
	}
}
