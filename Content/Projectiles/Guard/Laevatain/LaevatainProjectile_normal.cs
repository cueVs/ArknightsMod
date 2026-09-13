using System;
using System.Collections.Generic;
using ArknightsMod.Common.VisualEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Laevatain
{
	public class LaevatainProjectile_normal : ModProjectile
	{
		public override string Texture =>
			"ArknightsMod/Content/Items/Weapons/Guard/Surtr/SurtrLaevatain";

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 12;
			ProjectileID.Sets.TrailingMode[Type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 64;
			Projectile.height = 70;

			Projectile.friendly = true;
			Projectile.hostile = false;

			Projectile.penetrate = -1;

			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 9999;

			Projectile.DamageType = DamageClass.Melee;

			Projectile.ownerHitCheck = true;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
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
				Projectile.timeLeft = player.itemAnimationMax;
				Projectile.localAI[0] = Projectile.timeLeft;

				Projectile.netUpdate = true;
			}

			int dir = (int)Projectile.ai[0];

			player.direction = dir;
			player.heldProj = Projectile.whoAmI;
			float progress = 1f - Projectile.timeLeft / Projectile.localAI[0];
			float startRot = -1.8f;
			float endRot = dir == 1 ? 1.8f : -5.2f;

			float rot = MathHelper.Lerp(startRot, endRot, progress);
			Vector2 offset = rot.ToRotationVector2() * 30f;

			Projectile.Center = player.MountedCenter + offset;
			Projectile.rotation = rot + MathHelper.PiOver4 * dir;
			player.itemRotation = rot;
			float len;
			if (Main.rand.NextBool(2))
			{
				len = Projectile.Size.Length();
				Vector2 trailPos =
					Projectile.Center
					+ player.itemRotation.ToRotationVector2()
						* Main.rand.NextFloat(len * 0.8f, len)
						/ 2;

				Dust d = Dust.NewDustPerfect(
					trailPos,
					DustID.Torch,
					(player.itemRotation + MathHelper.PiOver4 * dir).ToRotationVector2() * 5f, //虽然这里值和rotation是一样的，但意义不一样，就这么写吧
					150,
					Color.OrangeRed,
					1.6f
				);

				d.noGravity = true;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			Vector2 origin = tex.Size() / 2f;

			SpriteEffects effects =
				Projectile.ai[0] == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

			Main.spriteBatch.Draw(
				tex,
				Projectile.Center - Main.screenPosition,
				null,
				Color.White,
				Projectile.rotation,
				origin,
				Projectile.scale,
				effects,
				0f
			);

			float rangeFix = Projectile.Size.Length() * Projectile.scale;
			List<Vertex> vertices = [];
			for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Type]; i++)
			{
				Color up_coordColor = Main.hslToRgb(0.03f, 1f - i / 15f, 0.5f) * 0.7f;
				Color bottom_coordColor = Main.hslToRgb(0.5f, 1f - i / 15f, 0.5f) * 0.7f;
				Vector2 pos =
					Projectile.Center
					+ new Vector2(
						-1 * Projectile.width,
						Projectile.height * Projectile.ai[0]
					).RotatedBy(Projectile.rotation) / 2;

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
									* 1f,
							new Vector3((float)i / ProjectileID.Sets.TrailCacheLength[Type], 1, 1),
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
									* 0.3f,
							new Vector3((float)i / ProjectileID.Sets.TrailCacheLength[Type], 0, 1),
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
									* 1f,
							new Vector3((float)i / ProjectileID.Sets.TrailCacheLength[Type], 1, 1),
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
									* 0.3f,
							new Vector3((float)i / ProjectileID.Sets.TrailCacheLength[Type], 0, 1),
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
				SamplerState.AnisotropicClamp,
				DepthStencilState.None,
				RasterizerState.CullNone,
				null,
				Main.GameViewMatrix.TransformationMatrix
			);
			Main.graphics.GraphicsDevice.Textures[0] = ModContent
				.Request<Texture2D>("ArknightsMod/Content/Projectiles/Guard/Laevatain/SlashTex")
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
	}
}
