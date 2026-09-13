using ArknightsMod.Content.NPCs.Enemy.Chapter6.FrostNova;
using ArknightsMod.Players;
using ArknightsMod.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Entelechia
{
	// 隐德来希·二技能「绯红壁合」：迁移自 EntelechiaScytheS2 的循环斩特效。
	// 原模组中这是武器的常驻攻击方式（持续按住左键即可循环挥砍），迁移后改为仅能在二技能
	// 「绯红壁合」激活的 12 秒窗口内按住左键挥出，技能时间到后即使仍按住左键也会自动淡出收尾。
	public class EntelechiaScytheCircleSlashProjectile : ModProjectile, IIceAltarWarpDraw
	{
		private const int FadeInTime = 12;
		private const int FadeOutTime = 20;
		private const float Radius = 210f;
		private const float Width = 46f;
		private const float RenderWidthMul = 0.62f;
		private const float OuterRingRadiusOffset = 16f;
		private const float DamageBandThickness = 42f;
		private const float EllipseYScale = 0.92f;
		private const int Segments = 3;
		private const int InnerSegments = 4;
		private const int PointsPerSegment = 22;
		private const float SegmentOverlapAngle = 0.08f;
		private const float RotationSpeed = 0.072f;       // 外圈刀光旋转加快
		private const float InnerRotationSpeed = 0.050f;  // 内圈刀光旋转加快
		private const float InnerRadiusMul = 0.86f;
		private const float InnerWidthMul = 0.82f;
		private const float WarpIntensity = 0.18f;   // 旋转刀光区域扭曲增强
		private const int WarpAnnulusPoints = 96;

		private static string ShapeTexturePath => "ArknightsMod/Content/Projectiles/Guard/Entelechia/EntelechiaScytheSlash";
		private static string InnerShapeTexturePath => "ArknightsMod/Content/Projectiles/Guard/Entelechia/EntelechiaScytheSlash_Extra";
		private static SoundStyle SwingSound => new SoundStyle("ArknightsMod/Sounds/EntelechiaScytheSwing") { Volume = 0.6f, MaxInstances = 2 };

		public override string Texture => ArknightsMod.noTexture;

		private Player Owner => Main.player[Projectile.owner];

		private float RingRotation {
			get => Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}

		private float FadeOutCounter {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		private float VisualScale {
			get => Projectile.localAI[0];
			set => Projectile.localAI[0] = value;
		}

		private float InnerRingRotation {
			get => Projectile.localAI[1];
			set => Projectile.localAI[1] = value;
		}

		private int Timer;

		/// <inheritdoc />
		public bool IceAltarWarpMaskActive => Projectile.Opacity > 0.05f;

		public override void SetDefaults() {
			Projectile.width = 64;
			Projectile.height = 64;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.ownerHitCheck = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 30;   // 每 0.5 秒造成一次伤害
			Projectile.DamageType = DamageClass.Melee;
			FadeOutCounter = -1f;
			Projectile.Opacity = 0f;
			VisualScale = 0f;
			InnerRingRotation = 0f;
		}

		public override void AI() {
			Player p = Owner;
			if (!p.active || p.dead) {
				Projectile.Kill();
				return;
			}
			Projectile.timeLeft = 2;
			Attack(p);
			Timer++;
		}

		private void Attack(Player p) {
			var mp = p.GetModPlayer<WeaponPlayer>();
			if (Timer == 0 && Projectile.owner == Main.myPlayer)
				SoundEngine.PlaySound(SwingSound, Projectile.Center);
			Projectile.gfxOffY = 0f;

			// 仅在二技能「绯红壁合」激活期间，按住左键才会维持循环斩；技能时限到了则自动收尾
			bool holding = p.controlUseItem && mp.Skill == 1 && mp.SkillActive;
			bool fadingOut = FadeOutCounter >= 0f;
			if (Timer == 0 && holding) {
				Projectile.Opacity = 0f;
				VisualScale = 0f;
			}

			if (!holding) {
				if (!fadingOut)
					FadeOutCounter = 0f;
				else
					FadeOutCounter++;

				float fadeOutT = Utils.GetLerpValue(0f, FadeOutTime, FadeOutCounter, clamped: true);
				Projectile.Opacity = 1f - fadeOutT;
				if (Projectile.Opacity <= 0.001f) {
					Projectile.Kill();
					return;
				}
			}
			else {
				FadeOutCounter = -1f;
			}

			float baseAngle = (Main.MouseWorld - p.MountedCenter).ToRotation();
			int dir = (Main.MouseWorld.X >= p.MountedCenter.X) ? 1 : -1;
			Projectile.spriteDirection = dir;
			p.direction = dir;

			if (holding && Projectile.owner == Main.myPlayer && Main.netMode != NetmodeID.MultiplayerClient)
				MaintainCursorMiniSlash(p);

			RingRotation += RotationSpeed;
			InnerRingRotation += InnerRotationSpeed;
			if (!Main.dedServ && Projectile.Opacity > 0.05f) {
				float ringR = (Radius + OuterRingRadiusOffset) * VisualScale * Projectile.scale;
				TrySpawnStarFX(Projectile.Center, ringR, Projectile.Opacity);
				SpawnSoftGlowDust(Projectile.Center, ringR, Projectile.Opacity);
				SpawnRedSwirlDust(Projectile.Center, ringR, Projectile.Opacity);
			}

			float damageOuterRadius = (Radius + OuterRingRadiusOffset) * VisualScale;
			float damageHalfThickness = DamageBandThickness * 0.5f;
			float r = (damageOuterRadius + damageHalfThickness) * Projectile.scale + 1f;
			int size = Math.Max(64, (int)(r * 2f));
			Projectile.width = size;
			Projectile.height = size;
			Projectile.Center = p.MountedCenter;

			if (!Main.dedServ && Projectile.Opacity > 0.05f) {
				IceAltarWarpPostProcess.EnableIceAltarWarpThisFrame = true;
				IceAltarWarpPostProcess.IceAltarWarpIntensity = WarpIntensity;
			}

			if (holding && !fadingOut) {
				float fadeIn = Utils.GetLerpValue(0f, FadeInTime, Timer, clamped: true);
				Projectile.Opacity = fadeIn;
				VisualScale = 1f - (float)Math.Pow(1f - fadeIn, 3f);
			}
			else {
				VisualScale = 1f;
			}
		}

		// 在光标处维持一个跟随旋转的小刀光，作为大圆环之外的「近身/远端」双重反馈
		private void MaintainCursorMiniSlash(Player p) {
			Vector2 pos = Main.MouseWorld;
			int type = ModContent.ProjectileType<EntelechiaScytheCursorMiniSlashProjectile>();
			int found = -1;
			float best = 999999f;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile pr = Main.projectile[i];
				if (pr == null || !pr.active || pr.owner != Projectile.owner || pr.type != type)
					continue;
				float d2 = Vector2.DistanceSquared(pr.Center, pos);
				if (d2 < best) {
					best = d2;
					found = i;
				}
			}

			int dmg = (int)(Projectile.damage * 0.65f);
			float kb = Projectile.knockBack * 0.7f;
			if (found == -1) {
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), pos, Vector2.Zero, type, dmg, kb, Projectile.owner);
				return;
			}

			Projectile mini = Main.projectile[found];
			mini.localAI[0] = pos.X;
			mini.localAI[2] = pos.Y;
			mini.damage = dmg;
			mini.knockBack = kb;
			mini.timeLeft = 6;
			mini.localAI[1] = 0f;
		}

		private void TrySpawnStarFX(Vector2 center, float outerRadius, float intensity) {
			if (Main.gamePaused || outerRadius <= 8f || !Main.rand.NextBool(120) || Main.netMode == NetmodeID.Server)
				return;

			float ang = Main.rand.NextFloat(MathHelper.TwoPi);
			float r = outerRadius * (float)Math.Sqrt(Main.rand.NextFloat());
			Vector2 local = ang.ToRotationVector2() * r;
			local.Y *= EllipseYScale;
			Vector2 pos = center + local;

			float baseScale = Main.rand.NextFloat(0.18f, 0.34f) * MathHelper.Clamp(intensity, 0.35f, 1f);
			float rotSpeed = Main.rand.NextFloat(-0.12f, 0.12f);
			int timeLeft = Main.rand.Next(22, 34);
			int id = Projectile.NewProjectile(Projectile.GetSource_FromThis(), pos, Vector2.Zero, ModContent.ProjectileType<EntelechiaScytheStarFXProjectile>(), 0, 0f, Projectile.owner, baseScale, rotSpeed);
			if (id >= 0 && id < Main.maxProjectiles) {
				Main.projectile[id].timeLeft = timeLeft;
				Main.projectile[id].netUpdate = false;
			}
		}

		public override bool? CanHitNPC(NPC target) => null;

		/// <inheritdoc />
		public void DrawIceAltarWarp() {
			if (Main.dedServ || !Projectile.active || Projectile.Opacity <= 0.05f)
				return;

			GraphicsDevice device = Main.instance.GraphicsDevice;
			Vector2 center = Projectile.Center;
			float intensity = Projectile.Opacity;
			float outer = Radius * VisualScale * Projectile.scale + Width * RenderWidthMul * Projectile.scale;
			float inner = Math.Max(outer - DamageBandThickness, 8f);
			float time = Main.GlobalTimeWrappedHourly;
			float rotation = RingRotation;

			Vector2[] outerPts = new Vector2[WarpAnnulusPoints];
			Vector2[] innerPts = new Vector2[WarpAnnulusPoints];
			for (int i = 0; i < WarpAnnulusPoints; i++) {
				float t = i / (float)WarpAnnulusPoints;
				float ang = MathHelper.TwoPi * t + rotation;
				Vector2 o0 = ang.ToRotationVector2() * outer;
				o0.Y *= EllipseYScale;
				outerPts[i] = center + o0;
				Vector2 o1 = ang.ToRotationVector2() * inner;
				o1.Y *= EllipseYScale;
				innerPts[i] = center + o1;
			}

			VertexPositionColor VertexFor(Vector2 worldPos, float baseAng, bool isOuter) {
				baseAng %= MathHelper.TwoPi;
				if (baseAng < 0f)
					baseAng += MathHelper.TwoPi;
				float wave = 0.72f + 0.28f * (float)Math.Sin(baseAng * 6f + time * 10f);
				float micro = 0.25f * (float)(Math.Sin(baseAng * 11f - time * 8f) * Math.Cos(baseAng * 7f + time * 5f));
				float dirAng = baseAng + MathHelper.PiOver2 + 0.035f * (float)Math.Sin(baseAng * 5f - time * 6f) + micro * 0.18f;
				dirAng %= MathHelper.TwoPi;
				if (dirAng < 0f)
					dirAng += MathHelper.TwoPi;
				float rEnc = dirAng / MathHelper.TwoPi;
				float gOuter = MathHelper.Clamp(0.55f * intensity * (0.65f + 0.35f * wave), 0f, 1f);
				float gInner = gOuter * 0.4f;
				float g = isOuter ? gOuter : gInner;
				float rChannel = isOuter ? rEnc : rEnc * 0.97f;
				return new VertexPositionColor(new Vector3(worldPos.X, worldPos.Y, 0f), new Color(rChannel, g, 0f, 1f));
			}

			var verts = new VertexPositionColor[WarpAnnulusPoints * 6];
			int vi = 0;
			for (int i = 0; i < WarpAnnulusPoints; i++) {
				int j = (i + 1) % WarpAnnulusPoints;
				float bi = MathHelper.TwoPi * (i / (float)WarpAnnulusPoints) + rotation;
				float bj = MathHelper.TwoPi * (j / (float)WarpAnnulusPoints) + rotation;
				VertexPositionColor oi = VertexFor(outerPts[i], bi, true);
				VertexPositionColor ii = VertexFor(innerPts[i], bi, false);
				VertexPositionColor oj = VertexFor(outerPts[j], bj, true);
				VertexPositionColor ij = VertexFor(innerPts[j], bj, false);
				verts[vi++] = oi;
				verts[vi++] = ii;
				verts[vi++] = oj;
				verts[vi++] = ii;
				verts[vi++] = oj;
				verts[vi++] = ij;
			}

			Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, 0f, 1f);
			Matrix model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0f)) * Main.GameViewMatrix.ZoomMatrix;
			using BasicEffect effect = new(device) {
				VertexColorEnabled = true,
				TextureEnabled = false,
				World = model,
				View = Matrix.Identity,
				Projection = projection
			};
			device.RasterizerState = RasterizerState.CullNone;
			device.BlendState = BlendState.Opaque;
			device.DepthStencilState = DepthStencilState.None;
			foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
				pass.Apply();
				if (verts.Length >= 9)
					device.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, verts.Length / 3);
			}
		}

		// 红色粒子：沿旋转方向（切向）略微移动，环绕刀光区域漂移
		private void SpawnRedSwirlDust(Vector2 center, float outerRadius, float intensity) {
			if (Main.gamePaused || outerRadius <= 8f || !Main.rand.NextBool(3))
				return;

			int count = Main.rand.NextBool(5) ? 2 : 1;
			for (int i = 0; i < count; i++) {
				float ang = Main.rand.NextFloat(MathHelper.TwoPi);
				float r = outerRadius * MathHelper.Lerp(0.78f, 1.02f, Main.rand.NextFloat());
				Vector2 local = ang.ToRotationVector2() * r;
				local.Y *= EllipseYScale;
				Vector2 pos = center + local;

				// 切向单位向量（与外圈旋转方向一致，RotationSpeed 为正 → 逆时针）
				Vector2 tangent = ang.ToRotationVector2().RotatedBy(MathHelper.PiOver2);
				tangent.Y *= EllipseYScale;
				tangent = tangent.SafeNormalize(Vector2.UnitX);
				Vector2 vel = tangent * (RotationSpeed * outerRadius) * Main.rand.NextFloat(0.6f, 1.1f)
					+ local.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(-0.4f, 0.6f); // 轻微径向扰动

				int dustType = Main.rand.NextBool(3) ? DustID.RedTorch : DustID.Blood;
				int idx = Dust.NewDust(pos - new Vector2(2f, 2f), 0, 0, dustType, vel.X, vel.Y, 90, default, 1f);
				Dust d = Main.dust[idx];
				d.noGravity = true;
				d.scale = Main.rand.NextFloat(0.7f, 1.25f) * MathHelper.Clamp(intensity, 0.4f, 1f);
				d.fadeIn = Main.rand.NextFloat(0.6f, 1.1f);
				if (dustType == DustID.RedTorch)
					d.color = new Color(255, 70, 60, 200);
			}
		}

		private static void SpawnSoftGlowDust(Vector2 center, float outerRadius, float intensity) {
			if (Main.gamePaused || outerRadius <= 8f || !Main.rand.NextBool(4))
				return;

			int count = Main.rand.NextBool(7) ? 2 : 1;
			for (int i = 0; i < count; i++) {
				float ang = Main.rand.NextFloat(MathHelper.TwoPi);
				float r = outerRadius * (float)Math.Sqrt(Main.rand.NextFloat());
				Vector2 local = ang.ToRotationVector2() * r;
				local.Y *= EllipseYScale;
				Vector2 pos = center + local;

				float wind = Main.windSpeedCurrent;
				float drift = Main.rand.NextFloat(-1.25f, 1.25f);
				float up = Main.rand.NextFloat(-4.1f, -1.2f);
				Vector2 vel = new Vector2(drift + wind * 3.0f, up);
				vel *= Main.rand.NextFloat(0.85f, 1.35f);
				int idx = Dust.NewDust(pos - new Vector2(2f, 2f), 0, 0, DustID.WhiteTorch, vel.X, vel.Y, 90, default, 1f);
				Dust d = Main.dust[idx];
				d.noGravity = true;
				d.noLight = false;
				d.scale = Main.rand.NextFloat(0.55f, 1.05f) * MathHelper.Clamp(intensity, 0.35f, 1f);
				d.fadeIn = Main.rand.NextFloat(0.9f, 1.6f);
				d.alpha = 70;
				d.color = new Color(255, 255, 255, 255);
			}
		}

		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			float damageOuterRadius = (Radius + OuterRingRadiusOffset) * VisualScale;
			float damageHalfThickness = DamageBandThickness * 0.5f;
			float r = (damageOuterRadius + damageHalfThickness) * Projectile.scale + 1f;
			Vector2 c = Projectile.Center;
			hitbox = new Rectangle((int)(c.X - r), (int)(c.Y - r), (int)(r * 2f), (int)(r * 2f));
		}

		private static void BuildStripVerts(Vector2[] points, VertexPositionColorTexture[] verts, float width, float intensity) {
			for (int i = 0; i < points.Length; i++) {
				float t = (points.Length <= 1) ? 0f : i / (float)(points.Length - 1);
				float a = MathHelper.Clamp(intensity, 0f, 1f);
				Color c = Color.White;
				c.A = (byte)(255f * a);

				Vector2 cur = points[i];
				Vector2 prev = (i == 0) ? points[i] : points[i - 1];
				Vector2 next = (i == points.Length - 1) ? points[i] : points[i + 1];
				Vector2 d1 = (points[i] - prev).SafeNormalize(Vector2.UnitX);
				Vector2 d2 = (next - points[i]).SafeNormalize(Vector2.UnitX);
				Vector2 d = (i == 0) ? d2 : (i == points.Length - 1 ? d1 : (d1 + d2).SafeNormalize(d2));
				Vector2 normal = d.RotatedBy(MathHelper.PiOver2);
				Vector2 top = cur + normal * width;
				Vector2 bottom = cur - normal * width;

				verts[i * 2] = new VertexPositionColorTexture(new Vector3(top, 0f), c, new Vector2(0f, t));
				verts[i * 2 + 1] = new VertexPositionColorTexture(new Vector3(bottom, 0f), c, new Vector2(1f, t));
			}
		}

		private static void BuildStripVerts(Vector2[] points, VertexPositionColorTexture[] verts, float width, float intensity, Color tint) {
			for (int i = 0; i < points.Length; i++) {
				float t = (points.Length <= 1) ? 0f : i / (float)(points.Length - 1);
				float a = MathHelper.Clamp(intensity, 0f, 1f);
				Color c = tint;
				c.A = (byte)(c.A * a);

				Vector2 cur = points[i];
				Vector2 prev = (i == 0) ? points[i] : points[i - 1];
				Vector2 next = (i == points.Length - 1) ? points[i] : points[i + 1];
				Vector2 d1 = (points[i] - prev).SafeNormalize(Vector2.UnitX);
				Vector2 d2 = (next - points[i]).SafeNormalize(Vector2.UnitX);
				Vector2 d = (i == 0) ? d2 : (i == points.Length - 1 ? d1 : (d1 + d2).SafeNormalize(d2));
				Vector2 normal = d.RotatedBy(MathHelper.PiOver2);
				Vector2 top = cur + normal * width;
				Vector2 bottom = cur - normal * width;

				verts[i * 2] = new VertexPositionColorTexture(new Vector3(top, 0f), c, new Vector2(0f, t));
				verts[i * 2 + 1] = new VertexPositionColorTexture(new Vector3(bottom, 0f), c, new Vector2(1f, t));
			}
		}

		private void BuildCircleSegmentPoints(Vector2 center, float radius, float startAngle, float endAngle, Vector2[] points) {
			for (int i = 0; i < points.Length; i++) {
				float t = (points.Length <= 1) ? 0f : i / (float)(points.Length - 1);
				float ang = MathHelper.Lerp(startAngle, endAngle, t);
				Vector2 o = ang.ToRotationVector2() * radius;
				o.Y *= EllipseYScale;
				points[i] = center + o;
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ || Projectile.Opacity <= 0.001f)
				return false;

			Player p = Owner;
			if (p == null || !p.active)
				return false;

			GraphicsDevice device = Main.instance.GraphicsDevice;
			Texture2D shapeTex = ModContent.Request<Texture2D>(ShapeTexturePath).Value;
			Texture2D innerShapeTex = ModContent.Request<Texture2D>(InnerShapeTexturePath).Value;

			Vector2 center = Projectile.Center;
			float intensity = Projectile.Opacity;
			float width = Width * RenderWidthMul * Projectile.scale;
			float innerWidth = width * InnerWidthMul;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, 0f, 1f);
			Matrix model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0f)) * Main.GameViewMatrix.ZoomMatrix;
			BasicEffect effect = new(device) {
				TextureEnabled = true,
				VertexColorEnabled = true,
				World = model,
				View = Matrix.Identity,
				Projection = projection
			};

			Main.spriteBatch.End();
			BlendState oldBlend = device.BlendState;
			SamplerState oldSampler = device.SamplerStates[0];
			RasterizerState oldRasterizer = device.RasterizerState;
			DepthStencilState oldDepth = device.DepthStencilState;
			try {
				device.RasterizerState = RasterizerState.CullNone;
				device.SamplerStates[0] = SamplerState.LinearWrap;
				device.DepthStencilState = DepthStencilState.None;

				device.BlendState = BlendState.NonPremultiplied;
				effect.Texture = shapeTex;
				foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
					pass.Apply();

					// 外圈叠加的双层背景刀光：比主环略大、转速不同，与主环叠加出更厚重的不透明度堆叠
					float bgBaseRot = RingRotation + Main.GlobalTimeWrappedHourly * 0.85f;
					Color bgTint = new Color(255, 252, 255, 90);
					for (int ring = 0; ring < 2; ring++) {
						float bgRot = bgBaseRot + ring * 0.9f;
						float bgRadiusMul = 0.92f - ring * 0.05f;
						float bgWidthMul = 1.18f - ring * 0.08f;
						for (int s = 0; s < Segments; s++) {
							float a0 = MathHelper.TwoPi * s / Segments - SegmentOverlapAngle + bgRot;
							float a1 = MathHelper.TwoPi * (s + 1) / Segments + SegmentOverlapAngle + bgRot;
							Vector2[] bgPts = new Vector2[PointsPerSegment];
							BuildCircleSegmentPoints(center, (Radius + OuterRingRadiusOffset) * bgRadiusMul * VisualScale * Projectile.scale, a0, a1, bgPts);
							int bgVertCount = bgPts.Length * 2;
							if (bgVertCount < 6)
								continue;
							VertexPositionColorTexture[] bgVerts = new VertexPositionColorTexture[bgVertCount];
							BuildStripVerts(bgPts, bgVerts, width * bgWidthMul, intensity * 0.30f, bgTint);
							device.DrawUserPrimitives(PrimitiveType.TriangleStrip, bgVerts, 0, bgVertCount - 2);
						}
					}

					for (int s = 0; s < Segments; s++) {
						float a0 = MathHelper.TwoPi * s / Segments - SegmentOverlapAngle + RingRotation;
						float a1 = MathHelper.TwoPi * (s + 1) / Segments + SegmentOverlapAngle + RingRotation;
						Vector2[] pts = new Vector2[PointsPerSegment];
						BuildCircleSegmentPoints(center, Radius * VisualScale * Projectile.scale, a0, a1, pts);

						int vertCount = pts.Length * 2;
						if (vertCount < 6)
							continue;
						VertexPositionColorTexture[] verts = new VertexPositionColorTexture[vertCount];
						BuildStripVerts(pts, verts, width, intensity);
						device.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts, 0, vertCount - 2);
					}
				}

				device.BlendState = BlendState.Additive;
				effect.Texture = shapeTex;
				foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
					pass.Apply();
					float glowIntensity = intensity * 0.28f;
					float glowWidth = width * 1.22f;
					for (int s = 0; s < Segments; s++) {
						float a0 = MathHelper.TwoPi * s / Segments - SegmentOverlapAngle + RingRotation;
						float a1 = MathHelper.TwoPi * (s + 1) / Segments + SegmentOverlapAngle + RingRotation;
						Vector2[] pts = new Vector2[PointsPerSegment];
						BuildCircleSegmentPoints(center, Radius * VisualScale * Projectile.scale, a0, a1, pts);

						int vertCount = pts.Length * 2;
						if (vertCount < 6)
							continue;
						VertexPositionColorTexture[] verts = new VertexPositionColorTexture[vertCount];
						BuildStripVerts(pts, verts, glowWidth, glowIntensity);
						device.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts, 0, vertCount - 2);
					}
				}

				effect.Texture = innerShapeTex;
				foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
					pass.Apply();
					for (int s = 0; s < InnerSegments; s++) {
						float a0 = MathHelper.TwoPi * s / InnerSegments - SegmentOverlapAngle + InnerRingRotation;
						float a1 = MathHelper.TwoPi * (s + 1) / InnerSegments + SegmentOverlapAngle + InnerRingRotation;
						Vector2[] pts = new Vector2[PointsPerSegment];
						BuildCircleSegmentPoints(center, Radius * InnerRadiusMul * VisualScale * Projectile.scale, a0, a1, pts);

						int vertCount = pts.Length * 2;
						if (vertCount < 6)
							continue;
						VertexPositionColorTexture[] verts = new VertexPositionColorTexture[vertCount];
						BuildStripVerts(pts, verts, innerWidth, intensity);
						device.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts, 0, vertCount - 2);
					}
				}

				device.BlendState = BlendState.Additive;
				effect.Texture = innerShapeTex;
				foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
					pass.Apply();
					float glowIntensity = intensity * 0.24f;
					float glowWidth = innerWidth * 1.22f;
					for (int s = 0; s < InnerSegments; s++) {
						float a0 = MathHelper.TwoPi * s / InnerSegments - SegmentOverlapAngle + InnerRingRotation;
						float a1 = MathHelper.TwoPi * (s + 1) / InnerSegments + SegmentOverlapAngle + InnerRingRotation;
						Vector2[] pts = new Vector2[PointsPerSegment];
						BuildCircleSegmentPoints(center, Radius * InnerRadiusMul * VisualScale * Projectile.scale, a0, a1, pts);

						int vertCount = pts.Length * 2;
						if (vertCount < 6)
							continue;
						VertexPositionColorTexture[] verts = new VertexPositionColorTexture[vertCount];
						BuildStripVerts(pts, verts, glowWidth, glowIntensity);
						device.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts, 0, vertCount - 2);
					}
				}
			}
			finally {
				device.BlendState = oldBlend;
				device.SamplerStates[0] = oldSampler;
				device.RasterizerState = oldRasterizer;
				device.DepthStencilState = oldDepth;
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}

			return false;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Player p = Owner;
			if (p == null || !p.active || Projectile.Opacity <= 0.05f)
				return false;

			Vector2 center = Projectile.Center;
			float outerR = (Radius + OuterRingRadiusOffset) * VisualScale * Projectile.scale;

			float closestX = MathHelper.Clamp(center.X, targetHitbox.Left, targetHitbox.Right);
			float closestY = MathHelper.Clamp(center.Y, targetHitbox.Top, targetHitbox.Bottom);
			Vector2 closest = new(closestX, closestY);
			Vector2 local = closest - center;
			local.Y /= EllipseYScale;
			return local.Length() <= outerR;
		}
	}

	/// <summary>循环斩光圈周边的星光粒子，纯视觉效果。</summary>
	public class EntelechiaScytheStarFXProjectile : ModProjectile
	{
		public override string Texture => "ArknightsMod/Content/Projectiles/Guard/Entelechia/EntelechiaScytheStarFX";

		public override void SetDefaults() {
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 28;
			Projectile.alpha = 0;
		}

		public override void AI() {
			Projectile.localAI[0] += 1f;
			float age = Projectile.localAI[0];
			float life = Math.Max(1, Projectile.timeLeft + age);
			float t = age / life;
			float baseScale = Projectile.ai[0];
			float rotSpeed = Projectile.ai[1];
			Projectile.rotation += rotSpeed;

			float growT = MathHelper.Clamp(t / 0.25f, 0f, 1f);
			float shrinkT = MathHelper.Clamp((t - 0.25f) / 0.75f, 0f, 1f);
			float grow = 1f - (1f - growT) * (1f - growT);
			float shrink = 1f - shrinkT;
			Projectile.scale = baseScale * MathHelper.Lerp(0.15f, 1f, grow) * shrink;
			Projectile.Opacity = MathHelper.Clamp(shrink * 1.15f, 0f, 1f);

			float light = 0.25f * Projectile.Opacity;
			Lighting.AddLight(Projectile.Center, light, light, light);
		}

		public override bool PreDraw(ref Color lightColor) {
			Texture2D tex = TextureAssets.Projectile[Type].Value;
			Vector2 origin = tex.Size() * 0.5f;
			Vector2 pos = Projectile.Center - Main.screenPosition;

			SpriteBatch sb = Main.spriteBatch;
			try { sb.End(); } catch { }
			sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			Color c = new Color(255, 255, 255, 255) * (0.75f * Projectile.Opacity);
			float s = Projectile.scale;
			sb.Draw(tex, pos, null, c * 0.55f, Projectile.rotation, origin, s * 1.15f, SpriteEffects.None, 0f);
			sb.Draw(tex, pos, null, c * 0.35f, Projectile.rotation, origin, s * 1.45f, SpriteEffects.None, 0f);
			sb.Draw(tex, pos, null, c, Projectile.rotation, origin, s, SpriteEffects.None, 0f);
			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			return false;
		}
	}

	/// <summary>跟随光标旋转的小刀光，循环斩激活期间维持在鼠标位置，作为远端命中反馈。</summary>
	public class EntelechiaScytheCursorMiniSlashProjectile : ModProjectile
	{
		private const float MiniRadius = 88f;
		private const float MiniWidth = 18f;
		private const float MiniEllipseYScale = 0.92f;
		private const int MiniSegments = 3;
		private const int MiniPointsPerSegment = 18;
		private const float MiniSegmentOverlapAngle = 0.09f;
		private const float MiniRotationSpeed = 0.09f;

		private float Rot {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override string Texture => "ArknightsMod/Content/Projectiles/Guard/Entelechia/EntelechiaScytheSlash";

		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 6;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override void AI() {
			Rot += MiniRotationSpeed;
			float refreshedFlag = Projectile.localAI[1];
			bool refreshedThisFrame = refreshedFlag == 0f;
			Projectile.localAI[1] = refreshedFlag + 1f;
			Projectile.Opacity = refreshedThisFrame ? 1f : MathHelper.Clamp(Projectile.timeLeft / 6f, 0f, 1f);

			Vector2 target = new(Projectile.localAI[0], Projectile.localAI[2]);
			if (refreshedThisFrame) {
				Vector2 to = target - Projectile.Center;
				float dist = to.Length();
				if (dist <= 1.25f) {
					Projectile.Center = target;
				}
				else {
					float speed = MathHelper.Clamp(dist * 0.42f, 6f, 42f);
					float maxStep = Math.Min(speed, dist);
					Projectile.Center += to * (maxStep / dist);
				}
			}
			Projectile.rotation = Rot;
			Projectile.velocity = Vector2.Zero;
			if (!Main.dedServ && Projectile.Opacity > 0.05f)
				SpawnMiniGlowDust(Projectile.Center, MiniRadius, Projectile.Opacity);

			float light = 0.12f * Projectile.Opacity;
			Lighting.AddLight(Projectile.Center, light, light, light);
		}

		private static void SpawnMiniGlowDust(Vector2 center, float outerRadius, float intensity) {
			if (Main.gamePaused || outerRadius <= 8f || !Main.rand.NextBool(8))
				return;

			int count = Main.rand.NextBool(10) ? 2 : 1;
			for (int i = 0; i < count; i++) {
				float ang = Main.rand.NextFloat(MathHelper.TwoPi);
				float r = outerRadius * (float)Math.Sqrt(Main.rand.NextFloat());
				Vector2 local = ang.ToRotationVector2() * r;
				local.Y *= MiniEllipseYScale;
				Vector2 pos = center + local;

				float wind = Main.windSpeedCurrent;
				float drift = Main.rand.NextFloat(-0.75f, 0.75f);
				float up = Main.rand.NextFloat(-2.8f, -1.0f);
				Vector2 vel = new Vector2(drift + wind * 1.6f, up);
				vel *= Main.rand.NextFloat(0.9f, 1.2f);
				int idx = Dust.NewDust(pos - new Vector2(2f, 2f), 0, 0, DustID.WhiteTorch, vel.X, vel.Y, 110, default, 1f);
				Dust d = Main.dust[idx];
				d.noGravity = true;
				d.noLight = false;
				d.scale = Main.rand.NextFloat(0.35f, 0.70f) * MathHelper.Clamp(intensity, 0.35f, 1f);
				d.fadeIn = Main.rand.NextFloat(0.8f, 1.3f);
				d.alpha = 120;
				d.color = new Color(255, 255, 255, 255);
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (Projectile.Opacity <= 0.05f)
				return false;
			Vector2 center = Projectile.Center;
			float closestX = MathHelper.Clamp(center.X, targetHitbox.Left, targetHitbox.Right);
			float closestY = MathHelper.Clamp(center.Y, targetHitbox.Top, targetHitbox.Bottom);
			Vector2 closest = new(closestX, closestY);
			Vector2 local = closest - center;
			local.Y /= MiniEllipseYScale;
			return local.Length() <= MiniRadius;
		}

		private static void BuildCircleSegmentPoints(Vector2 center, float radius, float startAngle, float endAngle, Vector2[] points) {
			for (int i = 0; i < points.Length; i++) {
				float tt = (points.Length <= 1) ? 0f : i / (float)(points.Length - 1);
				float ang = MathHelper.Lerp(startAngle, endAngle, tt);
				Vector2 o = ang.ToRotationVector2() * radius;
				o.Y *= MiniEllipseYScale;
				points[i] = center + o;
			}
		}

		private static void BuildStripVerts(Vector2[] points, VertexPositionColorTexture[] verts, float width, float intensity) {
			for (int i = 0; i < points.Length; i++) {
				float t = (points.Length <= 1) ? 0f : i / (float)(points.Length - 1);
				float a = MathHelper.Clamp(intensity, 0f, 1f);
				Color c = Color.White;
				c.A = (byte)(255f * a);

				Vector2 cur = points[i];
				Vector2 prev = (i == 0) ? points[i] : points[i - 1];
				Vector2 next = (i == points.Length - 1) ? points[i] : points[i + 1];
				Vector2 d1 = (points[i] - prev).SafeNormalize(Vector2.UnitX);
				Vector2 d2 = (next - points[i]).SafeNormalize(Vector2.UnitX);
				Vector2 d = (i == 0) ? d2 : (i == points.Length - 1 ? d1 : (d1 + d2).SafeNormalize(d2));
				Vector2 normal = d.RotatedBy(MathHelper.PiOver2);
				Vector2 top = cur + normal * width;
				Vector2 bottom = cur - normal * width;

				verts[i * 2] = new VertexPositionColorTexture(new Vector3(top, 0f), c, new Vector2(0f, t));
				verts[i * 2 + 1] = new VertexPositionColorTexture(new Vector3(bottom, 0f), c, new Vector2(1f, t));
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ || Projectile.Opacity <= 0.001f)
				return false;

			GraphicsDevice device = Main.instance.GraphicsDevice;
			Texture2D tex = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Guard/Entelechia/EntelechiaScytheSlash").Value;
			Vector2 center = Projectile.Center;
			float intensity = Projectile.Opacity;
			float width = MiniWidth;

			Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, 0f, 1f);
			Matrix model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0f)) * Main.GameViewMatrix.ZoomMatrix;
			BasicEffect effect = new(device) {
				TextureEnabled = true,
				VertexColorEnabled = true,
				World = model,
				View = Matrix.Identity,
				Projection = projection,
				Texture = tex
			};

			Main.spriteBatch.End();
			BlendState oldBlend = device.BlendState;
			SamplerState oldSampler = device.SamplerStates[0];
			RasterizerState oldRasterizer = device.RasterizerState;
			DepthStencilState oldDepth = device.DepthStencilState;
			try {
				device.RasterizerState = RasterizerState.CullNone;
				device.SamplerStates[0] = SamplerState.LinearWrap;
				device.DepthStencilState = DepthStencilState.None;
				device.BlendState = BlendState.NonPremultiplied;
				foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
					pass.Apply();
					for (int s = 0; s < MiniSegments; s++) {
						float a0 = MathHelper.TwoPi * s / MiniSegments - MiniSegmentOverlapAngle + Rot;
						float a1 = MathHelper.TwoPi * (s + 1) / MiniSegments + MiniSegmentOverlapAngle + Rot;
						Vector2[] pts = new Vector2[MiniPointsPerSegment];
						BuildCircleSegmentPoints(center, MiniRadius, a0, a1, pts);
						int vertCount = pts.Length * 2;
						if (vertCount < 6)
							continue;
						VertexPositionColorTexture[] verts = new VertexPositionColorTexture[vertCount];
						BuildStripVerts(pts, verts, width, intensity);
						device.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts, 0, vertCount - 2);
					}
				}

				device.BlendState = BlendState.Additive;
				foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
					pass.Apply();
					float glowIntensity = intensity * 0.35f;
					float glowWidth = width * 1.25f;
					for (int s = 0; s < MiniSegments; s++) {
						float a0 = MathHelper.TwoPi * s / MiniSegments - MiniSegmentOverlapAngle + Rot;
						float a1 = MathHelper.TwoPi * (s + 1) / MiniSegments + MiniSegmentOverlapAngle + Rot;
						Vector2[] pts = new Vector2[MiniPointsPerSegment];
						BuildCircleSegmentPoints(center, MiniRadius, a0, a1, pts);
						int vertCount = pts.Length * 2;
						if (vertCount < 6)
							continue;
						VertexPositionColorTexture[] verts = new VertexPositionColorTexture[vertCount];
						BuildStripVerts(pts, verts, glowWidth, glowIntensity);
						device.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts, 0, vertCount - 2);
					}
				}
			}
			finally {
				device.BlendState = oldBlend;
				device.SamplerStates[0] = oldSampler;
				device.RasterizerState = oldRasterizer;
				device.DepthStencilState = oldDepth;
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}
			return false;
		}
	}
}
