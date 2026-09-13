using System;
using System.Collections.Generic;
using System.Linq;
using ArknightsMod.Content.Items.Weapons.Guard.SilverAsh;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.Graphics;
using Terraria.ID;
using static Terraria.Graphics.VertexStrip;

namespace ArknightsMod.Content.Projectiles.Guard.Laevatain
{
	public class LaevatainProjectile_1_plan2 : ModProjectile
	{
		public override string Texture =>
			"ArknightsMod/Content/Items/Weapons/Guard/Surtr/SurtrLaevatain";

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Player player = Main.player[Projectile.owner];
			var mp = player.GetModPlayer<WeaponPlayer>();
			Projectile.NewProjectile(
				player.GetSource_Death(),
				target.Center,
				Projectile.velocity.SafeNormalize(Vector2.Zero),
				ModContent.ProjectileType<LaevatainQiHitFlash>(),
				0,
				0,
				Main.myPlayer
			);
			if (target.life <= 0 && mp.CurrentSkill != null)
			{
				mp.StockCount = 1;
				mp.SP = 0;
			}
		}
		public override void SetDefaults()
		{
			Projectile.extraUpdates = 1;
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.scale = 1f;
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 900;
			Projectile.penetrate = -1;
			Projectile.alpha = 255;
			Projectile.light = 0.5f;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 60; 
			Projectile.usesIDStaticNPCImmunity = false;
			Projectile.idStaticNPCHitCooldown = 60;
		}

		public override void DrawBehind(
			int index,
			List<int> behindNPCsAndTiles,
			List<int> behindNPCs,
			List<int> behindProjectiles,
			List<int> overPlayers,
			List<int> overWiresUI
		) => overWiresUI.Add(index);

		private const float FlyOutSpeed = 0.9f;
		private const float MaxVisualRadius = 35f;
		private static readonly Color FrontColor = new Color(230, 40, 30);
		private static readonly Color BackColor = new Color(45, 220, 175);

		public Vector2[] oldVec = new Vector2[30];
		float t = 0;
		float flyDistance = 0f;

		public override void OnSpawn(IEntitySource source)
		{
			Player player = Main.player[Projectile.owner];
			t = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero).ToRotation();
			Projectile.velocity = t.ToRotationVector2() * FlyOutSpeed;
			Projectile.localAI[0] = player.direction;
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			player.itemTime = 2;
			player.itemAnimation = 2;
			player.heldProj = Projectile.whoAmI;
			int swingDir = (int)Projectile.localAI[0];
			player.direction = swingDir;
			float swingProgress = MathHelper.Clamp(Projectile.ai[1] / 17.5f, 0f, 1f);
			const float swingStartRot = -1.8f;
			float swingEndRot = swingDir == 1 ? 1.8f : -5.2f;
			float swingRot = MathHelper.Lerp(swingStartRot, swingEndRot, swingProgress);
			player.itemRotation = swingRot;
			Projectile.localAI[1] = swingRot;

			Vector2 swordCenter = player.MountedCenter + swingRot.ToRotationVector2() * 30f;
			if (Main.rand.NextBool(2))
			{
				float len = ModContent.Request<Texture2D>(Texture).Value.Size().Length();
				Vector2 trailPos =
					swordCenter
					+ swingRot.ToRotationVector2() * Main.rand.NextFloat(len * 0.8f, len) / 2f;

				Dust d = Dust.NewDustPerfect(
					trailPos,
					DustID.Torch,
					(swingRot + MathHelper.PiOver4 * swingDir).ToRotationVector2() * 5f,
					150,
					Color.OrangeRed,
					1.6f
				);
				d.noGravity = true;
			}

			float ndjd = 0;
			float speedFactor = 1f;
			if (Projectile.ai[1] > 30f)
				speedFactor = MathHelper.Clamp(1f - (Projectile.ai[1] - 30f) / 20f, 0f, 1f);

			flyDistance += FlyOutSpeed * speedFactor;
			Vector2 aimDir = t.ToRotationVector2();
			Projectile.velocity = aimDir * FlyOutSpeed * speedFactor; // 保留方向信息供 OnHitNPC/PreDraw 读取
			Projectile.Center = player.Center + aimDir * flyDistance;
			Projectile.ai[1] += 0.7f;
			Projectile.ai[0] = 3.14f / 2f + 3.14f / 4f;
			if (Projectile.width < 200 && Projectile.ai[1] < 35)
				Projectile.width = Projectile.height += 5;

