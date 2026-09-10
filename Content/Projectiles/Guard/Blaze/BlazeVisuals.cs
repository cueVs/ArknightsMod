using System;
using ArknightsMod.Common.Particle;
using ArknightsMod.Common.VisualEffects;
using ArknightsMod.Content.Projectiles.BasePROJ;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Blaze
{
	/// <summary>
	/// 煌链锯专用的程序化视觉层。所有颜色都保留有效 Alpha；调用 Draw* 前需要已经切到 Additive。
	/// </summary>
	internal static class BlazeVisuals
	{
		private static readonly Rectangle PixelSource = new(0, 0, 1, 1);

		internal static SoundStyle ButcherMotorSound =>
			ContentSamples.ItemsByType[ItemID.ButchersChainsaw].UseSound ?? SoundID.Item22;

		internal static void DrawBladeHeat(Vector2 handWorld, Vector2 tipWorld, float heat,
			float chainPhase, float opacity = 1f, bool extended = false) {
			if (Main.dedServ || opacity <= 0f)
				return;

			Texture2D glow = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
			Texture2D chainArc = ModContent.Request<Texture2D>(
				"ArknightsMod/Content/Textures/spark_07").Value;
			Texture2D heatTrace = ModContent.Request<Texture2D>(
				"ArknightsMod/Content/Textures/trace_03").Value;
			Texture2D flare = ModContent.Request<Texture2D>(
				"ArknightsMod/Content/Textures/flare_01").Value;
			Vector2 start = handWorld;
			Vector2 end = tipWorld;
			Vector2 delta = end - start;
			float length = delta.Length();
			if (length < 2f)
				return;

			Vector2 dir = delta / length;
			Vector2 normal = new(-dir.Y, dir.X);
			float rotation = dir.ToRotation();
			float hot = MathHelper.Clamp(heat, 0f, 1f);
			float alpha = MathHelper.Clamp(opacity, 0f, 1f);
			float pulse = 0.88f + MathF.Sin(chainPhase * MathHelper.TwoPi) * 0.12f;

			// 以真实链锯长度为骨架铺一层纹理化链轨能量，不再用程序矩形替代电锯贴图。
			Vector2 chainCenter = Vector2.Lerp(start, end, 0.63f) - Main.screenPosition;
			float chainScaleX = length / Math.Max(1f, chainArc.Width) * (extended ? 0.92f : 0.82f);
			float chainScaleY = (extended ? 0.09f : 0.066f) * (0.82f + hot * 0.28f);
			Main.spriteBatch.Draw(chainArc, chainCenter, null,
				new Color(185, 0, 28, 130) * (alpha * pulse), rotation,
				chainArc.Size() * 0.5f, new Vector2(chainScaleX, chainScaleY * 1.65f),
				SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(chainArc, chainCenter + normal * 0.8f, null,
				new Color(255, 96, 28, 105) * (alpha * hot), rotation,
				chainArc.Size() * 0.5f, new Vector2(chainScaleX * 0.96f, chainScaleY * 0.68f),
				SpriteEffects.None, 0f);

			// 三簇短促热裂纹附着在锯齿上，并随链相位移动，形成真正“在运转”的层次。
			for (int i = 0; i < (extended ? 4 : 3); i++) {
				float u = (i + 0.35f + chainPhase * 0.27f) / (extended ? 4f : 3f);
				u -= MathF.Floor(u);
				Vector2 world = start + dir * (length * MathHelper.Lerp(0.34f, 0.96f, u));
				world += normal * (((i & 1) == 0 ? 1f : -1f) * (2.8f + hot * 1.5f));
				float traceScale = (0.045f + hot * 0.018f) * (extended ? 1.18f : 1f);
				Main.spriteBatch.Draw(heatTrace, world - Main.screenPosition, null,
					new Color(255, 18, 40, 118) * (alpha * pulse), rotation + MathHelper.PiOver2,
					heatTrace.Size() * 0.5f, new Vector2(traceScale * 0.32f, traceScale),
					SpriteEffects.None, 0f);
			}

			// 不再用 MagicPixel 画一把“霓虹长条电锯”。这里只在真实贴图的锯刃上安排
			// 少量沿链条滚动的 SharpTears 热点，让光来自齿轨而不是替代机械轮廓。
			int teeth = extended ? 7 : 5;
			float scroll = chainPhase - MathF.Floor(chainPhase);
			for (int i = 0; i < teeth; i++) {
				float u = (i + scroll) / teeth;
				if (u > 1f)
					u -= 1f;
				Vector2 tooth = start + dir * (length * MathHelper.Lerp(0.28f, 0.98f, u));
				float side = ((i & 1) == 0 ? 1f : -1f) * (2.2f + hot * 1.8f);
				tooth += normal * side;
				Color toothColor = Color.Lerp(new Color(224, 28, 39), new Color(255, 226, 158), hot);
				BaseHeldMeleeSupport.DrawSharpTear(tooth, rotation, toothColor,
					MathHelper.Lerp(13f, 22f, hot), MathHelper.Lerp(3.5f, 6.5f, hot),
					(0.34f + hot * 0.42f) * alpha);
			}

			float glowScale = 0.11f + hot * 0.12f;
			Main.spriteBatch.Draw(glow, end - Main.screenPosition, null, new Color(218, 12, 32) * (0.42f * alpha),
				0f, glow.Size() * 0.5f, glowScale * 1.5f, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(glow, end - Main.screenPosition, null, new Color(255, 118, 30) * (0.5f * alpha),
				0f, glow.Size() * 0.5f, glowScale, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(flare, end - Main.screenPosition, null,
				new Color(255, 32, 48, 125) * (alpha * pulse), rotation,
				flare.Size() * 0.5f, new Vector2((extended ? 0.18f : 0.13f) * (0.8f + hot * 0.3f),
					(extended ? 0.09f : 0.065f)), SpriteEffects.None, 0f);
		}

		internal static void DrawBurstTextures(Vector2 worldCenter, float progress,
			float rotation, bool finalBurst) {
			if (Main.dedServ)
				return;

			float t = MathHelper.Clamp(progress, 0f, 1f);
			float fade = 1f - t;
			Texture2D ring = ModContent.Request<Texture2D>(
				"ArknightsMod/Content/Textures/circle_03").Value;
			Texture2D core = ModContent.Request<Texture2D>(
				"ArknightsMod/Content/Textures/muzzle_02").Value;
			Vector2 center = worldCenter - Main.screenPosition;
			float ringScale = MathHelper.Lerp(finalBurst ? 0.1f : 0.055f,
				finalBurst ? 0.58f : 0.25f, MathF.Sqrt(t));
			Main.spriteBatch.Draw(ring, center, null,
				new Color(232, 4, 38, 145) * (fade * fade), rotation,
				ring.Size() * 0.5f, ringScale, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(ring, center, null,
				new Color(255, 149, 40, 105) * fade, -rotation * 0.7f,
				ring.Size() * 0.5f, ringScale * 0.72f, SpriteEffects.None, 0f);

			float coreEnvelope = finalBurst
				? MathF.Pow(fade, 1.35f)
				: MathF.Sin(MathHelper.Pi * t) * fade;
			float coreScale = (finalBurst ? 0.48f : 0.24f) * (0.72f + t * 0.55f);
			Main.spriteBatch.Draw(core, center, null,
				new Color(255, 18, 42, 125) * coreEnvelope, rotation - MathHelper.PiOver2,
				core.Size() * 0.5f, new Vector2(coreScale * 1.05f, coreScale),
				SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(core, center, null,
				new Color(255, 202, 92, 95) * coreEnvelope, rotation - MathHelper.PiOver2,
				core.Size() * 0.5f, new Vector2(coreScale * 0.48f, coreScale * 0.62f),
				SpriteEffects.None, 0f);
		}

		internal static void DrawScarletTears(Vector2 worldCenter, float rotation, float scale, float opacity) {
			if (Main.dedServ || opacity <= 0f)
				return;

			float pulse = 0.9f + MathF.Sin((float)Main.GlobalTimeWrappedHourly * 13f) * 0.1f;
			BaseHeldMeleeSupport.DrawSharpTear(worldCenter, rotation, new Color(255, 20, 38),
				112f * scale * pulse, 34f * scale, opacity);
			BaseHeldMeleeSupport.DrawSharpTear(worldCenter, rotation, new Color(255, 179, 74),
				82f * scale * pulse, 11f * scale, opacity * 0.62f);
		}

		internal static void DrawRing(Vector2 worldCenter, float radius, float width, Color color,
			float opacity, int segments = 48, float rotation = 0f) {
			if (Main.dedServ || radius <= 1f || width <= 0f || opacity <= 0f)
				return;

			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Color drawColor = color * MathHelper.Clamp(opacity, 0f, 1f);
			Vector2 center = worldCenter - Main.screenPosition;
			Vector2 previous = center + rotation.ToRotationVector2() * radius;
			for (int i = 1; i <= segments; i++) {
				float angle = rotation + MathHelper.TwoPi * i / segments;
				Vector2 current = center + angle.ToRotationVector2() * radius;
				Vector2 delta = current - previous;
				float len = delta.Length();
				if (len > 0.5f)
					DrawBeam(pixel, previous, len, delta.ToRotation(), width, drawColor);
				previous = current;
			}
		}

		internal static void DrawRadialRays(Vector2 worldCenter, float innerRadius, float outerRadius,
			int rayCount, float width, Color color, float opacity, float rotation) {
			if (Main.dedServ || rayCount < 1 || opacity <= 0f)
				return;

			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Color drawColor = color * MathHelper.Clamp(opacity, 0f, 1f);
			Vector2 center = worldCenter - Main.screenPosition;
			for (int i = 0; i < rayCount; i++) {
				float angle = rotation + MathHelper.TwoPi * i / rayCount;
				Vector2 dir = angle.ToRotationVector2();
				Vector2 start = center + dir * innerRadius;
				DrawBeam(pixel, start, Math.Max(1f, outerRadius - innerRadius), angle, width, drawColor);
			}
		}

		private static void DrawBeam(Texture2D pixel, Vector2 start, float length, float rotation,
			float width, Color color) {
			Main.spriteBatch.Draw(pixel, start, PixelSource, color, rotation, new Vector2(0f, 0.5f),
				new Vector2(Math.Max(1f, length), Math.Max(1f, width)), SpriteEffects.None, 0f);
		}

		internal static void SpawnMetalSparks(Vector2 position, Vector2 tangent, int count,
			float heat = 0.35f, float speed = 5f) {
			if (Main.dedServ)
				return;

			Vector2 baseDir = tangent.SafeNormalize(Vector2.UnitX);
			for (int i = 0; i < count; i++) {
				Vector2 velocity = baseDir.RotatedByRandom(1.15f) * Main.rand.NextFloat(speed * 0.45f, speed);
				velocity += Main.rand.NextVector2Circular(1.2f, 1.2f);
				int dustType = Main.rand.NextBool(4) ? DustID.Blood : DustID.Torch;
				Dust dust = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(7f, 7f), dustType,
					velocity, 0, default, Main.rand.NextFloat(0.75f, 1.35f + heat * 0.45f));
				dust.noGravity = true;
				dust.fadeIn = 0.25f;
			}
		}

		internal static void SpawnHeatMotes(Vector2 position, int count, float heat, float radius) {
			if (Main.dedServ)
				return;

			float hot = MathHelper.Clamp(heat, 0f, 1f);
			for (int i = 0; i < count; i++) {
				Vector2 offset = Main.rand.NextVector2Circular(radius, radius);
				Vector2 velocity = offset.SafeNormalize(Vector2.UnitY).RotatedByRandom(0.55f)
					* Main.rand.NextFloat(0.6f, 2.6f + hot * 2f);
				Color color = Main.rand.NextBool(3)
					? Color.Lerp(new Color(108, 0, 22), new Color(255, 28, 42), hot)
					: Color.Lerp(new Color(225, 48, 16), new Color(255, 202, 92), hot);
				var particle = new DefaultParticle(position + offset, velocity, Main.rand.Next(20, 34),
					Main.rand.NextFloat(0.3f, 0.72f + hot * 0.35f), color, true) {
					Deformation = new Vector2(0.35f, Main.rand.NextFloat(0.9f, 1.65f))
				};
				particle.Spawn();
			}
		}

		internal static void SpawnImpactBurst(Vector2 position, Vector2 attackDirection, bool heavy,
			float heat) {
			if (Main.dedServ)
				return;

			int glowCount = heavy ? 11 : 6;
			for (int i = 0; i < glowCount; i++) {
				Vector2 velocity = attackDirection.SafeNormalize(Vector2.UnitX).RotatedByRandom(1.45f)
					* Main.rand.NextFloat(1.5f, heavy ? 7f : 4.5f);
				Color color = Main.rand.NextBool(3)
					? new Color(218, 8, 35)
					: new Color(255, 137, 36);
				var particle = new DefaultParticle(position + Main.rand.NextVector2Circular(5f, 5f), velocity,
					heavy ? 32 : 24, Main.rand.NextFloat(0.45f, heavy ? 1.15f : 0.82f), color, true) {
					Deformation = new Vector2(0.28f, heavy ? 1.7f : 1.25f)
				};
				particle.Spawn();
			}
			SpawnMetalSparks(position, attackDirection, heavy ? 18 : 9, heat, heavy ? 8.5f : 5.5f);
		}

		internal static void AddImpactShake(Player player, int time, float strength) {
			if (!Main.dedServ && player.whoAmI == Main.myPlayer)
				player.GetModPlayer<BlazeImpactShakePlayer>().Add(time, strength);
		}

		internal static void AddShake(Player player, int time, float strength) {
			if (Main.dedServ || player.whoAmI != Main.myPlayer)
				return;

			var shake = player.GetModPlayer<ShakeEffectPlayer>();
			shake.screenShakeTime = Math.Max(shake.screenShakeTime, time);
			shake.screenShakeModifier += Main.rand.NextVector2Circular(strength, strength);
			shake.screenShakeVelocity = Main.rand.NextVector2Circular(1f, 1f)
				.SafeNormalize(Vector2.UnitX) * Math.Max(2f, strength);
		}
	}
}
