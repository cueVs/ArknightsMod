using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP2;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP1;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.EffectImage;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.NormalAttack;
namespace ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP2
{
	public class InstinctCallExplosion
	{

		public const int EffectDuration = 120;
		private const int SphereSegments = 32;
		private const float SphereRadius = 250f;
		private const int DarkSphereSegments = 32;


		private const int RibbonPointCount = 60;

		private const float RibbonRadiusMult = 1.05f;
		private const int RibbonMinCount = 15;
		private const int RibbonMaxCount = 25;
		private const float RibbonThetaMin = 0.15f;
		private const float RibbonThetaMax = 0.85f;
		private const float RibbonArcMin = 250f;
		private const float RibbonArcMax = 400f;
		private const float RibbonWidthMin = 20f;
		private const float RibbonWidthMax = 40f;
		private const float RibbonRotSpeedMin = 0.3f;
		private const float RibbonRotSpeedMax = 0.7f;
		private static readonly Color RibbonBaseColor = new Color(60, 20, 100);

		private const float BlackRibbonRadiusMult = 1.08f;
		private const int BlackRibbonMinCount = 18;
		private const int BlackRibbonMaxCount = 28;
		private const float BlackRibbonThetaMin = 0.275f;
		private const float BlackRibbonThetaMax = 0.97f;
		private const float BlackRibbonArcMin = 200f;
		private const float BlackRibbonArcMax = 350f;
		private const float BlackRibbonWidthMin = 18f;
		private const float BlackRibbonWidthMax = 32f;
		private const float BlackRibbonRotSpeedMin = 0.3f;
		private const float BlackRibbonRotSpeedMax = 0.7f;
		private static readonly Color BlackRibbonBaseColor = new Color(25, 8, 40);
		private static readonly Color BlackRibbonEdgeColor = new Color(40, 15, 60);

		private const float MidRibbonRadiusMult = 1.05f;
		private const int MidRibbonMinCount = 15;
		private const int MidRibbonMaxCount = 25;
		private const float MidRibbonThetaMin = 0.15f;
		private const float MidRibbonThetaMax = 0.85f;
		private const float MidRibbonArcMin = 250f;
		private const float MidRibbonArcMax = 400f;
		private const float MidRibbonWidthMin = 20f;
		private const float MidRibbonWidthMax = 40f;
		private const float MidRibbonRotSpeedMin = 0.3f;
		private const float MidRibbonRotSpeedMax = 0.7f;
		private static readonly Color MidRibbonBaseColor = new Color(50, 17, 80);


		private const float RingOuterRadiusA = 0.95f;
		private const float RingOuterRadiusB = 0.5225f;
		private const float RingMidRadiusA = 0.75f;
		private const float RingMidRadiusB = 0.4125f;
		private const float RingInnerRadiusA = 0.55f;
		private const float RingInnerRadiusB = 0.3025f;
		private const float RingBaseAngularSpeed = 0.3f;
		private const float RingAngularSpeedIncrement = 0.05f;
		private static readonly Color RingOuterColor = new Color(220, 180, 255, 255);
		private static readonly Color RingMidColor = new Color(180, 120, 255, 255);
		private static readonly Color RingInnerColor = new Color(140, 80, 220, 255);

		// 绘制坐标偏移
		private const float RibbonYScale = 0.85f;
		private const float RibbonZFactor = 0.6f;
		private const float BlackRibbonYScale = 1.0f;
		private const float BlackRibbonZFactor = 0.4f;


		private const float RibbonAlphaPower = 1.5f;
		private const float BlackRibbonAlphaPower = 1.8f;
		private const float MidRibbonAlphaPower = 1.65f;
		private const float SphereScaleStart = 0.85f;
		private const float SphereScaleEnd = 1.45f;
		private const float SphereScalePower = 0.5f;
		private const float SphereFadeInTime = 0.06f;
		private const float SphereFadeOutEnd = 0.25f;
		private const float SphereFadeOutMultiplier = 2.5f;
		private const float DarkSphereFadeInTime = 0.04f;
		private const float DarkSphereFadeOutEnd = 0.18f;
		private const float DarkSphereFadeOutMultiplier = 2f;


