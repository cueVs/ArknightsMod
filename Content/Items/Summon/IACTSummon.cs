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
//using Terraria.DataStructures;

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
            Item.UseSound = new SoundStyle($"{nameof(ArknightsMod)}/Assets/Sounds/ImperialArtilleyCoreTargeteer/airstrike");
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

        public override bool? UseItem(Player player) 
        {
            int IACTboss = NPC.NewNPC(Terraria.Entity.GetSource_TownSpawn(),(int)player.Center.X, (int)player.Center.Y - 800, NPCType<Content.NPCs.Enemy.RoaringFlare.ImperialArtilleyCoreTargeteer.IACT> ());
            Main.npc[IACTboss].netUpdate = true;
            Main.NewText(Language.GetTextValue("Mods.ArknightsMod.StatusMessage.IACT.Summon"), 138, 0, 18);
			return true;
		}

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.IronBar, 25);
            recipe.AddIngredient(ItemID.Wire, 100);
            recipe.AddIngredient(ItemID.ExplosivePowder, 50);
            recipe.AddIngredient(ItemID.SoulofLight, 20);
            recipe.AddIngredient(ItemID.SoulofNight, 20);
            recipe.AddIngredient(ItemID.SoulofFlight, 20);
            recipe.AddIngredient(ItemID.SoulofMight, 10);
            recipe.AddIngredient(ItemID.SoulofSight, 10);
            recipe.AddIngredient(ItemID.SoulofFright, 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
	}
}