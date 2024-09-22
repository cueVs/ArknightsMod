using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using static Terraria.ModLoader.ModContent;
using ArknightsMod.Content.NPCs.Enemy.Chapter6.FrostNova;

namespace ArknightsMod.Content.Items.Summon
{
    public class Spicycandy : ModItem
    {
        public override void SetStaticDefaults()
        {
			Item.ResearchUnlockCount = 3;
			ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;
		}

		public override void SetDefaults() {
			Item.UseSound = SoundID.Item4;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useTurn = false;
			Item.useAnimation = 5;
			Item.useTime = 5;
			Item.maxStack = 1;
			Item.consumable = false;
			Item.value = Item.buyPrice(0, 1);
			Item.width = 11;
			Item.height = 11;
			Item.rare = ItemRarityID.LightPurple;
			Item.value = 1;
			Item.UseSound = SoundID.NPCHit5;
		}

		public override bool CanUseItem(Player player) {
			return !NPC.AnyNPCs(ModContent.NPCType<FrostNova>()) && Main.player[Main.myPlayer].ZoneSnow;
		}

		public override bool? UseItem(Player player) {
			int randod = Main.rand.NextBool() ? 1 : -1;
			NPC.NewNPC(Terraria.Entity.GetSource_TownSpawn(), (int)player.Center.X + randod * Main.rand.Next(64,129), (int)player.Center.Y, NPCType<FrostNova>());
			return true;
		}

        public override void AddRecipes()
		{
            Recipe recipe1 = CreateRecipe();
            recipe1.AddIngredient(ItemID.SpicyPepper, 1);
            recipe1.AddIngredient(ItemID.Shiverthorn, 1);
            recipe1.AddIngredient(ItemID.Ale, 1);
            recipe1.Register();
        }
    }
}