		private const float DarkSphereRadiusMult = 1.12f;
		private static readonly Color DarkSphereBottomColor = new Color(40, 15, 70);
		private static readonly Color DarkSphereTopColor = new Color(100, 60, 160);
		private static readonly Color SphereBottomColor = new Color(30, 10, 60);
		private static readonly Color SphereTopColor = new Color(220, 180, 255);

		private const string GradientTexPath = "ArknightsMod/Content/Projectiles/Caster/Valarqvin/Effect/LightningGradient_2";

		//状态 
		private BasicEffect colorEffect;
		private BasicEffect textureEffect;
		private VertexPositionColor[] sphereVertices;
		private short[] sphereIndices;
		private VertexPositionColor[] darkSphereVertices;
		private short[] darkSphereIndices;
		private Texture2D gradientTexture;
		private float explodeAngle;

		private List<Ribbon> ribbons;
		private List<BlackRibbon> blackRibbons;
		private List<MidRibbon> midRibbons;

		private int owner;


		private abstract class BaseRibbon
		{
			public List<Vector3> Points;
			public float Width;
			public float RotationSpeed;

			public abstract float RadiusMult { get; }
			public abstract int MinCount { get; }
			public abstract int MaxCount { get; }
			public abstract float ThetaMin { get; }
			public abstract float ThetaMax { get; }
			public abstract float ArcMin { get; }
			public abstract float ArcMax { get; }
			public abstract float WidthMin { get; }
			public abstract float WidthMax { get; }
			public abstract float RotSpeedMin { get; }
			public abstract float RotSpeedMax { get; }

			public virtual void Generate(UnifiedRandom rand, float baseRadius) {
				float visualRadius = baseRadius * RadiusMult;
				float theta = rand.NextFloat(ThetaMin, ThetaMax);

				float cosT = (float)Math.Cos(theta);
				float sinT = (float)Math.Sin(theta);
				float circleR = visualRadius * sinT;
				float yH = visualRadius * cosT;

				float arcLen = rand.NextFloat(ArcMin, ArcMax);
				float arcAngle = arcLen / circleR;
				float startPhi = rand.NextFloat(0, MathHelper.TwoPi);

				Points = new List<Vector3>();
				for (int j = 0; j <= RibbonPointCount; j++) {
					float phi = startPhi + arcAngle * (j / (float)RibbonPointCount);
					Points.Add(new Vector3((float)Math.Cos(phi) * circleR, yH, (float)Math.Sin(phi) * circleR));
				}

				Width = rand.NextFloat(WidthMin, WidthMax);
				RotationSpeed = rand.NextFloat(RotSpeedMin, RotSpeedMax);
			}

			public virtual Color GetBaseColor() => Color.White;
			public virtual bool UseTexture() => false;
			public virtual float GetYScale() => RibbonYScale;
			public virtual float GetZFactor() => RibbonZFactor;
		}

		private class Ribbon : BaseRibbon
		{
			public override float RadiusMult => RibbonRadiusMult;
			public override int MinCount => RibbonMinCount;
			public override int MaxCount => RibbonMaxCount;
			public override float ThetaMin => RibbonThetaMin;
			public override float ThetaMax => RibbonThetaMax * (MathHelper.PiOver2);
			public override float ArcMin => RibbonArcMin;
			public override float ArcMax => RibbonArcMax;
			public override float WidthMin => RibbonWidthMin;
			public override float WidthMax => RibbonWidthMax;
			public override float RotSpeedMin => RibbonRotSpeedMin;
			public override float RotSpeedMax => RibbonRotSpeedMax;
			public override Color GetBaseColor() => RibbonBaseColor;
		}

