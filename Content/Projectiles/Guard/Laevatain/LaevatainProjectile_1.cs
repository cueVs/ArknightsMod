using System;
using System.Collections.Generic;
using ArknightsMod.Common.VisualEffects;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Laevatain
{
	public class LaevatainProjectile_1 : ModProjectile
	{
		public override string Texture =>
			"ArknightsMod/Content/Items/Weapons/Guard/Surtr/SurtrLaevatain";

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 50;
			ProjectileID.Sets.TrailingMode[Type] = 2;
		}

		//用于记录是fire的第几个切片
		private int t = 0;

		public override void SetDefaults()
		{
			Projectile.width = 192;
			Projectile.height = 210;

			Projectile.friendly = true;
			Projectile.hostile = false;

			Projectile.penetrate = -1;

			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 9999;

			Projectile.DamageType = DamageClass.Melee;

			Projectile.ownerHitCheck = true;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.extraUpdates = 2;
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];

			if (!player.active || player.dead)
			{
				Projectile.Kill();
				return;
			}
			if (Projectile.ai[1] == 0)
			{
				Projectile.ai[1] = 1;
				Projectile.ai[0] = player.direction;
				Projectile.timeLeft = player.itemAnimationMax * (1 + Projectile.extraUpdates);
				Projectile.localAI[0] = Projectile.timeLeft;

				Projectile.netUpdate = true;
			}

			int dir = (int)Projectile.ai[0];
			float len = Projectile.Size.Length();
			player.direction = dir;
			player.heldProj = Projectile.whoAmI;
			float progress = 1f - Projectile.timeLeft / Projectile.localAI[0];
			float startRot = MathHelper.PiOver2;
			float endRot = dir == 1 ? -MathHelper.PiOver2 * 3 : MathHelper.PiOver2 * 5;
			float middleRot = dir == 1 ? MathHelper.Pi * 2 / 3 : MathHelper.Pi / 3;
			float rot;
			if (progress < 0.35f)
			{
				rot = MathHelper.Lerp(startRot, middleRot, progress / 0.35f);
			}
			else
			{
				rot = MathHelper.Lerp(middleRot, endRot, (progress - 0.35f) / 0.65f);
			}

			Vector2 offset = rot.ToRotationVector2() * len / 4;

			Projectile.Center = player.MountedCenter + offset;
			Projectile.rotation = rot;
			player.itemRotation = rot;
			if (Main.rand.NextBool(2))
			{
				for (int i = 0; i < 3; i++)
				{
					Vector2 trailPos =
						Projectile.Center
						+ player.itemRotation.ToRotationVector2()
							* Main.rand.NextFloat(len * 0.6f, len)
							/ 2;
					Color dustColor;
					float rand = Main.rand.NextFloat();
					if (rand < 0.15f)
					{
						dustColor = Color.Cyan;
					}
					else if (rand < 0.30f)
					{
						dustColor = Color.White;
					}
					else if (rand < 0.65f)
					{
						dustColor = Color.Red;
					}
					else
					{
						dustColor = Color.Yellow;
					}
					Dust d = Dust.NewDustPerfect(
						trailPos,
						DustID.Torch,
						(
							player.itemRotation + Main.rand.NextFloat(0, MathHelper.PiOver4) * dir
						).ToRotationVector2() * 20f,
						150,
						dustColor,
						Main.rand.NextFloat(2f, 5f)
					);

					d.noGravity = true;
				}
			}

			t += 1;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Player player = Main.player[Projectile.owner];

			//剑本体
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			Vector2 origin = new Vector2(
				tex.Width / 4,
				tex.Height / 2 - tex.Height * Projectile.ai[0] / 4
			);

			SpriteEffects effects =
				Projectile.ai[0] == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

			Main.spriteBatch.Draw(
				tex,
				player.MountedCenter
					+ player.itemRotation.ToRotationVector2() * 0f
					- Main.screenPosition,
				null,
				Color.White,
				Projectile.rotation - MathHelper.PiOver2 * Projectile.ai[0],
				origin,
				Projectile.scale,
				effects,
				0f
			);
			//放大的剑
			tex = ModContent
				.Request<Texture2D>(
					"ArknightsMod/Content/Projectiles/Guard/Laevatain/LaevatainProjectile_1_swordShadow"
				)
				.Value;
			origin = new Vector2(tex.Width / 2, tex.Height / 2);
			effects = Projectile.ai[0] == -1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
			Vector2 offset =
				Projectile.Size.RotatedBy(
					Projectile.rotation - (Projectile.ai[0] == 1 ? MathHelper.PiOver2 : 0)
				)
				* 2
				/ 4;

			// Main.spriteBatch.Draw(
			// 	tex,
			// 	player.MountedCenter + offset - Main.screenPosition,
			// 	null,
			// 	Color.White,
			// 	Projectile.rotation - MathHelper.PiOver2 * Projectile.ai[0],
			// 	origin,
			// 	Projectile.scale * 5f,
			// 	effects,
			// 	0f
			// );

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.NonPremultiplied,
				Main.DefaultSamplerState,
				DepthStencilState.None,
				RasterizerState.CullNone,
				null,
				Main.GameViewMatrix.TransformationMatrix
			);

			Main.spriteBatch.Draw(
				tex,
				player.MountedCenter + offset - Main.screenPosition,
				null,
				Color.White,
				Projectile.rotation - MathHelper.PiOver2 * Projectile.ai[0],
				origin,
				Projectile.scale * 6f,
				effects,
				0f
			);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.AlphaBlend,
				Main.DefaultSamplerState,
				DepthStencilState.None,
				RasterizerState.CullNone,
				null,
				Main.GameViewMatrix.TransformationMatrix
			);
			// Main.spriteBatch.Draw(tex, player.MountedCenter + offset - Main.screenPosition, src, drawColor,
			//   Projectile.rotation - MathHelper.PiOver2 * Projectile.ai[0],
			//   origin, Projectile.scale * 3f, effects, 0f);
			//火光
			float rangeFix = Projectile.Size.Length() * Projectile.scale;
			List<Vertex> vertices = [];
			int length = ProjectileID.Sets.TrailCacheLength[Type];
			// 前3帧不用，否则覆盖到剑了
			for (int i = 3; i < length; i++)
			{
				Color up_coordColor = Main.hslToRgb(0.03f, 1f - i / 15f, 0.5f) * 0.7f;
				Color bottom_coordColor = Main.hslToRgb(0.5f, 1f - i / 15f, 0.5f) * 0.7f;
				Vector2 pos = player.MountedCenter;
				// Projectile.Center
				// + new Vector2(
				// 	-1 * Projectile.width,
				// 	Projectile.height * Projectile.ai[0]
				// ).RotatedBy(Projectile.rotation) / 4;

				if (Projectile.oldPos[i] == Vector2.Zero)
					continue;
				if (Projectile.ai[0] == 1)
				{
					vertices.Add(
						new Vertex(
							pos
								- Main.screenPosition
								+ rangeFix
									* (
										Projectile.oldRot[i] - (float)Math.PI / 4
									).ToRotationVector2()
									* (0.9f - (float)(0.0 * i / length)),
							new Vector3((t - i) / (float)length, 0.8f, 1),
							up_coordColor
						)
					); //上底
					vertices.Add(
						new Vertex(
							pos
								- Main.screenPosition
								+ rangeFix
									* (
										Projectile.oldRot[i] - (float)Math.PI / 4
									).ToRotationVector2()
									* (0.3f + (float)(0.59 * i / length)),
							new Vector3((t - i) / (float)length, 0.3f, 1),
							bottom_coordColor
						)
					); //下底
				}
				else
				{
					vertices.Add(
						new Vertex(
							pos
								- Main.screenPosition
								- rangeFix
									* (
										Projectile.oldRot[i] - (float)Math.PI * 3 / 4
									).ToRotationVector2()
									* (0.9f - (float)(0.0 * i / length)),
							new Vector3((t - i) / (float)length, 0.8f, 1),
							up_coordColor
						)
					); //上底
					vertices.Add(
						new Vertex(
							pos
								- Main.screenPosition
								- rangeFix
									* (
										Projectile.oldRot[i] - (float)Math.PI * 3 / 4
									).ToRotationVector2()
									* (0.3f + (float)(0.59 * i / length)),
							new Vector3((t - i) / (float)length, 0.3f, 1),
							bottom_coordColor
						)
					); //下底
				}
			}
			SpriteBatch spriteBatch = Main.spriteBatch;
			spriteBatch.End();
			spriteBatch.Begin(
				SpriteSortMode.Immediate,
				BlendState.Additive,
				SamplerState.LinearWrap,
				DepthStencilState.None,
				RasterizerState.CullNone,
				null,
				Main.GameViewMatrix.TransformationMatrix
			);
			Main.graphics.GraphicsDevice.Textures[0] = ModContent
				.Request<Texture2D>(
					"ArknightsMod/Content/Projectiles/Guard/Laevatain/LaevatainProjectile_1_fire"
				)
				.Value;
			Main.graphics.GraphicsDevice.DrawUserPrimitives(
				PrimitiveType.TriangleStrip,
				vertices.ToArray(),
				0,
				vertices.Count - 2
			);
			spriteBatch.End();
			spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.AlphaBlend,
				Main.DefaultSamplerState,
				DepthStencilState.None,
				RasterizerState.CullNone,
				null,
				Main.GameViewMatrix.TransformationMatrix
			);
			return false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Main.myPlayer != Projectile.owner)
				return;
			Player owner = Main.player[Projectile.owner];
			var mp = owner.GetModPlayer<WeaponPlayer>();

			if (target.life <= 0 && mp.CurrentSkill != null)
			{
				mp.StockCount = 1;
				mp.SP = 0;
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Player player = Main.player[Projectile.owner];

			float length = Projectile.Size.Length();

			Vector2 start = player.MountedCenter;

			Vector2 end = start + Projectile.rotation.ToRotationVector2() * length;

			float width = 30f;
			float collisionPoint = 0f;

			return Collision.CheckAABBvLineCollision(
				targetHitbox.TopLeft(),
				targetHitbox.Size(),
				start,
				end,
				width,
				ref collisionPoint
			);
		}
	}
}
