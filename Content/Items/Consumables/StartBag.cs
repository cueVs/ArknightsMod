using ArknightsMod.Content.Items.Armor.Doctor;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Consumables
{
	public class StartBag : ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults() {
			Item.maxStack = 9999;
			Item.consumable = true;
			Item.width = 38;
			Item.height = 32;
			Item.rare = ItemRarityID.White;
		}

		public override bool CanRightClick() {
			return true;
		}

		public override void ModifyItemLoot(ItemLoot itemLoot) {
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<DoctorHood>()));
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<DoctorJacket>()));
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<DoctorPants>()));
		}
	}
}