		private class BlackRibbon : BaseRibbon
		{
			public override float RadiusMult => BlackRibbonRadiusMult;
			public override int MinCount => BlackRibbonMinCount;
			public override int MaxCount => BlackRibbonMaxCount;
			public override float ThetaMin => BlackRibbonThetaMin * (MathHelper.PiOver2);
			public override float ThetaMax => BlackRibbonThetaMax * (MathHelper.PiOver2);
			public override float ArcMin => BlackRibbonArcMin;
			public override float ArcMax => BlackRibbonArcMax;
			public override float WidthMin => BlackRibbonWidthMin;
			public override float WidthMax => BlackRibbonWidthMax;
			public override float RotSpeedMin => BlackRibbonRotSpeedMin;
			public override float RotSpeedMax => BlackRibbonRotSpeedMax;
			public override Color GetBaseColor() => BlackRibbonBaseColor;
			public override bool UseTexture() => true;
			public override float GetYScale() => BlackRibbonYScale;
			public override float GetZFactor() => BlackRibbonZFactor;
		}

		private class MidRibbon : BaseRibbon
		{
			public override float RadiusMult => MidRibbonRadiusMult;
			public override int MinCount => MidRibbonMinCount;
			public override int MaxCount => MidRibbonMaxCount;
			public override float ThetaMin => MidRibbonThetaMin;
			public override float ThetaMax => MidRibbonThetaMax * (MathHelper.PiOver2);
			public override float ArcMin => MidRibbonArcMin;
			public override float ArcMax => MidRibbonArcMax;
			public override float WidthMin => MidRibbonWidthMin;
			public override float WidthMax => MidRibbonWidthMax;
			public override float RotSpeedMin => MidRibbonRotSpeedMin;
			public override float RotSpeedMax => MidRibbonRotSpeedMax;
			public override Color GetBaseColor() => MidRibbonBaseColor;
		}

		private struct VertexPositionColorTexture : IVertexType
		{
			public Vector3 Position;
			public Color Color;
			public Vector2 TextureCoordinate;

			public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
				new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
				new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
				new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
			);

			public VertexPositionColorTexture(Vector3 position, Color color, Vector2 textureCoordinate) {
				Position = position;
				Color = color;
				TextureCoordinate = textureCoordinate;
			}

			VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;
		}

		public void Initialize(Vector2 center, int ownerIndex) {
			owner = ownerIndex;
			explodeAngle = Main.rand.NextFloat(MathHelper.TwoPi);

			if (gradientTexture == null)
				gradientTexture = ModContent.Request<Texture2D>(GradientTexPath).Value;

			GenerateSphereMesh();
			GenerateDarkSphereMesh();
			GenerateRibbonList<MidRibbon>(ref midRibbons, SphereRadius);
			GenerateRibbonList<Ribbon>(ref ribbons, SphereRadius);
			GenerateRibbonList<BlackRibbon>(ref blackRibbons, SphereRadius);

			CreateRingDust(center, 0, RingOuterRadiusA, RingOuterRadiusB, RingOuterColor, 6);
			CreateRingDust(center, 1, RingMidRadiusA, RingMidRadiusB, RingMidColor, 5);
			CreateRingDust(center, 2, RingInnerRadiusA, RingInnerRadiusB, RingInnerColor, 4);
		}

		public void Dispose() {
			colorEffect?.Dispose();
			textureEffect?.Dispose();
			colorEffect = null;
			textureEffect = null;

			// 清理顶点和索引数组
			sphereVertices = null;
			sphereIndices = null;
			darkSphereVertices = null;
			darkSphereIndices = null;

			// 清理
			ribbons?.Clear();
			blackRibbons?.Clear();
			midRibbons?.Clear();
			ribbons = null;
			blackRibbons = null;
			midRibbons = null;


		}

