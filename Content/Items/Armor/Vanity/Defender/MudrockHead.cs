using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Vanity.Defender
{
	[AutoloadEquip(EquipType.Head)]
	internal class MudrockHead : ModItem
	{
		public override void SetDefaults() {
			Item.width = Item.height = 16;
			Item.rare = ItemRarityID.LightPurple;
			Item.vanity = true;
		}
	}
}
