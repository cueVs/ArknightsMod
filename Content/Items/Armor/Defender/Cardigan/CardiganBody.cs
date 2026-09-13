using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Defender.Cardigan
{
	[AutoloadEquip(EquipType.Body)]
	public class CardiganBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 36,
			LifeBonus = 122,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Cardigan",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Device>(1),
		};
	}
}
