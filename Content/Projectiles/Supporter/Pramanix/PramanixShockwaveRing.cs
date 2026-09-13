using System;
using System.Collections.Generic;
using ArknightsMod.Content.Buffs.Supporter.Pramanix;
using ArknightsMod.Content.Items.Weapons.Supporter.Pramanix;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Supporter.Pramanix
{
	public class PramanixShockwaveRing : ModProjectile
	{
		private const int Lifetime = 40;
		private const int RingSegments = 52;
		private const float RingBandWidth = 28f;
		private const float HitSweepBand = 42f;
		private const float WobbleAmp1 = 4.5f;
		private const float WobbleAmp2 = 2.2f;

		private static BasicEffect sharedRingEffect;
		private readonly HashSet<(int npcId, int ringIndex)> hitSegments = new();
		private float wobbleSeed;

		public override string Texture => "ArknightsMod/Assets/null";

		public override void SetStaticDefaults() {
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1600;
		}

		public override void SetDefaults() {
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = Lifetime;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = Lifetime;
		}

		public override void OnSpawn(IEntitySource source) {
			wobbleSeed = Projectile.identity * 0.173f;
		}

		private float MaxRadius => Projectile.ai[0] > 0 ? Projectile.ai[0] : SaintBellPlayer.Skill2ShockwaveMaxRadius;
		private float Progress => 1f - Projectile.timeLeft / (float)Lifetime;
		private float EaseExpand => 1f - (1f - Progress) * (1f - Progress);
		private float Fade => 1f - Progress;
		private float ExpandRadius => MathHelper.Lerp(MaxRadius * 0.5f, MaxRadius, EaseExpand);

		public override void AI() {
			Projectile.Center = Main.player[Projectile.owner].Center;

			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			if (Projectile.timeLeft % 2 == 0)
				TickSweepHit();
		}

		private void TickSweepHit() {
			Player owner = Main.player[Projectile.owner];
			Vector2 center = Projectile.Center;
			float[] damageRings = SaintBellPlayer.Skill2DamageRingScales;

			for (int ringIndex = 0; ringIndex < damageRings.Length; ringIndex++) {
				float radius = ExpandRadius * damageRings[ringIndex];
				float minR = radius - HitSweepBand;
				float maxR = radius + HitSweepBand;
				float minRSq = minR * minR;
				float maxRSq = maxR * maxR;

				foreach (NPC npc in Main.ActiveNPCs) {
					if (!npc.active || !npc.CanBeChasedBy(Projectile))
						continue;

					int id = npc.whoAmI;
					var key = (id, ringIndex);
					if (hitSegments.Contains(key))
						continue;

					Vector2 offset = npc.Center - center;
					float distSq = offset.LengthSquared();
					if (distSq < minRSq || distSq > maxRSq)
						continue;

					float dist = MathF.Sqrt(distSq);
					if (Math.Abs(dist - radius) > HitSweepBand)
						continue;

					hitSegments.Add(key);
					FrostDomainLogic.StrikeMagic(owner, npc, Projectile.damage);
					npc.AddBuff(ModContent.BuffType<PramanixSlowDebuff>(), 20);
					PramanixHitVfx.SpawnSlowSmoke(npc);
					PramanixColdAttachment.ApplySkill2Hit(npc, owner);
				}
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.netMode == NetmodeID.Server || Fade <= 0.01f)
				return false;

			DrawAllRings(Projectile.Center, Fade);
			return false;
		}

		private void DrawAllRings(Vector2 worldCenter, float intensity) {
			if (Main.dedServ || intensity <= 0.01f)
				return;

			int ringCount = SaintBellPlayer.Skill2RingScales.Length;
			int vertsPerRing = RingSegments * 12;
			var verts = new VertexPositionColor[ringCount * vertsPerRing];
			int vi = 0;
			float time = Main.GlobalTimeWrappedHourly;

			for (int ring = 0; ring < ringCount; ring++) {
				float ringRadius = ExpandRadius * SaintBellPlayer.Skill2RingScales[ring];
				float ringAlpha = intensity * (0.38f + ring * 0.16f);
				vi = AppendWobbledRing(verts, vi, worldCenter, ringRadius, ringAlpha, time, ring);
			}

			GraphicsDevice gd = Main.graphics?.GraphicsDevice;
			if (gd == null)
				return;

			EnsureRingEffect(gd);
			if (sharedRingEffect == null)
				return;

			BlendState oldBlend = gd.BlendState;
			RasterizerState oldRaster = gd.RasterizerState;
			DepthStencilState oldDepth = gd.DepthStencilState;
			SpriteBatch sb = Main.spriteBatch;
			try {
				try { sb.End(); } catch { }
				gd.BlendState = BlendState.NonPremultiplied;
				gd.RasterizerState = RasterizerState.CullNone;
				gd.DepthStencilState = DepthStencilState.None;

				Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, 0f, 1f);
				Matrix model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0f)) * Main.GameViewMatrix.ZoomMatrix;
				sharedRingEffect.World = model;
				sharedRingEffect.View = Matrix.Identity;
				sharedRingEffect.Projection = projection;
				foreach (EffectPass pass in sharedRingEffect.CurrentTechnique.Passes) {
					pass.Apply();
					gd.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, verts.Length / 3);
				}
			}
			finally {
				gd.BlendState = oldBlend;
				gd.RasterizerState = oldRaster;
				gd.DepthStencilState = oldDepth;
				try {
					sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
				}
				catch { }
			}
		}

		private int AppendWobbledRing(VertexPositionColor[] verts, int vi, Vector2 worldCenter, float rOuter, float intensity, float time, int ringIndex) {
			float f = MathHelper.Clamp(intensity, 0f, 1f);
			var colInner = new Color(0, 0, 0, 0);
			var colMid = new Color(
				(byte)MathHelper.Clamp(100f * f, 0f, 255f),
				(byte)MathHelper.Clamp(210f * f, 0f, 255f),
				(byte)MathHelper.Clamp(255f * f, 0f, 255f),
				(byte)MathHelper.Clamp(95f * f, 0f, 255f));
			var colOuter = new Color(
				(byte)MathHelper.Clamp(230f * f, 0f, 255f),
				(byte)MathHelper.Clamp(250f * f, 0f, 255f),
				(byte)MathHelper.Clamp(255f * f, 0f, 255f),
				(byte)MathHelper.Clamp(155f * f, 0f, 255f));

			float phase = time * 3.2f + ringIndex * 0.55f + wobbleSeed;

			for (int i = 0; i < RingSegments; i++) {
				int j = (i + 1) % RingSegments;
				float angI = MathHelper.TwoPi * i / RingSegments;
				float angJ = MathHelper.TwoPi * j / RingSegments;

				float rOutI = rOuter + WobbleAt(angI, phase);
				float rOutJ = rOuter + WobbleAt(angJ, phase);
				float rInI = Math.Max(rOutI - RingBandWidth, 6f);
				float rInJ = Math.Max(rOutJ - RingBandWidth, 6f);
				float rMidI = MathHelper.Lerp(rInI, rOutI, 0.55f);
				float rMidJ = MathHelper.Lerp(rInJ, rOutJ, 0.55f);

				Vector2 dI = angI.ToRotationVector2();
				Vector2 dJ = angJ.ToRotationVector2();
				Vector2 pIi = worldCenter + dI * rInI;
				Vector2 pIm = worldCenter + dI * rMidI;
				Vector2 pIo = worldCenter + dI * rOutI;
				Vector2 pJi = worldCenter + dJ * rInJ;
				Vector2 pJm = worldCenter + dJ * rMidJ;
				Vector2 pJo = worldCenter + dJ * rOutJ;

				verts[vi++] = Vpc(pIm, colMid);
				verts[vi++] = Vpc(pIi, colInner);
				verts[vi++] = Vpc(pJm, colMid);
				verts[vi++] = Vpc(pIi, colInner);
				verts[vi++] = Vpc(pJm, colMid);
				verts[vi++] = Vpc(pJi, colInner);
				verts[vi++] = Vpc(pIo, colOuter);
				verts[vi++] = Vpc(pIm, colMid);
				verts[vi++] = Vpc(pJo, colOuter);
				verts[vi++] = Vpc(pIm, colMid);
				verts[vi++] = Vpc(pJo, colOuter);
				verts[vi++] = Vpc(pJm, colMid);
			}

			return vi;
		}

		private float WobbleAt(float angle, float phase) =>
			MathF.Sin(angle * 5f + phase) * WobbleAmp1 + MathF.Sin(angle * 9f - phase * 1.4f) * WobbleAmp2;

		private static VertexPositionColor Vpc(Vector2 p, Color col) =>
			new(new Vector3(p.X, p.Y, 0f), col);

		private static void EnsureRingEffect(GraphicsDevice gd) {
			if (sharedRingEffect == null || sharedRingEffect.IsDisposed) {
				sharedRingEffect?.Dispose();
				sharedRingEffect = new BasicEffect(gd) {
					VertexColorEnabled = true,
					TextureEnabled = false,
				};
			}
		}
	}
}
