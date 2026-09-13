using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP2;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP1;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.EffectImage;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.NormalAttack;

namespace ArknightsMod.Content.Projectiles.Supporter.Tragodia.EffectImage
{
	public class LightParticle : ModProjectile
	{
		private static Texture2D _circleTexture;

		public int TotalFrames = 35;
		public float StartSize = 25f;
		public float EndSize = 0.5f;
		public float VelocityMultiplier = 0.5f;
		public Vector2 VelocityRandomRange = new Vector2(0.3f, 0.6f);
		public float VelocityDamping = 0.95f;
		public bool FaceMovementDirection = true;
		public Vector2 RotationSpeedRange = new Vector2(-0.08f, 0.08f);
		public Vector2 DeformationXRange = new Vector2(0.3f, 0.5f);
		public Vector2 DeformationYRange = new Vector2(2.5f, 4.0f);
		public float SizeMultiplier = 1.5f;
		public float OpacityPower = 0.3f;
		public float SpeedOpacityInfluence = 0.2f;
		public float MaxSpeedForOpacity = 8f;
		public Color ParticleColor = new Color(200, 150, 255);
		public bool UseAdditiveBlending = true;
		public int TextureSize = 32;
		public float TextureSoftness = 0.6f;

		private int totalFrames;
		private float startSize;
		private float endSize;
		private Vector2 velocity;
		private float rotation;
		private float rotationSpeed;
		private float deformationX;
		private float deformationY;
		private bool initialized = false;
		private Color currentColor;

		public override void SetDefaults() {
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.aiStyle = -1;
		}

		public void Initialize(int totalFrames, float startSize, float endSize,
							  Vector2 velocity, Vector2 position, Color color) {
			this.totalFrames = totalFrames;
			this.startSize = startSize * SizeMultiplier;
			this.endSize = endSize * SizeMultiplier;
			this.currentColor = color;

			this.velocity = velocity * VelocityMultiplier * Main.rand.NextFloat(VelocityRandomRange.X, VelocityRandomRange.Y);
			this.rotationSpeed = Main.rand.NextFloat(RotationSpeedRange.X, RotationSpeedRange.Y);
			this.deformationX = Main.rand.NextFloat(DeformationXRange.X, DeformationXRange.Y);
			this.deformationY = Main.rand.NextFloat(DeformationYRange.X, DeformationYRange.Y);

			Projectile.Center = position;
			Projectile.timeLeft = this.totalFrames;
			initialized = true;
		}

		public override void AI() {
			if (!initialized)
				return;
			velocity *= VelocityDamping;
			Projectile.Center += velocity;
			rotation += rotationSpeed;
			if (FaceMovementDirection && velocity.Length() > 0.1f)
				rotation = velocity.ToRotation() + MathHelper.PiOver2;
		}

		private void EnsureTexture() {
			if (_circleTexture != null && _circleTexture.Width == TextureSize)
				return;
			_circleTexture = new Texture2D(Main.instance.GraphicsDevice, TextureSize, TextureSize);
			Color[] data = new Color[TextureSize * TextureSize];
			Vector2 center = new Vector2(TextureSize / 2f);
			float radius = TextureSize / 2f;
			for (int y = 0; y < TextureSize; y++)
				for (int x = 0; x < TextureSize; x++) {
					float dist = Vector2.Distance(new Vector2(x, y), center);
					if (dist <= radius) {
						float alpha = 1f - dist / radius;
						alpha = (float)Math.Pow(alpha, TextureSoftness);
						data[y * TextureSize + x] = Color.White * alpha;
					}
					else
						data[y * TextureSize + x] = Color.Transparent;
				}
			_circleTexture.SetData(data);
		}

		public override bool PreDraw(ref Color lightColor) {
			if (!initialized)
				return false;
			EnsureTexture();

			int currentFrame = totalFrames - Projectile.timeLeft;
			float progress = currentFrame / (float)totalFrames;
			float currentSize = MathHelper.Lerp(startSize, endSize, progress);
			float opacity = (float)Math.Pow(1f - progress, OpacityPower);
			float speedFactor = 1f - MathHelper.Clamp(velocity.Length() / MaxSpeedForOpacity, 0f, SpeedOpacityInfluence);
			opacity *= speedFactor;

			Vector2 drawPos = Projectile.Center - Main.screenPosition;

			
			float lineWidth = currentSize * deformationX * 0.5f;
			float lineLength = currentSize * deformationY;

			Vector2 scale = new Vector2(lineWidth / _circleTexture.Width,
										 lineLength / _circleTexture.Height);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
				Main.DefaultSamplerState, DepthStencilState.Default, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);

		
			float drawRotation = velocity.Length() > 0.1f ? velocity.ToRotation() + MathHelper.PiOver2 : rotation;

			//»­Á½´Î
			Main.spriteBatch.Draw(_circleTexture, drawPos, null, currentColor * opacity,
				drawRotation, _circleTexture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
	
			Main.spriteBatch.Draw(_circleTexture, drawPos, null, currentColor * opacity * 0.4f,
				drawRotation, _circleTexture.Size() * 0.5f, scale * 1.5f, SpriteEffects.None, 0f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.Default, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);
			return false;
		}
	}
}