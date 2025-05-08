using ArknightsMod.Content.NPCs.Friendly;
using Terraria.ModLoader;
using Terraria.GameContent.UI;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria;
using ArknightsMod.Content.NPCs.Enemy.Seamonster;
using Terraria.Audio;
using Terraria.DataStructures;
using System;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using ArknightsMod.Content.Projectiles;
using ArknightsMod.Common.UI;
using Terraria.UI;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using ArknightsMod.Content.Buffs;

namespace ArknightsMod
{
	public class ArknightsMod : Mod
	{
		public static int OrundumCurrencyId;
		internal Closure.AOSystem CurrentAO;
		public static Effect IACTSW;




		public override void Load() {
			// Registers a new custom currency
			OrundumCurrencyId = CustomCurrencyManager.RegisterCurrency(new Content.Currencies.OrundumCurrency(ModContent.ItemType<Content.Items.Orundum>(), 9999L, "Mods.ArknightsMod.Currencies.OrundumCurrency"));
			//shader
			if (Main.netMode != NetmodeID.Server) {
				IACTSW = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/IACTSW", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Filters.Scene["IACTSW"] = new Filter(new ScreenShaderData(new Ref<Effect>(IACTSW), "IACTSW"), EffectPriority.VeryHigh);
				Filters.Scene["IACTSW"].Load();
			}







		}


	}
	//public class Ex : GlobalNPC
	//{
	//public override void SetDefaults(NPC entity) {
	//if (entity.ModNPC is not null && entity.ModNPC.Mod == Mod) {
	//entity.lifeMax = (int)(entity.lifeMax * 1.2f);
	//entity.life = entity.lifeMax;
	//entity.damage = (int)(entity.damage * 1.2f);
	//}
	//}
	//}
	public class SanUI : ModSystem
	{
		internal Santable santable;
		internal UserInterface sanUserInterface;
		public override void Load() {
			santable = new Santable();
			santable.Activate();
			sanUserInterface = new UserInterface();
			sanUserInterface.SetState(santable);
		}
		public override void UpdateUI(GameTime gameTime) {
			if (Santable.Visible) {
				sanUserInterface?.Update(gameTime);
			}
			base.UpdateUI(gameTime);
		}
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int MouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (MouseTextIndex != -1) {
				layers.Insert(MouseTextIndex, new LegacyGameInterfaceLayer(
					"ArknightsMod : Santable",
					delegate {
						if (Santable.Visible)
							santable.Draw(Main.spriteBatch);
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
			base.ModifyInterfaceLayers(layers);
		}

	}
	
	public class RAfood : ModPlayer {
		public static List<int> RAfoodBuff = [ModContent.BuffType<RAMeatchipBuff>(), ModContent.BuffType<RARicecrabBuff>()];
	}


	public class San : ModPlayer
	{
		public int CurrentSan = 1000;
		public int MadnessCD = 600;
		public int madtime;
		public int madframe;

		public override void PreUpdate() {
			MadnessCD++;
			var newSource = Player.GetSource_FromThis();
			if (CurrentSan <= 0) {
				Player.AddBuff(31, 90);
				Player.AddBuff(23, 240);
				Player.Hurt(PlayerDeathReason.ByCustomReason("¾«Éñ±ÀÀ£"), 200, 1, false, false, 0, true, 1000, 1000, 0);
				CurrentSan = 1000;
				SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/Madness") with { Volume = 1f, Pitch = 0f }, Player.Center);
				MadnessCD = 0;
				Projectile.NewProjectile(newSource, Player.Center + new Vector2(100, 180), new Vector2(0, 0), ModContent.ProjectileType<SanCrash>(), 0, 0);

			}

			foreach (var npc in Main.ActiveNPCs) {

				if (npc.type == ModContent.NPCType<BasinSeaReaper>() && ((float)Math.Pow((npc.position.X - Player.position.X), 2) + (float)Math.Pow((npc.position.Y - Player.position.Y), 2) <= 36000) && npc.life <= npc.lifeMax * 0.99 && MadnessCD > 600) {
					CurrentSan -= 5;

				}

			}
			if (MadnessCD % 2 == 0 && CurrentSan < 1000) {
				CurrentSan += 1;
			}
		}
		public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) {
			if (npc.type == ModContent.NPCType<DeepSeaSlider>() && MadnessCD > 600) {
				CurrentSan -= 100;
			}
			if (npc.type == ModContent.NPCType<TheFirstToTalk>() && MadnessCD > 600) {
				CurrentSan -= 300;
				if (Main.expertMode) {
					CurrentSan -= 50;
				}
			}
		}
		public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo) {
			if (proj.type == ModContent.ProjectileType<TFTTShoot>() && MadnessCD > 600) {
				CurrentSan -= 400;
				if (Main.expertMode) {
					CurrentSan -= 100;
				}
			}
			if (proj.type == ModContent.ProjectileType<seashoot>() && MadnessCD > 600) {
				CurrentSan -= 300;
			}
			if (proj.type == ModContent.ProjectileType<TFTTSkillshoot>() && MadnessCD > 600) {
				CurrentSan -= 300;
			}
			if (proj.type == ModContent.ProjectileType<TFTTRush2>() && MadnessCD > 600) {
				CurrentSan -= 300;
			}
			if (proj.type == ModContent.ProjectileType<TFTTRush>() && MadnessCD > 600) {
				CurrentSan -= 300;
			}
			if (proj.type == ModContent.ProjectileType<PocketSeaCrawlerShoot>() && MadnessCD > 600) {
				CurrentSan -= 300;
			}
			if (proj.type == ModContent.ProjectileType<PocketSeaCrawlerShoot2>() && MadnessCD > 600) {
				CurrentSan -= 300;
			}
		}
		public override void OnEnterWorld() {
			Santable.Visible = true;
		}
	}
}

