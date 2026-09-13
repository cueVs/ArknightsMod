using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ArknightsMod.Content.NPCs.Enemy.OF.Pmp
{
	public class PmpSlugEgg : ModProjectile
	{
		public override string Texture => "ArknightsMod/Content/Projectiles/PmpSlugEggFly";

		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 600;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.aiStyle = 0;
		}

		public override void AI()
		{
			Projectile.velocity.Y += 0.25f;
			Projectile.rotation = Projectile.velocity.ToRotation();

			for (int i = 0; i < 4; i++)
			{
				int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
					DustID.Torch, -Projectile.velocity.X * 0.4f, -Projectile.velocity.Y * 0.4f, 100, default, 1.3f);
				Main.dust[d].noGravity = true;
				Main.dust[d].velocity *= 0.5f;
			}
			for (int i = 0; i < 2; i++)
			{
				int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
					DustID.FlameBurst, -Projectile.velocity.X * 0.2f, -Projectile.velocity.Y * 0.2f, 60, default, 1.1f);
				Main.dust[d].noGravity = true;
				Main.dust[d].velocity += Main.rand.NextVector2Circular(0.8f, 0.8f);
			}
			if (Main.rand.NextBool(3))
			{
				int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
					DustID.Torch, Main.rand.NextFloatDirection() * 1.5f, Main.rand.NextFloatDirection() * 1.5f, 80, default, 0.7f);
				Main.dust[d].noGravity = false;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Vector2 landPos = Projectile.Center;
			SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);

			for (int i = 0; i < 12; i++)
			{
				Vector2 spd = Main.rand.NextVector2Circular(2.5f, 1.2f);
				spd.Y = -Math.Abs(spd.Y);
				int d = Dust.NewDust(landPos, 2, 2, DustID.Torch, spd.X, spd.Y, 100, default, 1.1f);
				Main.dust[d].noGravity = false;
			}
			for (int i = 0; i < 6; i++)
			{
				Vector2 spd = Main.rand.NextVector2Circular(1.5f, 0.8f);
				spd.Y = -Math.Abs(spd.Y);
				int d = Dust.NewDust(landPos, 2, 2, DustID.FlameBurst, spd.X, spd.Y, 80, default, 0.9f);
				Main.dust[d].noGravity = true;
			}

			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				NPC.NewNPC(Projectile.GetSource_FromAI(), (int)landPos.X, (int)landPos.Y, NPCType<PmpSlugEggLandNPC>());
			}

			return true;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
		{
			// 允许虫卵稳定落在平台类方块上，而不是穿过平台继续下坠
			fallThrough = false;
			return true;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			var spriteBatch = Main.spriteBatch;
			Texture2D tex = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/PmpSlugEggFly").Value;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			spriteBatch.Draw(tex, drawPos, null, lightColor, Projectile.rotation,
				tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}
