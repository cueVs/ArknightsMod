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
	public class PramanixSkill1Snowflake : ModProjectile
	{
		private const int MaxTrailPositions = 36;
		private const float TrailHalfWidthMax = 20f;
		private const int Lifetime = 54;
		// 命中判定比视觉效果更宽，确保风墙范围感
		private const float HeadHitRadius = 28f;
		private const int TrailHitCheckCount = 15;

		private static BasicEffect sharedEffect;
		private readonly List<Vector2> trail = new();
		private readonly HashSet<int> hitNpcIds = new();

		// ai[0]: 发射方向 (+1 或 -1)
		public override string Texture => "ArknightsMod/Assets/null";

		public override void SetStaticDefaults() {
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1600;
		}

		public override void SetDefaults() {
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = Lifetime;
			Projectile.ignoreWater = true;
			Projectile.light = 0.22f;
		}

		public override void OnSpawn(IEntitySource source) {
			trail.Clear();
			hitNpcIds.Clear();
			Projectile.ai[0] = Projectile.velocity.X >= 0f ? 1f : -1f;
		}

		public override void AI() {
			trail.Insert(0, Projectile.Center);
			if (trail.Count > MaxTrailPositions)
				trail.RemoveAt(trail.Count - 1);

			Projectile.rotation += 0.18f;

			if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(2))
				SpawnTrailDust();

			if (Main.netMode != NetmodeID.MultiplayerClient)
				CheckHits();
		}

		// ─── 伤害判定 ─────────────────────────────────────────────────────────────

		private void CheckHits() {
			Player owner = Main.player[Projectile.owner];
			if (!owner.active || owner.dead)
				return;

			int damage = Math.Max(1, (int)(FrostDomainLogic.GetMagicDamage(owner) * SaintBellPlayer.Skill1DamageMult));

			foreach (NPC npc in Main.ActiveNPCs) {
				if (!npc.active || !npc.CanBeChasedBy(Projectile))
					continue;
				if (hitNpcIds.Contains(npc.whoAmI))
					continue;
				if (!IsNpcInPath(npc))
					continue;

				hitNpcIds.Add(npc.whoAmI);
				HitNpc(owner, npc, damage);
			}
		}

		// 判定范围故意大于视觉，让风墙有充足的"风压"感
		private bool IsNpcInPath(NPC npc) {
			if (Vector2.DistanceSquared(npc.Center, Projectile.Center) <= HeadHitRadius * HeadHitRadius)
				return true;

			int check = Math.Min(trail.Count, TrailHitCheckCount);
			for (int j = 1; j < check; j++) {
				float t = (float)j / (MaxTrailPositions - 1);
				float trailW = TrailHalfWidthMax * t + 22f; // 判定宽于视觉宽度
				if (Vector2.DistanceSquared(npc.Center, trail[j]) <= trailW * trailW)
					return true;
			}
			return false;
		}

		private void HitNpc(Player owner, NPC npc, int damage) {
			FrostDomainLogic.StrikeMagic(owner, npc, damage);
			npc.AddBuff(ModContent.BuffType<PramanixSlowDebuff>(), SaintBellPlayer.Skill1ColdTicks);

			int dir = (int)Projectile.ai[0];
			float resist = MathHelper.Clamp(npc.knockBackResist, 0f, 1f);
			float kb = Math.Max(SaintBellPlayer.Skill1BossKnockbackMin, SaintBellPlayer.Skill1KnockbackForce * resist);
			// 小怪（knockBackResist≈1）大幅击飞，BOSS（knockBackResist≈0）仅轻微位移
			npc.velocity.X = dir * kb;
			npc.velocity.Y = -SaintBellPlayer.Skill1KnockbackVertical * MathHelper.Clamp(resist, 0.05f, 1f);
			npc.netUpdate = true;

			PramanixColdAttachment.GrantKnockbackGrace(npc);
			PramanixColdAttachment.ApplySkill1Hit(npc, 3);

			PramanixHitVfx.SpawnSnowflakeBurst(npc);
			PramanixHitVfx.SpawnSlowSmoke(npc);
		}

		// ─── 粒子 ─────────────────────────────────────────────────────────────────

		private void SpawnTrailDust() {
			Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(3f, 3f);
			Dust d = Dust.NewDustPerfect(pos, DustID.Snow,
				new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.7f, 0.2f)),
				110, new Color(185, 225, 255), 0.55f + Main.rand.NextFloat(0.3f));
			d.noGravity = true;
			d.fadeIn = 0.25f;
		}

		// ─── 渲染 ─────────────────────────────────────────────────────────────────

		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ || trail.Count < 2)
				return false;

			DrawTrailAndBody();
			return false;
		}

		private void DrawTrailAndBody() {
			int count = trail.Count;
			// 每段：4 trail quad (24 verts) + 2 fog quad (12 verts) = 36；雪花：18
			var verts = new VertexPositionColor[(count - 1) * 36 + 18];
			int vi = 0;

			float lifeFade = MathHelper.Clamp(Projectile.timeLeft / (float)Lifetime, 0f, 1f);

			for (int i = 0; i < count - 1; i++) {
				float t0 = (float)i / (count - 1);
				float t1 = (float)(i + 1) / (count - 1);

				// 头部细（t=0→w=0），尾部宽（t=1→w=max）
				float w0 = TrailHalfWidthMax * t0;
				float w1 = TrailHalfWidthMax * t1;

				// 尾部最后25%淡出至0，消除断面感
				float tailFade0 = t0 > 0.75f ? 1f - (t0 - 0.75f) / 0.25f : 1f;
				float tailFade1 = t1 > 0.75f ? 1f - (t1 - 0.75f) / 0.25f : 1f;
				float a0 = (1f - t0 * 0.2f) * lifeFade * tailFade0;
				float a1 = (1f - t1 * 0.2f) * lifeFade * tailFade1;

				Vector2 p0 = trail[i];
				Vector2 p1 = trail[i + 1];
				Vector2 seg = p1 - p0;
				if (seg.LengthSquared() < 0.0001f)
					seg = Vector2.UnitX;
				seg.Normalize();
				Vector2 perp = new(-seg.Y, seg.X);

				// 雾气：随机不透明度，宽于trail
				float fogNoise = (float)(Math.Sin(i * 1.73f + Projectile.whoAmI * 0.61f + Main.GlobalTimeWrappedHourly * 2.1f) * 0.3f + 0.6f);
				AppendTrailSegment(verts, ref vi, p0, p1, perp, w0, w1, a0, a1, fogNoise);
			}

			AppendSnowflakeArms(verts, ref vi, trail[0], Projectile.rotation, lifeFade);

			if (vi == 0)
				return;

			GraphicsDevice gd = Main.graphics?.GraphicsDevice;
			if (gd == null)
				return;

			EnsureEffect(gd);
			if (sharedEffect == null)
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

				Matrix proj = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, 0f, 1f);
				Matrix world = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0f))
				               * Main.GameViewMatrix.ZoomMatrix;
				sharedEffect.World = world;
				sharedEffect.View = Matrix.Identity;
				sharedEffect.Projection = proj;

				foreach (EffectPass pass in sharedEffect.CurrentTechnique.Passes) {
					pass.Apply();
					gd.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, vi / 3);
				}
			}
			finally {
				gd.BlendState = oldBlend;
				gd.RasterizerState = oldRaster;
				gd.DepthStencilState = oldDepth;
				try {
					sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
						DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
				}
				catch { }
			}
		}

		// 截面：outerL(透明) → innerL(蓝) → center(白) → innerR(蓝) → outerR(透明)
		// 两侧额外附加雾气 quad（更宽、低不透明度、随 fogNoise 变化）
		private static void AppendTrailSegment(VertexPositionColor[] verts, ref int vi,
			Vector2 p0, Vector2 p1, Vector2 perp,
			float w0, float w1, float a0, float a1, float fogNoise) {

			float iw0 = w0 * 0.5f, iw1 = w1 * 0.5f;
			float fw0 = w0 * 1.4f, fw1 = w1 * 1.4f; // 雾气扩展宽度

			Color oL0 = new Color(70, 130, 255, 0),   oL1 = new Color(70, 130, 255, 0);
			Color bL0 = new Color(100, 175, 255, (byte)MathHelper.Clamp(135 * a0, 0, 255));
			Color bL1 = new Color(100, 175, 255, (byte)MathHelper.Clamp(135 * a1, 0, 255));
			Color wh0 = new Color(255, 255, 255, (byte)MathHelper.Clamp(215 * a0, 0, 255));
			Color wh1 = new Color(255, 255, 255, (byte)MathHelper.Clamp(215 * a1, 0, 255));

			// 雾气颜色：边缘完全透明，内侧有随机不透明度（浅蓝白雾）
			byte fogA0 = (byte)MathHelper.Clamp(90 * a0 * fogNoise, 0, 255);
			byte fogA1 = (byte)MathHelper.Clamp(90 * a1 * fogNoise, 0, 255);
			Color fogInner0 = new Color(180, 215, 255, fogA0);
			Color fogInner1 = new Color(180, 215, 255, fogA1);
			Color fogEdge   = new Color(180, 215, 255, 0);

			Vector2 oLA = p0 - perp * w0,  oLB = p1 - perp * w1;
			Vector2 bLA = p0 - perp * iw0, bLB = p1 - perp * iw1;
			Vector2 cA  = p0,              cB  = p1;
			Vector2 bRA = p0 + perp * iw0, bRB = p1 + perp * iw1;
			Vector2 oRA = p0 + perp * w0,  oRB = p1 + perp * w1;
			Vector2 fLA = p0 - perp * fw0, fLB = p1 - perp * fw1;
			Vector2 fRA = p0 + perp * fw0, fRB = p1 + perp * fw1;

			// 主体 trail（4 quads）
			AppendQuad(verts, ref vi, oLA, bLA, oLB, bLB, oL0, bL0, oL1, bL1);
			AppendQuad(verts, ref vi, bLA, cA,  bLB, cB,  bL0, wh0, bL1, wh1);
			AppendQuad(verts, ref vi, cA,  bRA, cB,  bRB, wh0, bL0, wh1, bL1);
			AppendQuad(verts, ref vi, bRA, oRA, bRB, oRB, bL0, oL0, bL1, oL1);

			// 两侧雾气（2 quads）：从 trail 外缘向外扩展，边缘完全透明
			AppendQuad(verts, ref vi, fLA, oLA, fLB, oLB, fogEdge, fogInner0, fogEdge, fogInner1);
			AppendQuad(verts, ref vi, oRA, fRA, oRB, fRB, fogInner0, fogEdge, fogInner1, fogEdge);
		}

		// 6臂星形雪花身体
		private static void AppendSnowflakeArms(VertexPositionColor[] verts, ref int vi,
			Vector2 center, float rotation, float alpha) {

			byte a  = (byte)MathHelper.Clamp(245 * alpha, 0, 255);
			byte ab = (byte)MathHelper.Clamp(110 * alpha, 0, 255);
			Color tip  = new Color(255, 255, 255, a);
			Color root = new Color(160, 215, 255, ab);
			const float armLen = 9f, armWidth = 1.8f;

			for (int arm = 0; arm < 6; arm++) {
				float angle = rotation + arm * MathF.PI / 3f;
				Vector2 dir  = angle.ToRotationVector2();
				Vector2 perpArm = new Vector2(-dir.Y, dir.X) * armWidth;

				verts[vi++] = Vpc(center + dir * armLen, tip);
				verts[vi++] = Vpc(center - perpArm,      root);
				verts[vi++] = Vpc(center + perpArm,      root);
			}
		}

		private static void AppendQuad(VertexPositionColor[] verts, ref int vi,
			Vector2 a, Vector2 b, Vector2 c, Vector2 d,
			Color ca, Color cb, Color cc, Color cd) {
			verts[vi++] = Vpc(a, ca);
			verts[vi++] = Vpc(b, cb);
			verts[vi++] = Vpc(c, cc);
			verts[vi++] = Vpc(b, cb);
			verts[vi++] = Vpc(d, cd);
			verts[vi++] = Vpc(c, cc);
		}

		private static VertexPositionColor Vpc(Vector2 pos, Color col) =>
			new(new Vector3(pos.X, pos.Y, 0f), col);

		private static void EnsureEffect(GraphicsDevice gd) {
			if (sharedEffect == null || sharedEffect.IsDisposed) {
				sharedEffect?.Dispose();
				sharedEffect = new BasicEffect(gd) {
					VertexColorEnabled = true,
					TextureEnabled     = false,
				};
			}
		}
	}
}
