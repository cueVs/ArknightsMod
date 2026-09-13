using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Supporter.Pramanix
{
	public class PramanixFrostDomainDrawLayer : PlayerDrawLayer
	{
		public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.ForbiddenSetRing);

		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
			=> drawInfo.shadow == 0f;

		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Player player = drawInfo.drawPlayer;
			if (!player.active || player.dead || player.HeldItem.ModItem is not SaintBell)
				return;

			var state = player.GetModPlayer<SaintBellPlayer>();
			int tier = state.FrostTier;
			if (tier <= 0)
				return;

			float radius = FrostDomainLogic.GetRadius(tier, state.Skill3Active);
			if (radius <= 0f)
				return;

			float budgetRatio = state.FrostHitBudget / (float)Math.Max(1, FrostDomainLogic.HitBudgetMax[tier]);
			Vector2 center = player.Center - Main.screenPosition;
			float pulse = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2.4f) * 0.06f + 0.94f;
			float alpha = budgetRatio * pulse;

			// 各等级颜色
			Color tierColor = tier switch {
				1 => new Color(200, 230, 255),
				2 => new Color(140, 200, 255),
				3 => new Color(80, 160, 255),
				_ => new Color(200, 230, 255),
			};
			// 三级寒域外环额外增亮
			float brightness = tier == 3 ? 1.35f : 1f;

			DrawGroundMist(center, radius, alpha * 0.55f, tierColor);

			// 各等级绘制对应数量的波动环（等级越高，环越多、颜色越深、亮度越强）
			for (int r = 0; r < tier; r++) {
				float ringScale = r == 0 ? 1f : (r == 1 ? 0.88f : 0.76f);
				float ringAlpha = r == 0 ? alpha * 0.95f : (r == 1 ? alpha * 0.65f : alpha * 0.40f);
				float waveFreq = r == 0 ? 5f : (r == 1 ? 7f : 9f);
				float thickness = r == 0 ? 4.5f : (r == 1 ? 3f : 2f);
				DrawWavyRing(center, radius * ringScale, ringAlpha * brightness, thickness, waveFreq, tierColor);
			}
		}

		private static void DrawGroundMist(Vector2 center, float radius, float alpha, Color tierColor) {
			if (alpha <= 0.01f)
				return;

			int spokes = 36;
			Color spokeColor = tierColor * (alpha * 0.30f);
			for (int i = 0; i < spokes; i++) {
				float angle = i / (float)spokes * MathHelper.TwoPi;
				Vector2 dir = angle.ToRotationVector2();
				float wobble = (float)Math.Sin(angle * 3f + Main.GlobalTimeWrappedHourly * 3f) * radius * 0.04f;
				Vector2 end = center + dir * (radius * 0.82f + wobble);
				Utils.DrawLine(Main.spriteBatch, center, end, Color.Transparent, spokeColor, 6f);
			}

			int fillRings = 6;
			for (int i = 0; i < fillRings; i++) {
				float t = (i + 1) / (float)(fillRings + 1);
				float ringRadius = radius * t;
				Color ringColor = tierColor * (alpha * (1f - t) * 0.10f);
				DrawSoftRing(center, ringRadius, ringColor, 10f);
			}
		}

		private static void DrawSoftRing(Vector2 center, float radius, Color color, float thickness) {
			if (radius <= 4f)
				return;

			int segments = 48;
			for (int i = 0; i < segments; i++) {
				float a0 = i / (float)segments * MathHelper.TwoPi;
				float a1 = (i + 1) / (float)segments * MathHelper.TwoPi;
				Vector2 p0 = center + a0.ToRotationVector2() * radius;
				Vector2 p1 = center + a1.ToRotationVector2() * radius;
				Utils.DrawLine(Main.spriteBatch, p0, p1, color, color, thickness);
			}
		}

		private static void DrawWavyRing(Vector2 center, float radius, float alpha, float thickness, float waveFreq, Color tierColor) {
			if (radius <= 4f || alpha <= 0.01f)
				return;

			int segments = 72;
			Color color = tierColor * alpha;
			for (int i = 0; i < segments; i++) {
				float a0 = i / (float)segments * MathHelper.TwoPi;
				float a1 = (i + 1) / (float)segments * MathHelper.TwoPi;
				float wobble0 = (float)Math.Sin(a0 * waveFreq + Main.GlobalTimeWrappedHourly * 7f) * 8f;
				float wobble1 = (float)Math.Sin(a1 * waveFreq + Main.GlobalTimeWrappedHourly * 7f) * 8f;
				Vector2 p0 = center + a0.ToRotationVector2() * (radius + wobble0);
				Vector2 p1 = center + a1.ToRotationVector2() * (radius + wobble1);
				Utils.DrawLine(Main.spriteBatch, p0, p1, color, color, thickness);
			}
		}
	}
}
