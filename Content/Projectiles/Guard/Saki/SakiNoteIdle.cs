using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using ArknightsMod.Common;

namespace ArknightsMod.Content.Projectiles.Guard.Saki
{
	/// <summary>
	/// 待机状态音符
	/// </summary>
    public class SakiNoteIdle : ModProjectile
    {
		public override string Texture => ArknightsMod.noTexture;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 10;
		}
		public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.scale = 1f;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
            Projectile.aiStyle = -1;
			AIType = -1;
        }
		/// <summary>
		/// 是否碰撞物块/命中敌怪，碰撞后即渐隐消失
		/// </summary>
		public bool collided;

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[Projectile.owner];

            //使击退始终远离玩家
            float hitDir = target.position.X - player.position.X;
            if (hitDir < 0) { modifiers.HitDirectionOverride = -1; }
            else { modifiers.HitDirectionOverride = 1; }

            /*Projectile.NewProjectile
                (new EntitySource_Parent(Projectile), target.Center, Vector2.Zero,
                ModContent.ProjectileType<SakiSlashHit>(), 0, 0, Projectile.owner);*/

			collided = true;
		}
		public override bool? CanDamage() => !collided;

		public override bool OnTileCollide(Vector2 oldVelocity) {
			collided = true;
			return false;
		}
		public override void OnKill(int timeLeft)
		{
			/*Projectile.NewProjectile
				(new EntitySource_Parent(Projectile), target.Center, Vector2.Zero,
				ModContent.ProjectileType<SakiSlashHit>(), 0, 0, Projectile.owner);*/
		}

		private Vector2 perturbation = Vector2.Zero;
		private Vector2 targetPerturbation = Vector2.Zero;
		private int perturbTimer = 0;
		private int perturbUpdateInterval = 10; //每10帧更新一次
		private int _lastPerturbationDirection = 1;

		public float[] oldrot = new float[40];
		public Vector2[] oldpos = new Vector2[40];
		public override void AI()
        {
			for (int i = oldrot.Length - 1; i > 0; i--) {
				oldrot[i] = oldrot[i - 1];
				if (Math.Abs(Projectile.velocity.Length()) > 0.075f) {
					oldpos[i] = oldpos[i - 1];
				}
			}

			oldrot[0] = Projectile.rotation;
			oldpos[0] = Projectile.Center;


			//碰撞缓慢消逝
			if (collided) {
				Projectile.alpha += 10;
				if (Projectile.alpha > 255) {
					Projectile.Kill();
				}
			}

			//追踪
			float centerX = Projectile.Center.X;
			float centerY = Projectile.Center.Y;
			float num3 = 800f;
			bool flag = false;
			for (int i = 0; i < 200; i++) {
				if (Main.npc[i].CanBeChasedBy(Projectile, false) && Collision.CanHit(Projectile.Center, 1, 1, Main.npc[i].Center, 1, 1)) {
					float num4 = Main.npc[i].position.X + (float)(Main.npc[i].width / 2);
					float num5 = Main.npc[i].position.Y + (float)(Main.npc[i].height / 2);
					float num6 = Math.Abs(Projectile.position.X + (float)(Projectile.width / 2) - num4) + Math.Abs(Projectile.position.Y + (float)(Projectile.height / 2) - num5);
					if (num6 < num3) {
						num3 = num6;
						centerX = num4;
						centerY = num5;
						flag = true;
					}
				}
			}
			if (flag)
			{
				float speed = 3;
				Vector2 vector = new Vector2(Projectile.position.X + Projectile.width * 0.5f, Projectile.position.Y + Projectile.height * 0.5f);
				float velX = centerX - vector.X;
				float velY = centerY - vector.Y;
				float spd = (float)Math.Sqrt((double)(velX * velX + velY * velY));
				spd = speed / spd;
				velX *= spd;
				velY *= spd;
				Projectile.velocity.X = (Projectile.velocity.X * 20f + velX) / 21f;
				Projectile.velocity.Y = (Projectile.velocity.Y * 20f + velY) / 21f;
				return;
			}

			//随机扰动
			Vector2 velocity = Projectile.velocity;
			float projSpeed = velocity.Length();

			if (projSpeed > 0f) {
				Vector2 dir = Vector2.Normalize(velocity);

				//计算垂直向量（两个方向）
				Vector2 perpendicular = new Vector2(-dir.Y, dir.X);

				//更新扰动方向
				perturbTimer++;
				if (perturbTimer >= perturbUpdateInterval) {
					perturbTimer = 0;
					float strength = Main.rand.NextFloat(-0.1f, 0.1f);
					targetPerturbation = perpendicular * strength;
				}
				perturbation = targetPerturbation;
				Projectile.velocity = velocity + perturbation;
				Projectile.velocity = Vector2.Normalize(Projectile.velocity) * projSpeed;
			}
		}
		public override bool PreDraw(ref Color lightColor)
        {
			Texture2D tex = ModContent.Request<Texture2D>($"ArknightsMod/Content/Projectiles/Guard/Saki/Note2").Value;
			Texture2D glow = ModContent.Request<Texture2D>($"ArknightsMod/Content/Projectiles/Guard/Saki/Assets/ray_130").Value;
			Color color = new Color(54, 53, 143) * 2f;

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color * Projectile.Opacity,
				0f, tex.Size() / 2, Projectile.scale * 0.7f, SpriteEffects.None, 0f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.Default,
				Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);

			Main.spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, null, Color.White * Projectile.Opacity,
				0f, glow.Size() / 2, Projectile.scale * 0.15f, SpriteEffects.None, 0f);

			float alpha = (float)EaseFunction.SineEase(Projectile.timeLeft, 0.4f, 0.8f, 4);
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, alpha) * Projectile.Opacity,
				0f, tex.Size() / 2, Projectile.scale * 0.7f, SpriteEffects.None, 0f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.Default,
				Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);

			for (int i = 0; i < 2; i++)
			{
				DrawStar(Projectile.Center, Main.rand.NextFloat(7, 10));
			}

			return false;
        }
		private Vector2 _cachedPositionOffset;
		private float _cachedScale;
		private int _updateTimer;
		public void DrawStar(Vector2 center, float radius)
		{
			if (++_updateTimer >= 2)
			{
				_updateTimer = 0;

				// 更新随机数
				_cachedPositionOffset = new Vector2(radius).RotatedByRandom(MathHelper.Pi);
				_cachedScale = Main.rand.NextFloat(0.05f, 0.3f);
			}
			Texture2D star = ModContent.Request<Texture2D>($"ArknightsMod/Content/Projectiles/Guard/Saki/Assets/star_234_2").Value;
			Vector2 position = center + _cachedPositionOffset;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.Default,
				Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);

			float alpha = (float)EaseFunction.SineEase(Projectile.timeLeft, 0.2f, 0.4f, 4);
			Main.spriteBatch.Draw(star, position - Main.screenPosition, null, new Color(255, 255, 255) * Projectile.Opacity,
				0f, star.Size() / 2, Projectile.scale * _cachedScale, SpriteEffects.None, 0f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.Default,
				Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);
		}
	}
}

