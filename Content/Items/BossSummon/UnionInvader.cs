using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using ArknightsMod.Content.NPCs.Enemy.ThroughChapter4;

namespace ArknightsMod.Content.Items.BossSummon
{
	public class UnionInvader : ModItem, ILocalizedModType
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 20;
			Item.maxStack = 1;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.consumable = true;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(0, 1, 0, 0);
		}

		public override bool CanUseItem(Player player) {
			return !NPC.AnyNPCs(ModContent.NPCType<Crownslayer>());
		}

		public override bool? UseItem(Player player) {
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				NPC.NewNPC(player.GetSource_ItemUse(Item), (int)player.Center.X, (int)player.Center.Y, ModContent.NPCType<Crownslayer>());
			}
			return true;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.Wood, 3)
				.AddTile(TileID.DemonAltar)
				.Register();
		}
	}
}
