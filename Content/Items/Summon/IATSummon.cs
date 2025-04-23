//using ArknightsMod.Content.NPCs;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using System;
//using System.IO;
using Terraria.Localization;
//using System.Collections.Generic;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
//using Terraria.DataStructures;

namespace ArknightsMod.Content.Items.Summon
{
	public class IATSummon : ModItem
	{
		public override void SetStaticDefaults() {
			ItemID.Sets.SortingPriorityBossSpawns[Item.type] = 12;
		}

		public override void SetDefaults() {
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

		public override bool CanUseItem(Player player) {
			if (!Main.dayTime && !NPC.AnyNPCs(ModContent.NPCType<Content.NPCs.Enemy.RoaringFlare.ImperialArtilleyCoreTargeteer.IACT>())) {
				return true;
			}
			else {
				return false;
			}
		}

		public override bool? UseItem(Player player) {
			int IACTboss = NPC.NewNPC(Terraria.Entity.GetSource_TownSpawn(), (int)player.Center.X, (int)player.Center.Y - 800, NPCType<Content.NPCs.Enemy.RoaringFlare.ImperialArtilleyCoreTargeteer.IAT>());
			Main.npc[IACTboss].netUpdate = true;
			Main.NewText(Language.GetTextValue("Mods.ArknightsMod.StatusMessage.IACT.Summon"), 138, 0, 18);
			return true;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<Content.Items.Material.IncandescentAlloy>(), 3);
			recipe.AddIngredient(ModContent.ItemType<Content.Items.Material.CrystallineComponent>(), 3);
			recipe.AddIngredient(ModContent.ItemType<Content.Items.Material.IntegratedDevice>(), 3);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}