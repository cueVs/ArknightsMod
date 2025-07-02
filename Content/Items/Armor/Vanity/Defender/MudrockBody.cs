using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Vanity.Defender
{
	[AutoloadEquip(EquipType.Body)]
	internal class MudrockBody : ModItem
	{
		public override void SetStaticDefaults() {
			ArmorIDs.Body.Sets.HidesTopSkin[Item.bodySlot] = true;
		}
		public override void SetDefaults() {
			Item.width = Item.height = 16;
			Item.rare = ItemRarityID.LightPurple;
			Item.vanity = true;
		}
	}
}
