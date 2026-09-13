using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace ArknightsMod.Common.Particle
{
	public class SparkParticle : Particle
	{
		public override BlendState DrawBlendState => BlendState.Additive;
        public override string TexturePath => "ArknightsMod/Common/Particle/DefaultParticle";
		public Vector2 Deformation = new Vector2(0.5f, 1.6f);
        public SparkParticle(Vector2 relativePosition, Vector2 velocity, int lifetime, float scale, Color color, bool noGravity)
		{
			this.Position = relativePosition;
			this.Velocity = velocity;
			this.NoGravity = noGravity;
			this.Scale = scale;
			this.Lifetime = lifetime;
			this.InitialColor = color;
			this.Color = color;
        }

		public override void Update()
		{
			this.Scale *= 1f;
			float speed = fadeIn ? 8f : 12f;
            this.Color = Color.Lerp(this.InitialColor, Color.Transparent, MathF.Pow(base.LifetimeRatio * speed, 4f));
            this.Velocity *= 0.95f;
			if (this.Velocity.Length() < 12f && !NoGravity)
			{
				this.Velocity.X = this.Velocity.X * 0.94f;
				this.Velocity.Y = this.Velocity.Y + 0.25f;
			}
			if (this.Color.A >= 255)
			{
				Kill();
			}
			this.Rotation = Utils.ToRotation(this.Velocity) + 1.5707964f;
		}
		public override void Draw()
		{
			Vector2 scale = Deformation * this.Scale;
			Main.spriteBatch.Draw(base.Texture, this.Position - Main.screenPosition, null, this.Color, this.Rotation, Utils.Size(base.Texture) * 0.5f, scale, 0, 0f);
			Main.spriteBatch.Draw(base.Texture, this.Position - Main.screenPosition, null, this.Color, this.Rotation, Utils.Size(base.Texture) * 0.5f, scale * new Vector2(0.45f, 1f), 0, 0f);
		}
		public Color InitialColor;
		public bool fadeIn;
		public bool NoGravity;
	}
}