			float displayRadius = MathHelper.Min(Projectile.ai[1], MaxVisualRadius);
			for (int gg = 0; gg < oldVec.Length; gg++)
			{
				for (int i = oldVec.Length - 1; i > 0; i--)
				{
					oldVec[i] = oldVec[i - 1];
				}
				oldVec[0] = (
					new Vector2(
						(float)Math.Sin(Projectile.ai[0] + ndjd),
						(float)Math.Cos(Projectile.ai[0] - ndjd)
					) * displayRadius
				).RotatedBy(t + 3.14 * .75f);
				//  Projectile.Center = player.Center + new Vector2((float)Math.Sin(Projectile.ai[0] + t), (float)Math.Cos(Projectile.ai[0] + t)) * 60f;
				Projectile.ai[0] += 0.12f; //旋转
			}

			if (Main.rand.NextBool(2))
			{
				Vector2 qiPos = Projectile.Center + oldVec[Main.rand.Next(oldVec.Length)]*Main.rand.NextFloat(2.5f, 4.0f);
				Color qiDustColor = Color.Lerp(FrontColor, BackColor, Main.rand.NextFloat(0f, 0.35f));
				Dust qiDust = Dust.NewDustPerfect(
					qiPos,
					DustID.Torch,
					Main.rand.NextVector2Circular(2f, 2f),
					100,
					qiDustColor,
					Main.rand.NextFloat(1.0f, 1.6f)
				);
				qiDust.noGravity = true;
			}
			if (Main.rand.NextBool(6))
			{
				Vector2 qiPos = Projectile.Center + oldVec[Main.rand.Next(oldVec.Length)]*Main.rand.NextFloat(2.5f, 4.0f);
				Dust spark = Dust.NewDustPerfect(
					qiPos,
					DustID.UltraBrightTorch,
					Main.rand.NextVector2Circular(1.5f, 1.5f),
					150,
					FrontColor,
					Main.rand.NextFloat(0.7f, 1.1f)
				);
				spark.noGravity = true;
			}
			if (Projectile.ai[1] > 5f)
				Projectile.friendly = true;
			if (Projectile.ai[1] > 50f)
				Projectile.active = false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Player player = Main.player[Projectile.owner];
			Texture2D swordTex = ModContent.Request<Texture2D>(Texture).Value;
			int swingDir = (int)Projectile.localAI[0];
			float swingRot = Projectile.localAI[1];
			Vector2 swordCenter = player.MountedCenter + swingRot.ToRotationVector2() * 30f;
			SpriteEffects swordEffects =
				swingDir == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

			Main.spriteBatch.Draw(
				swordTex,
				swordCenter - Main.screenPosition,
				null,
				Color.White,
				swingRot + MathHelper.PiOver4 * swingDir,
				swordTex.Size() / 2f,
				Projectile.scale,
				swordEffects,
				0f
			);
			Texture2D 贴图 = ModContent
				.Request<Texture2D>("ArknightsMod/Content/Textures/duaog/wbjex8")
				.Value;

			StripColorFunction stripColor = (x) =>
			{
				int alpha = (int)MathHelper.Clamp(
					170 - (int)(Projectile.ai[1] * Projectile.ai[1] * 0.16f),
					0,
					255
				);
				Color c = Color.Lerp(FrontColor, BackColor, x);
				c.A = (byte)alpha;
				return c;
			};

			StripColorFunction stripColor2 = stripColor;

			// 拖尾宽度同样用封顶后的视觉半径，和碰撞箱/螺旋轨迹保持一致
			float displayRadius = MathHelper.Min(Projectile.ai[1], MaxVisualRadius);

			float Cd = 80; // new Color(151, 151, 201, 255)
			VertexStrip strip = new VertexStrip();
			VertexStrip strip2 = new VertexStrip();
			VertexStrip strip3 = new VertexStrip();
			Vector2 wz = Projectile.Center;

			var rotations = oldVec
				.Zip(oldVec.Skip(1), (a, b) => a - b)
				.Select((a) => a.ToRotation());
			strip.PrepareStrip(
				oldVec,
				rotations.Prepend(rotations.FirstOrDefault()).ToArray(),
				stripColor,
				(x) => displayRadius * 2,
				-Main.screenPosition + Projectile.Center - Projectile.velocity * 10
			);

