using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Armor.Guard.LaPluma
{
	[AutoloadEquip(EquipType.Body)]
	public class LaPlumaBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 5;

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 34,
			LifeBonus = 113,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.LaPluma",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(50)
				.AddIngredient<KetonColloid>(3)
				.AddIngredient<CoagulatingGel>(2),
		};
	}
}
