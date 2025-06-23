using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using ArknightsMod.Content.NPCs;
using ArknightsMod.Content.NPCs.Enemy.ThroughChapter4;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent;
using Terraria.UI;
using static EventProgressBar;
using Humanizer;
using Terraria.Graphics.Effects;
using MonoMod.Cil;
using Terraria.Graphics.Shaders;
using ReLogic.Content;
using ArknightsMod.Assets.Effects;



namespace ArknightsMod.Content.Events
{
	public class UnionInvade : ModSystem {
		// 事件状态
		public static bool EventActive = false;
		public static int MonstersKilled = 0;
		public const int MonstersRequired = 400;
		private Asset<Texture2D> CustomBg;
		// 事件怪物列表（简化版）
		public override void Load() {
			var sky = new UnionInvadeSky();
			Mod.Logger.Debug($"天空类标识符: {sky}"); // 检查ToString()输出
		}

		public static readonly List<int> EventMonsters = new List<int>()
		{
			ModContent.NPCType<Soldier>(),         // 僵尸
			ModContent.NPCType<Hound>(),
			ModContent.NPCType<Crossbowman>(),
			ModContent.NPCType<Seniorcaster>(),// 鹰身女妖
			ModContent.NPCType<MortarGunner>(),
			ModContent.NPCType<Drone>(),
		};

		public override void OnWorldLoad() {
			EventActive = false;
			MonstersKilled = 0;
		}

		public override void SaveWorldData(TagCompound tag) {
			if (EventActive) {
				tag["UnionInvadeActive"] = false;
				tag["MonstersKilled"] = MonstersKilled;
			}
		}

		public override void LoadWorldData(TagCompound tag) {
			if (tag.ContainsKey("UnionInvadeActive")) {
				EventActive = tag.GetBool("UnionInvadeActive");
				MonstersKilled = tag.GetInt("MonstersKilled");
			}
		}

		public static void StartEvent() {
			EventActive = true;
			var uiSystem = ModContent.GetInstance<EventUISystem>();

			// 重置进度条位置
			uiSystem.progressBar.ResetPosition();
			ModContent.GetInstance<EventUISystem>().eventInterface?.SetState(
	   ModContent.GetInstance<EventUISystem>().progressBar);
			MonstersKilled = 0;
			Main.NewText("识别入侵者身份“整合运动”，启动全舰防御", Color.Orange);
			Main.curMusic = MusicLoader.GetMusicSlot("ArknightsMod/Music/UnionInvade");

		}

		public static void EndEvent() {
			EventActive = false;
			Main.NewText("入侵已被击退！", Color.Lime);
			SkyManager.Instance.Deactivate("ArknightsMod:UnionInvadeSky");
			// 发放奖励
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				foreach (Player player in Main.player) {
					if (player.active) {
						player.QuickSpawnItem(null, ItemID.GoldCoin, 10);
					}
				}
			}
		}

		public static void OnNPCKilled(NPC npc) {
			if (EventActive && EventMonsters.Contains(npc.type)) {
				MonstersKilled++;

				// 每50个提示进度
				if (MonstersKilled % 50 == 0) {
					Main.NewText($"歼灭进度: {MonstersKilled}/{MonstersRequired}", Color.Yellow);
				}
				if (MonstersKilled == 100) {
					Main.NewText($"检测到敌人的远程单位加入作战。", Color.Orange);
				}
				if (MonstersKilled == 150) {
					Main.NewText($"敌人的空中火力已经抵达。", Color.Orange);
				}
				if (MonstersKilled == 250) {
					Main.NewText($"检测到敌方的超远程火力支援，请注意隐蔽。", Color.Orange);
				}
				if (MonstersKilled == 300) {
					Main.NewText($"前方出现敌方高威胁法术作战单位，请小心应对。", Color.Orange);
				}
				// 检查是否完成
				if (MonstersKilled >= MonstersRequired) {
					EndEvent();
				}
			}
		}
		public class UnionInvadeMusic : ModSceneEffect {
			public override int Music => MusicLoader.GetMusicSlot(Mod, "Music/UnionInvade");
			public override SceneEffectPriority Priority => SceneEffectPriority.Event;

			public override bool IsSceneEffectActive(Player player) {
				return UnionInvade.EventActive;
			}
		}

	}
	public class UnionInvadeVisuals : ModSystem {
		private static Texture2D customBgTexture; // 手绘背景图
		private static RenderTarget2D screenFilter;

		public override void Load() {
			if (Main.dedServ)
				return;

			// 加载手绘背景图

			// 创建屏幕滤镜
			Main.QueueMainThreadAction(() => {
				screenFilter = new RenderTarget2D(
					Main.graphics.GraphicsDevice,
					Main.screenWidth,
					Main.screenHeight
				);
			});

			// 注册滤镜着色器
			Filters.Scene["UnionInvadeFilter"] = new Filter(
				new ScreenShaderData("FilterUnionInvade").UseColor(1f, 0.8f, 0.3f),
				EffectPriority.High
			);
		}
		public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor) {
			if (UnionInvade.EventActive) 
			{
				SkyManager.Instance.Activate("ArknightsMod:UnionInvadeSky");

				backgroundColor = Color.Lerp(backgroundColor, new Color(255, 200, 100), 0.7f);
				tileColor = Color.Lerp(backgroundColor, new Color(255, 200, 100), 0.7f);
			}
		}
		public override void PostUpdateEverything() {
			bool isEventActive = UnionInvade.EventActive;

			// 动态控制滤镜强度
			if (isEventActive) {
				Filters.Scene["UnionInvadeFilter"].GetShader().UseIntensity(0.5f);
				Filters.Scene["UnionInvadeFilter"].Activate(Main.LocalPlayer.Center,
					new object[] { 0.5f }); // 传递着色器参数

				// 生成火星粒子
				if (Main.rand.NextBool(8)) {
					Dust dust = Dust.NewDustPerfect(
				Main.LocalPlayer.Center + new Vector2(Main.rand.Next(-1000, 1000), -500),
				DustID.Torch,
				new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(1f, 3f)),
				0, default, 2.5f
);


					
					dust.fadeIn = 1.8f;// 淡入效果

				}
			}
			else {
				Filters.Scene["UnionInvadeFilter"].Deactivate();
			}
		}
		
	}
	

}

