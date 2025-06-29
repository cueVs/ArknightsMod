using ArknightsMod.Content.NPCs.Enemy.RoaringFlare.ImperialArtilleyCoreTargeteer;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Summon
{
	public class IACTSummon : ModItem
	{
		private static SoundStyle use;
		public override void Load() => use = new SoundStyle($"{nameof(ArknightsMod)}/Assets/Sound/ImperialArtilleyCoreTargeteer/airstrike");
		public override void SetStaticDefaults() {
			ItemID.Sets.SortingPriorityBossSpawns[Item.type] = 12;
		}

		public override void SetDefaults() {
			Item.width = 38;
			Item.height = 40;
			Item.maxStack = 1;
			Item.rare = ItemRarityID.LightPurple;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.consumable = false;
			Item.noUseGraphic = true;
			Item.scale = 0.01f;
			Item.UseSound = use;
		}

		public override bool CanUseItem(Player player) {
			return !Main.dayTime && !NPC.AnyNPCs(ModContent.NPCType<IACT>());
		}

		public override bool? UseItem(Player player) {
			int IACTboss = NPC.NewNPC(Terraria.Entity.GetSource_TownSpawn(), (int)player.Center.X, (int)player.Center.Y - 800, ModContent.NPCType<IACT>());
			Main.npc[IACTboss].netUpdate = true;
			Main.NewText(Language.GetTextValue("Mods.ArknightsMod.StatusMessage.IACT.Summon"), 138, 0, 18);
			return true;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<Material.IncandescentAlloyBlock>(), 3);
			recipe.AddIngredient(ModContent.ItemType<Material.CrystallineCircuit>(), 3);
			recipe.AddIngredient(ModContent.ItemType<Material.OptimizedDevice>(), 3);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
	}
}