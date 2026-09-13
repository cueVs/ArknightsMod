using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.Midnight
{
	[AutoloadEquip(EquipType.Body)]
	public class MidnightBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 3;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 21,
			LifeBonus = 83,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Midnight",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Device>(1),
		};
	}
}