			strip2.PrepareStrip(
				oldVec,
				rotations.Prepend(rotations.FirstOrDefault()).ToArray(),
				stripColor2,
				(x) => displayRadius * 1.8f,
				-Main.screenPosition + Projectile.Center - Projectile.velocity * 10
			);
			BlendState blendStatef = new BlendState() //配置透明度保留状态
			{
				AlphaBlendFunction = BlendState.AlphaBlend.AlphaBlendFunction,
				AlphaDestinationBlend = BlendState.AlphaBlend.AlphaDestinationBlend,
				AlphaSourceBlend = BlendState.AlphaBlend.AlphaSourceBlend,
				ColorBlendFunction = (BlendFunction)0,
				ColorDestinationBlend = (Blend)5,
				ColorSourceBlend = BlendState.Additive.ColorSourceBlend,
				ColorWriteChannels = ColorWriteChannels.All,
				ColorWriteChannels1 = ColorWriteChannels.All,
				ColorWriteChannels2 = ColorWriteChannels.All,
				ColorWriteChannels3 = ColorWriteChannels.All,
				BlendFactor = Color.White,
				MultiSampleMask = -1,
			};
			BlendState blendStatef2 = new BlendState() //配置反色混合状态
			{
				AlphaBlendFunction = BlendState.Additive.AlphaBlendFunction,
				AlphaDestinationBlend = BlendState.Additive.AlphaDestinationBlend,
				AlphaSourceBlend = BlendState.Additive.AlphaSourceBlend,
				ColorBlendFunction = BlendFunction.ReverseSubtract,
				ColorDestinationBlend = BlendState.Additive.ColorDestinationBlend,
				ColorSourceBlend = BlendState.Additive.ColorSourceBlend,
				ColorWriteChannels = ColorWriteChannels.All,
				ColorWriteChannels1 = ColorWriteChannels.All,
				ColorWriteChannels2 = ColorWriteChannels.All,
				ColorWriteChannels3 = ColorWriteChannels.All,
				BlendFactor = Color.White,
				MultiSampleMask = -1,
			};
			//GameShaders.Armor.Apply(GameShaders.Armor.GetShaderIdFromItemId(3556), Projectile);
			Color color = new Color(
				255 - (int)(Projectile.ai[1] * 1.275f * 1.4f),
				200 - (int)(Projectile.ai[1] * 1.4f),
				0,
				255 - (int)(Projectile.ai[1] * 1.275f * 1.4f)
			);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.AlphaBlend,
				Main.DefaultSamplerState,
				DepthStencilState.None,
				Main.Rasterizer,
				null,
				Main.GameViewMatrix.TransformationMatrix
			);
			Main.graphics.GraphicsDevice.Textures[0] = 贴图;
			Main.graphics.GraphicsDevice.BlendState = blendStatef2;
			strip2.DrawTrail();
			strip2.DrawTrail();
			Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
			// strip2.DrawTrail();
			strip.DrawTrail();
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.AlphaBlend,
				Main.DefaultSamplerState,
				DepthStencilState.None,
				Main.Rasterizer,
				null,
				Main.GameViewMatrix.TransformationMatrix
			);

			return false;
		}
	}

	/// <summary>
	/// Laevatain 剑气命中特效——克隆自 SilverAsh 的 yinhdsz3，但改成赤红色调
	/// </summary>
	public class LaevatainQiHitFlash : ModProjectile
	{
		public override string Texture =>
			"ArknightsMod/Content/Items/Weapons/Guard/SilverAsh/SilverAshWeapon2";

		public override void SetDefaults()
		{
			Projectile.extraUpdates = 1;
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.scale = 1f;
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 20;
			Projectile.penetrate = -1;
			Projectile.alpha = 255;
			Projectile.light = 0.5f;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Projectile.ai[0] = Projectile.timeLeft;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + 3.14f / 2f;
		}

		public override void DrawBehind(
			int index,
			List<int> behindNPCsAndTiles,
			List<int> behindNPCs,
			List<int> behindProjectiles,
			List<int> overPlayers,
			List<int> overWiresUI
		) => overWiresUI.Add(index);

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D 贴图 = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/ex24").Value;
			int d = (int)(200f / Projectile.ai[0] * Projectile.timeLeft);
			// 赤红色版：把银灰的(d+30,d+30,d+55) 的蓝灰色换成偏红的色调
			Main.spriteBatch.Draw(
				贴图,
				Projectile.Center - Main.screenPosition - Projectile.velocity.SafeNormalize(Vector2.Zero) * 2f,
				null,
				new Color(d + 170, d + 20, d + 15, d / 4 + 200),
				Projectile.rotation,
				贴图.Size() / 2f,
				new Vector2(1.5f / Projectile.ai[0] * Projectile.timeLeft, 2.5f) / 1f * (Projectile.ai[1] + 1),
				SpriteEffects.None,
				0
			);

			Main.spriteBatch.Draw(
				贴图,
				Projectile.Center - Main.screenPosition - Projectile.velocity.SafeNormalize(Vector2.Zero) * 2f,
				null,
				new Color(d + 170, d + 20, d + 15, 180),
				Projectile.rotation,
				贴图.Size() / 2f,
				new Vector2(1.5f / Projectile.ai[0] * Projectile.timeLeft, 2.5f) / 1.5f * (Projectile.ai[1] + 1),
				SpriteEffects.None,
				0
			);
			return true;
		}
	}
}
