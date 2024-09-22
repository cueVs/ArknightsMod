using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.Localization;
using ArknightsMod.Content.NPCs.Enemy.RoaringFlare.ImperialArtilleyCoreTargeteer;

namespace ArknightsMod.Content.Items.Summon
{
	public class IACTSummon : ModItem
	{
		public override void SetStaticDefaults() 
        {
			ItemID.Sets.SortingPriorityBossSpawns[Item.type] = 12;
		}

		public override void SetDefaults()
        {
			Item.width = 38;
			Item.height = 40;
			Item.maxStack = 1;
			Item.rare = 6;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useStyle = 4;
			Item.consumable = false;
            Item.noUseGraphic = true;
            Item.scale = 0.01f;
            Item.UseSound = new SoundStyle($"{nameof(ArknightsMod)}/Assets/Sound/ImperialArtilleyCoreTargeteer/airstrike");
		}

        public override bool CanUseItem(Player player)
        {
            if (!Main.dayTime && !NPC.AnyNPCs(ModContent.NPCType<Content.NPCs.Enemy.RoaringFlare.ImperialArtilleyCoreTargeteer.IACT>()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

		public override bool? UseItem(Player player) {
			NPC.NewNPC(Terraria.Entity.GetSource_TownSpawn(), (int)player.Center.X, (int)player.Center.Y - 800, NPCType<IACT>());
			return true;
		}

		public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<Content.Items.Material.IncandescentAlloyBlock>(), 3);
			recipe.AddIngredient(ModContent.ItemType<Content.Items.Material.CrystallineCircuit>(), 3);
			recipe.AddIngredient(ModContent.ItemType<Content.Items.Material.OptimizedDevice>(), 3);
			recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
	}
}