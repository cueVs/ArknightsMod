//using ArknightsMod.Content.NPCs;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
//using System.Collections.Generic;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using System;
//using System.IO;
using Terraria.Localization;
using System.Runtime.InteropServices;
//using Terraria.DataStructures;

namespace ArknightsMod.Content.Items.Summon
{
	public class AACTSummon : ModItem
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
			Item.useAnimation = 330;
			Item.useTime = 330;
			Item.useStyle = 4;
			Item.consumable = false;
            Item.noUseGraphic = true;
            Item.scale = 0.01f;
            Item.UseSound = new SoundStyle($"{nameof(ArknightsMod)}/Assets/Sound/ImperialArtilleyCoreTargeteer/pollutedairstrike");
		}

        public override bool CanUseItem(Player player)
        {
            if (!Main.dayTime && !NPC.AnyNPCs(ModContent.NPCType<Content.NPCs.Enemy.RoaringFlare.ImperialArtilleyCoreTargeteer.AACTIntro>()) && !NPC.AnyNPCs(ModContent.NPCType<Content.NPCs.Enemy.RoaringFlare.ImperialArtilleyCoreTargeteer.AACT>()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

		private float timer;

        public override bool? UseItem(Player player) 
        {
			int IACTboss = NPC.NewNPC(Terraria.Entity.GetSource_TownSpawn(), (int)player.Center.X, (int)player.Center.Y, NPCType<Content.NPCs.Enemy.RoaringFlare.ImperialArtilleyCoreTargeteer.AACTIntro>());
			Main.npc[IACTboss].netUpdate = true;
			return true;
		}

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<Content.Items.Material.IncandescentAlloyBlock>(), 5);
			recipe.AddIngredient(ModContent.ItemType<Content.Items.Material.CrystallineCircuit>(), 5);
			recipe.AddIngredient(ModContent.ItemType<Content.Items.Material.OptimizedDevice>(), 5);
			recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
	}
}