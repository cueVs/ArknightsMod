using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Vanity.Defender
{
	[AutoloadEquip(EquipType.Head)]
	internal class MudrockHelmet : ModItem
	{
		public override void SetDefaults() {
			Item.width = Item.height = 16;
			Item.rare = ItemRarityID.LightPurple;
			Item.vanity = true;
		}
	}
	internal class MudrockItemChange : GlobalItem
	{
		public override bool CanRightClick(Item item) {
			int type = item.type;
			if (type == ModContent.ItemType<MudrockHelmet>() || type == ModContent.ItemType<MudrockHead>()) {
				if (Main.mouseRightRelease) {
					item.ChangeItemType(item.type == ModContent.ItemType<MudrockHelmet>() ? ModContent.ItemType<MudrockHead>() : ModContent.ItemType<MudrockHelmet>());

					SoundEngine.PlaySound(SoundID.Grab);

					Main.stackSplit = 30;
					Main.mouseRightRelease = false;
					Recipe.FindRecipes();
				}
				return false;
			}
			if (type == ModContent.ItemType<MudrockChestplate>() || type == ModContent.ItemType<MudrockBody>()) {
				if (Main.mouseRightRelease) {
					item.ChangeItemType(item.type == ModContent.ItemType<MudrockChestplate>() ? ModContent.ItemType<MudrockBody>() : ModContent.ItemType<MudrockChestplate>());

					SoundEngine.PlaySound(SoundID.Grab);

					Main.stackSplit = 30;
					Main.mouseRightRelease = false;
					Recipe.FindRecipes();
				}
				return false;
			}
			return base.CanRightClick(item);
		}
	}
}