		private void CreateRingDust(Vector2 center, int ringIndex, float radiusA, float radiusB, Color color, int particleCount) {
			Projectile dust = Projectile.NewProjectileDirect(
				null,
				center,
				Vector2.Zero,
				ModContent.ProjectileType<InstinctCallDust>(),
				0, 0f, owner);

			if (dust.ModProjectile is InstinctCallDust dustProj) {
				dustProj.EllipseA = SphereRadius * radiusA;
				dustProj.EllipseB = SphereRadius * radiusB;
				dustProj.AngularSpeed = RingBaseAngularSpeed + ringIndex * RingAngularSpeedIncrement;
				dustProj.ParticleCount = particleCount;
				dustProj.TrailColor = color;
				dustProj.DustAlpha = 1.0f - ringIndex * 0.15f;
			}
		}

		private void GenerateRibbonList<T>(ref List<T> list, float baseRadius) where T : BaseRibbon, new() {
			T template = new T();
			int count = Main.rand.Next(template.MinCount, template.MaxCount);

			List<float> thetas = new List<float>();
			for (int i = 0; i < count; i++)
				thetas.Add(Main.rand.NextFloat(template.ThetaMin, template.ThetaMax));
			thetas.Sort();

			list = new List<T>();
			foreach (float theta in thetas) {
				T ribbon = new T();
				float visualRadius = baseRadius * ribbon.RadiusMult;
				float cosT = (float)Math.Cos(theta);
				float sinT = (float)Math.Sin(theta);
				float circleR = visualRadius * sinT;
				float yH = visualRadius * cosT;

				float arcLen = Main.rand.NextFloat(ribbon.ArcMin, ribbon.ArcMax);
				float arcAngle = arcLen / circleR;
				float startPhi = Main.rand.NextFloat(0, MathHelper.TwoPi);

				ribbon.Points = new List<Vector3>();
				for (int j = 0; j <= RibbonPointCount; j++) {
					float phi = startPhi + arcAngle * (j / (float)RibbonPointCount);
					ribbon.Points.Add(new Vector3((float)Math.Cos(phi) * circleR, yH, (float)Math.Sin(phi) * circleR));
				}

				ribbon.Width = Main.rand.NextFloat(ribbon.WidthMin, ribbon.WidthMax);
				ribbon.RotationSpeed = Main.rand.NextFloat(ribbon.RotSpeedMin, ribbon.RotSpeedMax);
				list.Add(ribbon);
			}
		}

		private void GenerateSphereMesh() {
			int latSegments = SphereSegments / 2;
			int lonSegments = SphereSegments;
			int vertCount = (latSegments + 1) * (lonSegments + 1);
			sphereVertices = new VertexPositionColor[vertCount];
			int indexCount = latSegments * lonSegments * 6;
			sphereIndices = new short[indexCount];

			int vi = 0;
			for (int lat = 0; lat <= latSegments; lat++) {
				float theta = lat * MathHelper.PiOver2 / latSegments;
				float sinTheta = (float)Math.Sin(theta);
				float cosTheta = (float)Math.Cos(theta);
				for (int lon = 0; lon <= lonSegments; lon++) {
					float phi = lon * MathHelper.TwoPi / lonSegments;
					float x = (float)Math.Cos(phi) * sinTheta * SphereRadius;
					float y = cosTheta * SphereRadius;
					float z = (float)Math.Sin(phi) * sinTheta * SphereRadius;
					float heightFactor = y / SphereRadius;
					Color color = Color.Lerp(SphereBottomColor, SphereTopColor, heightFactor);
					sphereVertices[vi++] = new VertexPositionColor(new Vector3(x, y, z), color);
				}
			}

			int ii = 0;
			for (int lat = 0; lat < latSegments; lat++) {
				for (int lon = 0; lon < lonSegments; lon++) {
					short tl = (short)(lat * (lonSegments + 1) + lon);
					short tr = (short)(tl + 1);
					short bl = (short)((lat + 1) * (lonSegments + 1) + lon);
					short br = (short)(bl + 1);
					sphereIndices[ii++] = tl;
					sphereIndices[ii++] = bl;
					sphereIndices[ii++] = tr;
					sphereIndices[ii++] = tr;
					sphereIndices[ii++] = bl;
					sphereIndices[ii++] = br;
				}
			}
		}

