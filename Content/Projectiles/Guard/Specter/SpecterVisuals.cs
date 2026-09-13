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

namespace ArknightsMod.Content.Projectiles.Guard.Specter
{
	/// <summary>
	/// 幽灵鲨专用视觉：以冷白骨光和深海蓝作底，二技能才让暗红压过蓝色。
	/// 粒子主体使用项目自定义 DefaultParticle，原版 Dust 只做少量机械火星。
	/// </summary>
	internal static class SpecterVisuals
	{
		internal static SoundStyle MotorSound =>
			ContentSamples.ItemsByType[ItemID.SawtoothShark].UseSound ?? SoundID.Item22;

		internal static void DrawSawCurrent(Vector2 handWorld, Vector2 tipWorld, float spool,
			float chainPhase, int skillMode) {
			if (Main.dedServ)
				return;

			Texture2D rail = ModContent.Request<Texture2D>(
				"ArknightsMod/Content/Textures/spark_07").Value;
			Texture2D trace = ModContent.Request<Texture2D>(
				"ArknightsMod/Content/Textures/trace_03").Value;
			Texture2D flare = ModContent.Request<Texture2D>(
				"ArknightsMod/Content/Textures/flare_01").Value;
			Texture2D glow = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;

			Vector2 delta = tipWorld - handWorld;
			float length = delta.Length();
			if (length < 2f)
				return;

			Vector2 direction = delta / length;
			Vector2 normal = new(-direction.Y, direction.X);
			float rotation = direction.ToRotation();
			float hot = MathHelper.Clamp(spool, 0f, 1f);
			float pulse = 0.86f + MathF.Sin(chainPhase * MathHelper.TwoPi) * 0.14f;
			bool frenzy = skillMode == 2;
			bool empowered = skillMode == 1;
			Color abyss = frenzy ? new Color(42, 3, 20, 130) : new Color(8, 34, 58, 126);
			Color edge = frenzy ? new Color(226, 12, 50, 145)
				: empowered ? new Color(108, 226, 242, 142) : new Color(58, 146, 184, 118);
			Color bone = frenzy ? new Color(255, 226, 218, 132) : new Color(224, 244, 238, 124);

			Vector2 center = Vector2.Lerp(handWorld, tipWorld, 0.63f) - Main.screenPosition;
			float scaleX = length / Math.Max(1f, rail.Width) * 0.84f;
			float scaleY = 0.072f * (0.8f + hot * 0.3f);
			Main.spriteBatch.Draw(rail, center, null, abyss * pulse, rotation,
				rail.Size() * 0.5f, new Vector2(scaleX, scaleY * 1.8f), SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(rail, center + normal * 0.8f, null, edge * (hot * pulse), rotation,
				rail.Size() * 0.5f, new Vector2(scaleX * 0.96f, scaleY * 0.72f), SpriteEffects.None, 0f);

			int teeth = frenzy ? 8 : 6;
			float scroll = chainPhase - MathF.Floor(chainPhase);
			for (int i = 0; i < teeth; i++) {
				float u = (i + scroll) / teeth;
				if (u > 1f)
					u -= 1f;
				Vector2 tooth = handWorld + direction * (length * MathHelper.Lerp(0.26f, 0.99f, u));
				tooth += normal * (((i & 1) == 0 ? 1f : -1f) * (2.5f + hot * 1.6f));
				BaseHeldMeleeSupport.DrawSharpTear(tooth, rotation, Color.Lerp(edge, bone, 0.48f),
					frenzy ? 24f : 19f, frenzy ? 6.2f : 4.8f, (0.34f + hot * 0.42f) * pulse);
			}

			for (int i = 0; i < 3; i++) {
				float u = (i + 0.3f + chainPhase * 0.19f) / 3f;
				u -= MathF.Floor(u);
				Vector2 position = handWorld + direction * (length * MathHelper.Lerp(0.4f, 0.98f, u));
				Main.spriteBatch.Draw(trace, position + normal * ((i & 1) == 0 ? 3f : -3f) - Main.screenPosition,
					null, edge * (0.68f * pulse), rotation + MathHelper.PiOver2,
					trace.Size() * 0.5f, new Vector2(0.018f, frenzy ? 0.075f : 0.055f),
					SpriteEffects.None, 0f);
			}

			float glowScale = frenzy ? 0.24f : empowered ? 0.20f : 0.15f;
			Main.spriteBatch.Draw(glow, tipWorld - Main.screenPosition, null, edge * (0.5f * pulse),
				0f, glow.Size() * 0.5f, glowScale, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(glow, tipWorld - Main.screenPosition, null, bone * 0.48f,
				0f, glow.Size() * 0.5f, glowScale * 0.52f, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(flare, tipWorld - Main.screenPosition, null, edge * (0.72f * pulse),
				rotation, flare.Size() * 0.5f, new Vector2(frenzy ? 0.18f : 0.13f, frenzy ? 0.08f : 0.06f),
				SpriteEffects.None, 0f);
		}

		internal static void DrawFrenzyAura(Vector2 worldCenter, float rotation, float opacity) {
			if (Main.dedServ || opacity <= 0f)
				return;

			Texture2D ring = ModContent.Request<Texture2D>(
				"ArknightsMod/Content/Textures/circle_03").Value;
			Texture2D twirl = ModContent.Request<Texture2D>(
				"ArknightsMod/Content/Textures/twirl_03").Value;
			Vector2 center = worldCenter - Main.screenPosition;
			float pulse = 0.92f + MathF.Sin((float)Main.GlobalTimeWrappedHourly * 6.5f) * 0.08f;

			Main.spriteBatch.Draw(twirl, center, null, new Color(12, 52, 78, 88) * opacity,
				-rotation * 0.72f, twirl.Size() * 0.5f, 0.34f * pulse, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(ring, center, null, new Color(184, 8, 42, 112) * opacity,
				rotation, ring.Size() * 0.5f, new Vector2(0.36f, 0.54f) * pulse, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(ring, center, null, new Color(222, 240, 232, 58) * opacity,
				-rotation * 1.3f, ring.Size() * 0.5f, new Vector2(0.25f, 0.40f), SpriteEffects.None, 0f);

			for (int i = 0; i < 8; i++) {
				float angle = rotation * 0.55f + MathHelper.TwoPi * i / 8f;
				Vector2 position = worldCenter + angle.ToRotationVector2() * (38f + (i & 1) * 9f);
				Color color = i % 3 == 0 ? new Color(218, 238, 232) : new Color(206, 10, 46);
				BaseHeldMeleeSupport.DrawSharpTear(position, angle + MathHelper.PiOver2, color,
					28f + (i & 1) * 10f, 5f, opacity * (0.4f + (i % 3) * 0.12f));
			}
		}

		internal static void SpawnSawMotes(Vector2 position, Vector2 tangent, int skillMode,
			float intensity, int count) {
			if (Main.dedServ)
				return;

			Vector2 baseDirection = tangent.SafeNormalize(Vector2.UnitX);
			for (int i = 0; i < count; i++) {
				Vector2 velocity = baseDirection.RotatedByRandom(1.05f)
					* Main.rand.NextFloat(1.4f, 4.8f + intensity * 2f);
				Color color = skillMode == 2
					? (Main.rand.NextBool(3) ? new Color(234, 230, 218) : new Color(205, 10, 46))
					: (Main.rand.NextBool(3) ? new Color(218, 240, 234) : new Color(54, 156, 190));
				var mote = new DefaultParticle(position + Main.rand.NextVector2Circular(5f, 5f), velocity,
					Main.rand.Next(18, 29), Main.rand.NextFloat(0.35f, 0.78f), color, true) {
					Deformation = new Vector2(0.24f, Main.rand.NextFloat(1.1f, 1.8f))
				};
				mote.Spawn();
			}

			// Dust 只占辅助位：每四次生成才补一颗机械火星。
			if (Main.rand.NextBool(4)) {
				int dustType = skillMode == 2 ? DustID.Blood : DustID.BlueTorch;
				Dust dust = Dust.NewDustPerfect(position, dustType,
					baseDirection.RotatedByRandom(0.8f) * Main.rand.NextFloat(2f, 5f), 60,
					default, Main.rand.NextFloat(0.7f, 1.05f));
				dust.noGravity = true;
			}
		}

		internal static void SpawnImpactBurst(Vector2 position, Vector2 attackDirection, int skillMode) {
			if (Main.dedServ)
				return;

			int count = skillMode == 2 ? 14 : skillMode == 1 ? 10 : 7;
			for (int i = 0; i < count; i++) {
				Vector2 velocity = attackDirection.SafeNormalize(Vector2.UnitX).RotatedByRandom(1.4f)
					* Main.rand.NextFloat(1.4f, skillMode == 2 ? 7.5f : 5.2f);
				Color color = skillMode == 2
					? (i % 3 == 0 ? new Color(238, 232, 216) : new Color(206, 9, 45))
					: (i % 3 == 0 ? new Color(226, 244, 236) : new Color(50, 155, 194));
				var mote = new DefaultParticle(position + Main.rand.NextVector2Circular(6f, 6f), velocity,
					Main.rand.Next(22, 34), Main.rand.NextFloat(0.46f, skillMode == 2 ? 1.08f : 0.82f),
					color, true) {
					Deformation = new Vector2(0.23f, skillMode == 2 ? 1.9f : 1.45f)
				};
				mote.Spawn();
			}
			for (int i = 0; i < (skillMode == 2 ? 4 : 2); i++) {
				Dust dust = Dust.NewDustPerfect(position, skillMode == 2 ? DustID.Blood : DustID.BlueTorch,
					attackDirection.RotatedByRandom(1.2f) * Main.rand.NextFloat(2f, 5.5f), 70,
					default, Main.rand.NextFloat(0.75f, 1.2f));
				dust.noGravity = true;
			}
		}

		internal static void SpawnSkillActivation(Vector2 center, bool frenzy) {
			if (Main.dedServ)
				return;
			int count = frenzy ? 34 : 22;
			for (int i = 0; i < count; i++) {
				float angle = MathHelper.TwoPi * i / count;
				Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(2.2f, frenzy ? 7.6f : 5.2f);
				Color color = frenzy
					? (i % 4 == 0 ? new Color(228, 238, 226) : new Color(194, 7, 42))
					: (i % 4 == 0 ? new Color(226, 242, 232) : new Color(48, 151, 190));
				var mote = new DefaultParticle(center, velocity, Main.rand.Next(26, 42),
					Main.rand.NextFloat(0.55f, frenzy ? 1.25f : 0.92f), color, true) {
					Deformation = new Vector2(0.25f, frenzy ? 1.9f : 1.55f)
				};
				mote.Spawn();
			}
		}

		internal static void SpawnFrenzyMotes(Vector2 center, int count) {
			if (Main.dedServ)
				return;
			for (int i = 0; i < count; i++) {
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				Vector2 position = center + angle.ToRotationVector2() * Main.rand.NextFloat(32f, 58f);
				Vector2 velocity = (center - position).SafeNormalize(Vector2.Zero)
					* Main.rand.NextFloat(0.8f, 2.4f) + new Vector2(0f, -0.35f);
				Color color = Main.rand.NextBool(4) ? new Color(218, 235, 226) : new Color(184, 7, 40);
				new DefaultParticle(position, velocity, Main.rand.Next(24, 38),
					Main.rand.NextFloat(0.35f, 0.78f), color, true) {
					Deformation = new Vector2(0.32f, 1.5f)
				}.Spawn();
			}
		}

		internal static void SpawnRefusalBurst(Vector2 center) {
			SpawnSkillActivation(center, frenzy: true);
		}

		internal static void SpawnFrenzyCollapse(Vector2 center) {
			if (Main.dedServ)
				return;
			for (int i = 0; i < 26; i++) {
				float angle = MathHelper.TwoPi * i / 26f;
				Vector2 position = center + angle.ToRotationVector2() * Main.rand.NextFloat(42f, 68f);
				Vector2 velocity = (center - position).SafeNormalize(Vector2.UnitY)
					* Main.rand.NextFloat(2.4f, 5.8f);
				Color color = i % 4 == 0 ? new Color(218, 234, 226) : new Color(76, 8, 34);
				new DefaultParticle(position, velocity, Main.rand.Next(26, 42),
					Main.rand.NextFloat(0.45f, 0.95f), color, true) {
					Deformation = new Vector2(0.3f, 1.65f)
				}.Spawn();
			}
		}

		internal static void SpawnExhaustionMote(Vector2 position) {
			if (Main.dedServ)
				return;
			new DefaultParticle(position, new Vector2(Main.rand.NextFloat(-0.35f, 0.35f),
				Main.rand.NextFloat(-1.2f, -0.45f)), Main.rand.Next(24, 38),
				Main.rand.NextFloat(0.34f, 0.62f), new Color(54, 65, 76), true) {
				Deformation = new Vector2(0.55f, 1.15f)
			}.Spawn();
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
