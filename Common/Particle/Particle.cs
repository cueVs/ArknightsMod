using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace ArknightsMod.Common.Particle
{
	public abstract class Particle
	{
		public int ID { get; private set; }
		public Texture2D Texture { get; protected set; }

		public float LifetimeRatio
		{
			get
			{
				return (float)this.Time / (float)this.Lifetime;
			}
		}

		public abstract string TexturePath { get; }

		public virtual int FrameCount
		{
			get
			{
				return 1;
			}
		}

		public virtual BlendState DrawBlendState
		{
			get
			{
				return BlendState.AlphaBlend;
			}
		}

		public virtual void Load()
		{
		}

		public virtual void Update()
		{
		}

		public virtual void Draw()
		{
			Rectangle frame = Utils.Frame(this.Texture, 1, this.FrameCount, 0, this.Frame, 0, 0);
			SpriteEffects visualDirection = this.Direction != 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Main.spriteBatch.Draw(this.Texture, this.Position - Main.screenPosition, new Rectangle?(frame), this.Color * this.Opacity, this.Rotation, Utils.Size(frame) * 0.5f, this.Scale, visualDirection, 0f);
		}

		public void Spawn()
		{
			if (Main.netMode == 2)
			{
				return;
			}
			if (this.hasSpawned)
			{
				return;
			}
			ID = ParticleManager.particleIDLookup[GetType()];
			Texture = ParticleManager.particleTextureLookup[ID];
			ParticleManager.activeParticles.Add(this);
			this.hasSpawned = true;
		}

		public void Kill()
		{
			Time = Lifetime;
		}

		private bool hasSpawned;
		public int Frame;
		public int Time;
		public int Lifetime;
		public int Direction;
		public Vector2 Position;
		public Vector2 Velocity;
		public Color Color;
		public float Rotation;
		public float Scale = 1f;
		public float Opacity = 1f;
	}
}
