using ArknightsMod.Common.Items;
using ArknightsMod.Content.Items;
using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Content.NPCs.Friendly;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI;
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
using Humanizer;
using ReLogic.Content;
using ArknightsMod.Assets.Effects;

namespace ArknightsMod
{
	public class ArknightsMod : Mod
	{
		public static int OrundumCurrencyId;
		internal Closure.AOSystem CurrentAO;
		public const string noTexture = "ArknightsMod/Assets/null";//空材质
		public static Effect IACTSW;//冲击波涟漪效果shader（如IACT）
		public static Effect AACTTP;//缩小效果shader（AACT传送）
		public static Effect AACTOC;//变色效果shader（AACT）
		public static Effect AACTOC2;//反色效果shader（AACT转阶段）
		public static Effect LightRing;//光环shader（AACT二阶段）
		public static Effect CollapsedExplosionPart1;//坍缩爆炸效果（内核）（AACT二阶段）
		public static Effect CollapsedExplosionPart2;//坍缩爆炸效果（描边）（AACT二阶段）
		public static Effect AACTSTG3RBFence;//红蓝光栅效果（AACT三阶段）
		public static Effect AACTSTG3RBNoise;//红蓝噪声效果（AACT三阶段）
		public static Effect FNTwistedRing;//霜星限制阈（扭曲环效果）

		public override void Load() {
			UpgradeItemBase.LoadLevelData(this);
			UpgradeWeaponBase.LoadSkillData(this);
			// Registers a new custom currency
			OrundumCurrencyId = CustomCurrencyManager.RegisterCurrency(new Content.Currencies.OrundumCurrency(ModContent.ItemType<Content.Items.Orundum>(), 9999L, "Mods.ArknightsMod.Currencies.OrundumCurrency"));
			//shader
			if (Main.netMode != NetmodeID.Server) {
				IACTSW = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/IACTSW", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Filters.Scene["IACTSW"] = new Filter(new ScreenShaderData(new Ref<Effect>(IACTSW), "IACTSW"), EffectPriority.VeryHigh);
				Filters.Scene["IACTSW"].Load();

				AACTTP = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/AACTTP", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Filters.Scene["AACTTP"] = new Filter(new ScreenShaderData(new Ref<Effect>(AACTTP), "AACTTP"), EffectPriority.VeryHigh);
				Filters.Scene["AACTTP"].Load();

				AACTOC = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/AACTOC", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Filters.Scene["AACTOC"] = new Filter(new ScreenShaderData(new Ref<Effect>(AACTOC), "AACTOC"), EffectPriority.VeryHigh);
				Filters.Scene["AACTOC"].Load();

				AACTOC2 = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/AACTOC2", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Filters.Scene["AACTOC2"] = new Filter(new ScreenShaderData(new Ref<Effect>(AACTOC2), "AACTOC2"), EffectPriority.VeryHigh);
				Filters.Scene["AACTOC2"].Load();

				LightRing = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/LightRing", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Filters.Scene["LightRing"] = new Filter(new ScreenShaderData(new Ref<Effect>(LightRing), "LightRing"), EffectPriority.VeryHigh);
				Filters.Scene["LightRing"].Load();

				CollapsedExplosionPart1 = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/CollapsedExplosionPart1", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Filters.Scene["CollapsedExplosionPart1"] = new Filter(new ScreenShaderData(new Ref<Effect>(CollapsedExplosionPart1), "CollapsedExplosionPart1"), EffectPriority.VeryHigh);
				Filters.Scene["CollapsedExplosionPart1"].Load();

				CollapsedExplosionPart2 = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/CollapsedExplosionPart2", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Filters.Scene["CollapsedExplosionPart2"] = new Filter(new ScreenShaderData(new Ref<Effect>(CollapsedExplosionPart2), "CollapsedExplosionPart2"), EffectPriority.VeryHigh);
				Filters.Scene["CollapsedExplosionPart2"].Load();

				AACTSTG3RBFence = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/AACTSTG3RBFence", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Filters.Scene["AACTSTG3RBFence"] = new Filter(new ScreenShaderData(new Ref<Effect>(AACTSTG3RBFence), "AACTSTG3RBFence"), EffectPriority.VeryHigh);
				Filters.Scene["AACTSTG3RBFence"].Load();

				AACTSTG3RBNoise = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/AACTSTG3RBNoise", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Filters.Scene["AACTSTG3RBNoise"] = new Filter(new ScreenShaderData(new Ref<Effect>(AACTSTG3RBNoise), "AACTSTG3RBNoise"), EffectPriority.VeryHigh);
				Filters.Scene["AACTSTG3RBNoise"].Load();

				/*FNTwistedRing = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/FNTwistedRing", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
				Filters.Scene["FNTwistedRing"] = new Filter(new ScreenShaderData(new Ref<Effect>(FNTwistedRing), "FNTwistedRing"), EffectPriority.VeryHigh);
				Filters.Scene["FNTwistedRing"].Load();*/
			}
			Filters.Scene["AshStorm"] = new Filter(
		   new ScreenShaderData("FilterAsh").UseColor(1f, 0.8f, 0.5f),
		   EffectPriority.High
	   );

			LoadClient();
			SkyManager.Instance["ArknightsMod:UnionInvadeSky"] = new UnionInvadeSky();




			MusicLoader.AddMusic(this, "Assets/OriginalMusic/AACTintro");
			MusicLoader.AddMusic(this, "Assets/OriginalMusic/AACTloop");
		}
		public static Texture2D UnionInvadeSkyTexture { get; private set; }

		private void LoadClient() {
			// 强制立即加载天空纹理（仿照灾厄）
			UnionInvadeSkyTexture = ModContent.Request<Texture2D>(
				"ArknightsMod/Content/Events/UnionInvadeSky",
				AssetRequestMode.ImmediateLoad
			).Value;

			// 调试验证
			if (UnionInvadeSkyTexture == null || UnionInvadeSkyTexture.IsDisposed)
				Logger.Error("天空纹理加载失败！");
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
				Player.Hurt(PlayerDeathReason.ByCustomReason("精神崩溃"), 200, 1, false, false, 0, true, 1000, 1000, 0);
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