		private void GenerateDarkSphereMesh() {
			int latSegments = DarkSphereSegments / 2;
			int lonSegments = DarkSphereSegments;
			int vertCount = (latSegments + 1) * (lonSegments + 1);
			darkSphereVertices = new VertexPositionColor[vertCount];
			int indexCount = latSegments * lonSegments * 6;
			darkSphereIndices = new short[indexCount];

			float radius = SphereRadius * DarkSphereRadiusMult;

			int vi = 0;
			for (int lat = 0; lat <= latSegments; lat++) {
				float theta = lat * MathHelper.PiOver2 / latSegments;
				float sinTheta = (float)Math.Sin(theta);
				float cosTheta = (float)Math.Cos(theta);
				for (int lon = 0; lon <= lonSegments; lon++) {
					float phi = lon * MathHelper.TwoPi / lonSegments;
					float x = (float)Math.Cos(phi) * sinTheta * radius;
					float y = cosTheta * radius;
					float z = (float)Math.Sin(phi) * sinTheta * radius;
					float heightFactor = y / radius;
					Color color = Color.Lerp(DarkSphereBottomColor, DarkSphereTopColor, heightFactor);
					darkSphereVertices[vi++] = new VertexPositionColor(new Vector3(x, y, z), color);
				}
			}

			int ii = 0;
			for (int lat = 0; lat < latSegments; lat++) {
				for (int lon = 0; lon < lonSegments; lon++) {
					short tl = (short)(lat * (lonSegments + 1) + lon);
					short tr = (short)(tl + 1);
					short bl = (short)((lat + 1) * (lonSegments + 1) + lon);
					short br = (short)(bl + 1);
					darkSphereIndices[ii++] = tl;
					darkSphereIndices[ii++] = bl;
					darkSphereIndices[ii++] = tr;
					darkSphereIndices[ii++] = tr;
					darkSphereIndices[ii++] = bl;
					darkSphereIndices[ii++] = br;
				}
			}
		}

		public void Draw(Vector2 center, float explodeTimer) {
			GraphicsDevice device = Main.graphics.GraphicsDevice;
			EnsureEffects(device);

			float t = explodeTimer / (float)EffectDuration;
			float scale = MathHelper.Lerp(SphereScaleStart, SphereScaleEnd, (float)Math.Pow(t, SphereScalePower));

			float sphereAlpha;
			if (t < SphereFadeInTime)
				sphereAlpha = t / SphereFadeInTime;
			else {
				float fade = (t - SphereFadeInTime) / (SphereFadeOutEnd - SphereFadeInTime);
				sphereAlpha = Math.Max(0, 1f - fade * fade * SphereFadeOutMultiplier);
			}

			float darkSphereAlpha;
			if (t < DarkSphereFadeInTime)
				darkSphereAlpha = t / DarkSphereFadeInTime;
			else {
				float fade = (t - DarkSphereFadeInTime) / (DarkSphereFadeOutEnd - DarkSphereFadeInTime);
				darkSphereAlpha = Math.Max(0, 1f - fade * fade * DarkSphereFadeOutMultiplier);
			}

			float ribbonAlpha = (float)Math.Pow(1f - t, RibbonAlphaPower);
			float blackRibbonAlpha = (float)Math.Pow(1f - t, BlackRibbonAlphaPower);
			float midRibbonAlpha = (float)Math.Pow(1f - t, MidRibbonAlphaPower);

			float zoom = Main.GameViewMatrix.Zoom.X;
			Matrix transform = Main.GameViewMatrix.TransformationMatrix;
			Vector2 screenCenter = Vector2.Transform(center - Main.screenPosition, transform);
			screenCenter.Y += 30f * zoom;

			float totalScale = scale * zoom;

			BlendState origBlend = device.BlendState;
			DepthStencilState origDepth = device.DepthStencilState;
			RasterizerState origRaster = device.RasterizerState;

			try {
				device.BlendState = BlendState.Additive;
				device.DepthStencilState = DepthStencilState.None;
				device.RasterizerState = RasterizerState.CullNone;

				DrawSphere(device, screenCenter, totalScale, sphereAlpha);
				DrawRibbonList<Ribbon>(device, screenCenter, totalScale, ribbonAlpha, ribbons);
				DrawRibbonList<MidRibbon>(device, screenCenter, totalScale, midRibbonAlpha, midRibbons);
				DrawDarkSphere(device, screenCenter, totalScale, darkSphereAlpha);

				device.BlendState = BlendState.AlphaBlend;
				DrawBlackRibbonsWithTexture(device, screenCenter, totalScale, blackRibbonAlpha);
			}
			finally {
				device.BlendState = origBlend;
				device.DepthStencilState = origDepth;
				device.RasterizerState = origRaster;
			}
		}

