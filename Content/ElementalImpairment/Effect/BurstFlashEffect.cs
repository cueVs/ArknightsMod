using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.ElementalImpairment.Effect
{
	//这个是关于元素损伤爆条之后产生那个光圈的实现方法
	public static class BurstFlashEffect
	{
		private static List<FlashInstance> flashes = new();

		public static void Play(NPC npc, Vector2 worldPosition,
			string mainTex, string featherTex,
			Color mainColor, Color featherColor) {
			if (npc == null || string.IsNullOrEmpty(mainTex) || string.IsNullOrEmpty(featherTex))
				return;

			Vector2 offset = worldPosition - npc.Center;
			flashes.Add(new FlashInstance {
				Npc = npc,
				Offset = offset,
				MainTexPath = mainTex,
				FeatherTexPath = featherTex,
				MainColor = mainColor,
				FeatherColor = featherColor,
				StartTime = (float)Main.GlobalTimeWrappedHourly,
				Duration = 0.75f,
				MainStartScale = 0.18f,
				MainEndScale = 0.255f,
				FeatherStartScale = 0.17f,
				FeatherEndScale = 0.325f
			});
		}

		public static void UpdateAndDraw(GameTime gameTime) {
			if (flashes.Count == 0)
				return;

			SpriteBatch sb = Main.spriteBatch;
			float now = (float)Main.GlobalTimeWrappedHourly;

			for (int i = flashes.Count - 1; i >= 0; i--) {
				var flash = flashes[i];
				float elapsed = now - flash.StartTime;
				if (elapsed < 0)
					elapsed += 3600f;

				if (elapsed >= flash.Duration) {
					flashes.RemoveAt(i);
					continue;
				}

				if (flash.Npc != null && flash.Npc.active)
					flash.CurrentWorldPos = flash.Npc.Center + flash.Offset;
				else if (!flash.PositionStored) {
					flash.CurrentWorldPos = flash.Npc?.Center + flash.Offset ?? flash.CurrentWorldPos;
					flash.PositionStored = true;
				}

				flashes[i] = flash;
			}

			if (flashes.Count == 0)
				return;

			sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
					 DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			for (int i = flashes.Count - 1; i >= 0; i--) {
				var flash = flashes[i];
				float elapsed = now - flash.StartTime;
				if (elapsed < 0)
					elapsed += 3600f;
				if (elapsed >= flash.Duration)
					continue;

				float progress = elapsed / flash.Duration;
				float alpha = 1f - progress;

				Texture2D featherTex = ModContent.Request<Texture2D>(flash.FeatherTexPath).Value;
				Texture2D mainTex = ModContent.Request<Texture2D>(flash.MainTexPath).Value;
				Vector2 screenPos = flash.CurrentWorldPos - Main.screenPosition;


				float featherEased = 1f - (float)Math.Pow(1f - progress, 3);
				float featherScale = MathHelper.Lerp(flash.FeatherStartScale, flash.FeatherEndScale, featherEased);
				Color featherCol = new Color(flash.FeatherColor.R, flash.FeatherColor.G, flash.FeatherColor.B,
					(int)(flash.FeatherColor.A * alpha));
				sb.Draw(featherTex, screenPos, null, featherCol, 0f,
					featherTex.Size() * 0.5f, featherScale, SpriteEffects.None, 0);


				float mainScale = MathHelper.Lerp(flash.MainStartScale, flash.MainEndScale, progress);
				Color mainCol = new Color(flash.MainColor.R, flash.MainColor.G, flash.MainColor.B,
					(int)(flash.MainColor.A * alpha));
				sb.Draw(mainTex, screenPos, null, mainCol, 0f,
					mainTex.Size() * 0.5f, mainScale, SpriteEffects.None, 0);
			}

			sb.End();
		}

		private struct FlashInstance
		{
			public NPC Npc;
			public Vector2 Offset;
			public Vector2 CurrentWorldPos;
			public string MainTexPath;
			public string FeatherTexPath;
			public Color MainColor;
			public Color FeatherColor;
			public float StartTime;
			public float Duration;
			public float MainStartScale;
			public float MainEndScale;
			public float FeatherStartScale;
			public float FeatherEndScale;
			public bool PositionStored;
		}
	}

	public class BurstFlashSystem : ModSystem
	{
		public override void Load() => Main.OnPostDraw += BurstFlashEffect.UpdateAndDraw;
		public override void Unload() => Main.OnPostDraw -= BurstFlashEffect.UpdateAndDraw;
	}
}