		private void EnsureEffects(GraphicsDevice device) {
			if (colorEffect == null || colorEffect.IsDisposed)
				colorEffect = new BasicEffect(device) { VertexColorEnabled = true };
			if (textureEffect == null || textureEffect.IsDisposed)
				textureEffect = new BasicEffect(device) { VertexColorEnabled = true, TextureEnabled = true };

			colorEffect.World = Matrix.Identity;
			colorEffect.View = Matrix.Identity;
			colorEffect.Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			textureEffect.World = Matrix.Identity;
			textureEffect.View = Matrix.Identity;
			textureEffect.Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
		}

		private void DrawDarkSphere(GraphicsDevice device, Vector2 center, float scale, float alpha) {
			if (darkSphereVertices == null || darkSphereIndices == null)
				return;

			var transformed = new VertexPositionColor[darkSphereVertices.Length];
			float cosA = (float)Math.Cos(explodeAngle);
			float sinA = (float)Math.Sin(explodeAngle);

			for (int i = 0; i < darkSphereVertices.Length; i++) {
				Vector3 pos = darkSphereVertices[i].Position * scale;
				float x = pos.X * cosA - pos.Z * sinA;
				float z = pos.X * sinA + pos.Z * cosA;
				pos = new Vector3(x, pos.Y, z);

				float sx = pos.X + center.X;
				float sy = center.Y - pos.Y * RibbonYScale + pos.Z * RibbonZFactor;
				transformed[i] = new VertexPositionColor(new Vector3(sx, sy, 0), darkSphereVertices[i].Color * alpha);
			}

			colorEffect.CurrentTechnique.Passes[0].Apply();
			device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
				transformed, 0, transformed.Length, darkSphereIndices, 0, darkSphereIndices.Length / 3);
		}

		private void DrawSphere(GraphicsDevice device, Vector2 center, float scale, float alpha) {
			if (sphereVertices == null || sphereIndices == null)
				return;

			var transformed = new VertexPositionColor[sphereVertices.Length];
			float cosA = (float)Math.Cos(explodeAngle);
			float sinA = (float)Math.Sin(explodeAngle);

			for (int i = 0; i < sphereVertices.Length; i++) {
				Vector3 pos = sphereVertices[i].Position * scale;
				float x = pos.X * cosA - pos.Z * sinA;
				float z = pos.X * sinA + pos.Z * cosA;
				pos = new Vector3(x, pos.Y, z);

				float sx = pos.X + center.X;
				float sy = center.Y - pos.Y * RibbonYScale + pos.Z * RibbonZFactor;
				transformed[i] = new VertexPositionColor(new Vector3(sx, sy, 0), sphereVertices[i].Color * alpha);
			}

			colorEffect.CurrentTechnique.Passes[0].Apply();
			device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
				transformed, 0, transformed.Length, sphereIndices, 0, sphereIndices.Length / 3);
		}

		private void DrawRibbonList<T>(GraphicsDevice device, Vector2 center, float scale, float alpha, List<T> list) where T : BaseRibbon {
			if (list == null)
				return;

			float cosA = (float)Math.Cos(explodeAngle);
			float sinA = (float)Math.Sin(explodeAngle);
			float baseRotationAngle = (0.5f) * MathHelper.TwoPi * 1f;

			foreach (var ribbon in list) {
				if (ribbon.Points.Count < 2)
					continue;

				float rotationAngle = baseRotationAngle * ribbon.RotationSpeed;
				float cosR = (float)Math.Cos(rotationAngle);
				float sinR = (float)Math.Sin(rotationAngle);

				var verts = new List<VertexPositionColor>();
				var inds = new List<short>();

				Color baseCol = ribbon.GetBaseColor();
				float yScale = ribbon.GetYScale();
				float zFactor = ribbon.GetZFactor();

				for (int i = 0; i < ribbon.Points.Count; i++) {
					Vector3 pos = ribbon.Points[i] * scale;

					float rx = pos.X * cosR - pos.Z * sinR;
					float rz = pos.X * sinR + pos.Z * cosR;
					Vector3 rotPos = new Vector3(rx, pos.Y, rz);

					float x = rotPos.X * cosA - rotPos.Z * sinA;
					float z = rotPos.X * sinA + rotPos.Z * cosA;

					Vector2 scrPos = new Vector2(center.X + x, center.Y - rotPos.Y * yScale + z * zFactor);

					Vector3 nextPos = (i < ribbon.Points.Count - 1) ? ribbon.Points[i + 1] * scale : pos;
					float nrx = nextPos.X * cosR - nextPos.Z * sinR;
					float nrz = nextPos.X * sinR + nextPos.Z * cosR;
					float n2x = nrx * cosA - nrz * sinA;
					float n2z = nrx * sinA + nrz * cosA;
					Vector2 nextScr = new Vector2(center.X + n2x, center.Y - nextPos.Y * yScale + n2z * zFactor);

					Vector2 tangent = nextScr - scrPos;
					if (tangent.Length() < 0.01f)
						tangent = Vector2.UnitX;
					tangent.Normalize();
					Vector2 normal = new Vector2(-tangent.Y, tangent.X);

					float u = i / (float)(ribbon.Points.Count - 1);
					float widthScale = (float)Math.Pow(Math.Sin(u * MathHelper.Pi), 1.6f);
					float halfW = ribbon.Width * widthScale * 0.5f;

					Color vertColor = baseCol * alpha;

					verts.Add(new VertexPositionColor(new Vector3(scrPos - normal * halfW, 0), vertColor));
					verts.Add(new VertexPositionColor(new Vector3(scrPos + normal * halfW, 0), vertColor));
				}

				for (int i = 0; i < ribbon.Points.Count - 1; i++) {
					short i0 = (short)(i * 2), i1 = (short)(i * 2 + 1);
					short i2 = (short)((i + 1) * 2), i3 = (short)((i + 1) * 2 + 1);
					inds.Add(i0);
					inds.Add(i1);
					inds.Add(i2);
					inds.Add(i2);
					inds.Add(i1);
					inds.Add(i3);
				}

				if (verts.Count > 0 && inds.Count > 0) {
					colorEffect.CurrentTechnique.Passes[0].Apply();
					device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
						verts.ToArray(), 0, verts.Count, inds.ToArray(), 0, inds.Count / 3);
				}
			}
		}

		private void DrawBlackRibbonsWithTexture(GraphicsDevice device, Vector2 center, float scale, float blackRibbonAlpha) {
			if (blackRibbons == null || gradientTexture == null || gradientTexture.IsDisposed)
				return;

			float cosA = (float)Math.Cos(explodeAngle);
			float sinA = (float)Math.Sin(explodeAngle);
			float baseRotationAngle = (0.5f) * MathHelper.TwoPi * 1f;

			textureEffect.Texture = gradientTexture;

			foreach (var ribbon in blackRibbons) {
				if (ribbon.Points.Count < 2)
					continue;

				float rotationAngle = baseRotationAngle * ribbon.RotationSpeed;
				float cosR = (float)Math.Cos(rotationAngle);
				float sinR = (float)Math.Sin(rotationAngle);

				float[] cumLength = new float[ribbon.Points.Count];
				float totalLength = 0f;
				for (int i = 1; i < ribbon.Points.Count; i++) {
					cumLength[i] = cumLength[i - 1] + Vector3.Distance(ribbon.Points[i], ribbon.Points[i - 1]) * scale;
					totalLength = cumLength[i];
				}

				float timeOffset = (float)Main.timeForVisualEffects * 0.02f * 2f;
				timeOffset -= (float)Math.Floor(timeOffset);

				var verts = new List<VertexPositionColorTexture>();
				var inds = new List<short>();

				float yScale = BlackRibbonYScale;
				float zFactor = BlackRibbonZFactor;

				for (int i = 0; i < ribbon.Points.Count; i++) {
					Vector3 pos = ribbon.Points[i] * scale;

					float rx = pos.X * cosR - pos.Z * sinR;
					float rz = pos.X * sinR + pos.Z * cosR;
					Vector3 rotPos = new Vector3(rx, pos.Y, rz);

					float x = rotPos.X * cosA - rotPos.Z * sinA;
					float z = rotPos.X * sinA + rotPos.Z * cosA;

					Vector2 scrPos = new Vector2(center.X + x, center.Y - rotPos.Y * yScale + z * zFactor);

					Vector3 nextPos = (i < ribbon.Points.Count - 1) ? ribbon.Points[i + 1] * scale : pos;
					float nrx = nextPos.X * cosR - nextPos.Z * sinR;
					float nrz = nextPos.X * sinR + nextPos.Z * cosR;
					float n2x = nrx * cosA - nrz * sinA;
					float n2z = nrx * sinA + nrz * cosA;
					Vector2 nextScr = new Vector2(center.X + n2x, center.Y - nextPos.Y * yScale + n2z * zFactor);

					Vector2 tangent = nextScr - scrPos;
					if (tangent.Length() < 0.01f)
						tangent = Vector2.UnitX;
					tangent.Normalize();
					Vector2 normal = new Vector2(-tangent.Y, tangent.X);

					float u = i / (float)(ribbon.Points.Count - 1);
					float widthScale = (float)Math.Pow(Math.Sin(u * MathHelper.Pi), 1.6f);
					float halfW = ribbon.Width * widthScale * 0.5f;

					float v = totalLength > 0 ? cumLength[i] / totalLength : 0f;
					v = (v + timeOffset) % 1f;

					Color vertColor = Color.Lerp(BlackRibbonEdgeColor, BlackRibbonBaseColor, u) * blackRibbonAlpha;

					verts.Add(new VertexPositionColorTexture(
						new Vector3(scrPos - normal * halfW, 0), vertColor, new Vector2(0, v)));
					verts.Add(new VertexPositionColorTexture(
						new Vector3(scrPos + normal * halfW, 0), vertColor, new Vector2(1, v)));
				}

				for (int i = 0; i < ribbon.Points.Count - 1; i++) {
					short i0 = (short)(i * 2), i1 = (short)(i * 2 + 1);
					short i2 = (short)((i + 1) * 2), i3 = (short)((i + 1) * 2 + 1);
					inds.Add(i0);
					inds.Add(i1);
					inds.Add(i2);
					inds.Add(i2);
					inds.Add(i1);
					inds.Add(i3);
				}

				if (verts.Count > 0 && inds.Count > 0) {
					textureEffect.CurrentTechnique.Passes[0].Apply();
					device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
						verts.ToArray(), 0, verts.Count, inds.ToArray(), 0, inds.Count / 3);
				}
			}
		}
	}
}