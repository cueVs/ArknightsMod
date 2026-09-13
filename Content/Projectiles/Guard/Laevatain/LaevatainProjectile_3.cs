using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Laevatain
{
	/// <summary>
	/// S3（Skill==2）攻击：
	///   1. 本体只在小人位置播放一段 11 帧的挥砍动画，不参与伤害判定。
	///   2. OnSpawn 时以鼠标方向为准，在 1000 距离内挑出最近的 3 个敌人当伤害目标，
	///      每个目标各生成一把 LaevatainProjectile_3_swordDrop（天降剑），
	///      从目标头顶落到脚下，落地才是实际伤害来源。
	/// </summary>
	public class LaevatainProjectile_3 : ModProjectile
	{
		public override string Texture =>
			"ArknightsMod/Content/Projectiles/Guard/Laevatain/LaevatainProjectile_3_melee";

		private const int FrameCount = 11;
		private const float SearchRange = 1000f;
		private const int MaxTargets = 3;
		private const float SearchConeCos = 0.5f; // 鼠标方向左右各 60 度视为"朝向"
		private static readonly Vector2 DrawOffset = new(-60f, -90f); // 相对小人锚点的画面偏移，左200上300

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = FrameCount;
		}

		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 20;

			Projectile.friendly = false; // 纯动画，伤害全部交给天降剑
			Projectile.DamageType = DamageClass.Melee;

			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 9999;
		}

		public override bool ShouldUpdatePosition() => false;

		public override void DrawBehind(
			int index,
			List<int> behindNPCsAndTiles,
			List<int> behindNPCs,
			List<int> behindProjectiles,
			List<int> overPlayers,
			List<int> overWiresUI
		) => behindNPCs.Add(index);

		public override void OnSpawn(IEntitySource source)
		{
			if (Main.myPlayer != Projectile.owner)
				return;

			Player player = Main.player[Projectile.owner];

			Vector2 aimDir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);

			List<NPC> candidates = [];
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (!npc.CanBeChasedBy(player) || npc.friendly || npc.life <= 0 || npc.dontTakeDamage)
					continue;
				if (Vector2.DistanceSquared(npc.Center, player.Center) > SearchRange * SearchRange)
					continue;
				if (Vector2.Dot((npc.Center - player.Center).SafeNormalize(aimDir), aimDir) < SearchConeCos)
					continue;

				candidates.Add(npc);
			}

			var targets = candidates
				.OrderBy(npc => Vector2.DistanceSquared(npc.Center, player.Center))
				.Take(MaxTargets);

			foreach (NPC npc in targets)
			{
				// 只传目标的 npc 索引，下落过程中天降剑自己每帧读取目标当前位置来跟随，
				// 而不是在这里把落点写死成一个坐标快照
				Vector2 spawnCenter = npc.Bottom - new Vector2(0f, LaevatainProjectile_3_swordDrop.FallHeight);
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					spawnCenter,
					Vector2.Zero,
					ModContent.ProjectileType<LaevatainProjectile_3_swordDrop>(),
					Projectile.damage,
					Projectile.knockBack,
					Projectile.owner,
					npc.whoAmI
				);
			}
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
				Projectile.timeLeft = player.itemAnimationMax > 0 ? player.itemAnimationMax : 60;
				Projectile.localAI[0] = Projectile.timeLeft;
				Projectile.netUpdate = true;
			}
			player.itemTime = 2;
			player.itemAnimation = 2;
			Projectile.spriteDirection = player.direction;
			Projectile.Center = player.Center + DrawOffset;

			float progress = 1f - Projectile.timeLeft / Projectile.localAI[0];
			Projectile.frame = (int)MathHelper.Clamp(progress * FrameCount, 0, FrameCount - 1);
		}
	}

	/// <summary>
	/// 天降剑：在目标头顶生成，0.3s 内落到目标脚下位置，落地瞬间造成伤害并留下火焰特效。
	/// 下落过程中持续跟随目标当前位置（每帧读取目标的 Bottom），保证任何时刻都在目标正上方；
	/// 落地后不再跟随，固定停留在触地那一刻的位置直至淡出。
	/// </summary>
	public class LaevatainProjectile_3_swordDrop : ModProjectile
	{
		public override string Texture =>
			"ArknightsMod/Content/Items/Weapons/Guard/Surtr/SurtrLaevatain";

		public const int Size = 90; // 碰撞箱边长，生成位置换算要用到
		public const float FallHeight = 220f; // 下落起始高度（目标正上方多高开始落）

		private const int FallTicks = 18; // 0.3s * 60，垂直下落时长
		private const int HitWindowTicks = 4; // 落地瞬间的伤害判定窗口
		private const int FadeTicks = 18; // 0.3s * 60，落地命中后停留、期间透明度逐渐降低
		// 贴图默认朝向是"剑柄左下-剑尖右上"，下落时想让剑尖朝下，经验旋转值，如果朝向不对可以调这个
		// 在原有 45° 基础上再顺时针转 90°（贴图坐标系里正值就是顺时针）
		private const float FallRotationOffset = MathHelper.PiOver4 + MathHelper.PiOver2;

		private Vector2 lastKnownBottom; // 目标最近一次的脚下位置，目标失效后作为兜底
		private bool impactSpawned;

		// ── 金色拖尾参数：下落时剑身后拉出一条金光 ──
		private const float TrailMaxLength = 140f; // 拖尾最大长度
		private const float TrailWidth = 14f;      // 拖尾宽度

		public override void SetDefaults()
		{
			Projectile.width = Size;
			Projectile.height = Size;

			Projectile.friendly = false; // 下落途中不判定，落地才开启
			Projectile.DamageType = DamageClass.Melee;

			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = FallTicks + FadeTicks;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
		}

		public override bool ShouldUpdatePosition() => false;

		public override void OnSpawn(IEntitySource source)
		{
			NPC target = GetTarget();
			lastKnownBottom = target?.Bottom ?? Projectile.Center;
		}

		private NPC GetTarget()
		{
			int index = (int)Projectile.ai[0];
			if (index < 0 || index >= Main.maxNPCs)
				return null;

			NPC npc = Main.npc[index];
			return npc.active && npc.life > 0 ? npc : null;
		}

		public override void AI()
		{
			NPC target = GetTarget();
			if (target != null)
				lastKnownBottom = target.Bottom; // 每帧刷新，跟随目标移动

			int elapsed = FallTicks + FadeTicks - Projectile.timeLeft;

			if (elapsed < FallTicks)
			{
				float t = elapsed / (float)FallTicks;
				float easedT = t * t; // 越落越快，模拟重力加速
				float altitude = FallHeight * (1f - easedT); // 离目标脚下还有多高
				Projectile.Center = lastKnownBottom - new Vector2(0f, altitude); // 始终在目标正上方
				Projectile.rotation = FallRotationOffset;

				// 金色拖尾点缀：尘埃留在下落轨迹上、微微上飘，形成剑身后的拖尾
				if (Main.rand.NextBool(2))
				{
					Dust d = Dust.NewDustPerfect(
						Projectile.Center + Main.rand.NextVector2Circular(8f, 20f),
						DustID.Torch,
						-Vector2.UnitY * Main.rand.NextFloat(0.5f, 1.5f),
						120,
						Color.Gold,
						Main.rand.NextFloat(0.9f, 1.4f)
					);
					d.noGravity = true;
				}
			}
			else
			{
				Projectile.Center = lastKnownBottom; // 落地后固定，不再跟随
				int sinceLanding = elapsed - FallTicks;
				Projectile.friendly = sinceLanding < HitWindowTicks; // 落地瞬间短暂判定，随后只停留淡出

				if (!impactSpawned)
				{
					impactSpawned = true;
					Projectile.netUpdate = true;

					SpawnLandingSparks(Projectile.Center, FallHeight);

					if (Main.myPlayer == Projectile.owner)
					{
						Projectile.NewProjectile(
							Projectile.GetSource_FromThis(),
							Projectile.Center,
							Vector2.Zero,
							ModContent.ProjectileType<LaevatainProjectile_3_impactFire>(),
							0,
							0f,
							Projectile.owner
						);
					}
				}
			}
		}

		// Terraria 普通 Dust 每帧的重力加速度（noGravity=false 时）
		private const float DustGravity = 0.1f;

		private static void SpawnLandingSparks(Vector2 landPos, float fallDistance)
		{
			float peakHeight = fallDistance / 2f; // 蹦起的最高点，取下落高度的一半
			float launchSpeed = (float)Math.Sqrt(2f * DustGravity * peakHeight);

			for (int i = 0; i < 10; i++)
			{
				Vector2 velocity = new(
					Main.rand.NextFloat(-2f, 2f),
					-launchSpeed * Main.rand.NextFloat(0.7f, 1f)
				);

				// 红黄为主，绿色只作为少量点缀
				float roll = Main.rand.NextFloat();
				Color sparkColor = roll < 0.45f ? Color.Red
					: roll < 0.85f ? Color.Gold
					: Color.LimeGreen;

				Dust d = Dust.NewDustPerfect(
					landPos,
					DustID.Torch,
					velocity,
					100,
					sparkColor,
					Main.rand.NextFloat(1.0f, 1.6f)
				);
				d.noGravity = false; // 蹦起后自由下落
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Projectile.timeLeft <= 0)
				return false;

			int elapsed = FallTicks + FadeTicks - Projectile.timeLeft;

			// 金色拖尾：仅下落阶段画，剑身沿下落方向上方拉出一条金光
			if (elapsed < FallTicks)
				DrawFallTrail(elapsed);

			float alpha = 1f;
			if (elapsed >= FallTicks)
			{
				int sinceLanding = elapsed - FallTicks;
				float fadeProgress = MathHelper.Clamp(sinceLanding / (float)FadeTicks, 0f, 1f);
				alpha = MathHelper.Lerp(1f, 0.5f, fadeProgress);
			}

			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Main.spriteBatch.Draw(
				tex,
				Projectile.Center - Main.screenPosition,
				null,
				Color.White * alpha,
				Projectile.rotation,
				tex.Size() / 2f,
				Projectile.scale,
				SpriteEffects.None,
				0f
			);
			return false;
		}

		// 金色拖尾：贴图从剑身向上拉伸绘制，加法混合发光
		private void DrawFallTrail(int elapsed)
		{
			float t = MathHelper.Clamp(elapsed / (float)FallTicks, 0f, 1f);
			float easedT = t * t; // 与下落的缓动一致：落得越快拖尾拉得越长
			float trailLen = Math.Min(FallHeight * easedT, TrailMaxLength);
			if (trailLen < 4f)
				return;

			Texture2D tex = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/duaog/wbjex8").Value;

			// 切到加法混合，画完换回 AlphaBlend，不影响后面的剑本体绘制
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.Additive,
				Main.DefaultSamplerState,
				DepthStencilState.None,
				RasterizerState.CullNone,
				null,
				Main.GameViewMatrix.TransformationMatrix
			);

			// 以贴图左缘中点为原点，旋转 -90° 使其竖直向上延伸（剑往下落，拖尾在剑上方）
			Main.spriteBatch.Draw(
				tex,
				Projectile.Center - Main.screenPosition,
				null,
				Color.Gold * 0.8f,
				-MathHelper.PiOver2,
				new Vector2(0f, tex.Height / 2f),
				new Vector2(trailLen / tex.Width, TrailWidth / tex.Height),
				SpriteEffects.None,
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
		}
	}

	/// <summary>
	/// 天降剑落地时留下的火焰视觉效果，纯装饰，不判定伤害。
	/// </summary>
	public class LaevatainProjectile_3_impactFire : ModProjectile
	{
		public override string Texture =>
			"ArknightsMod/Content/Projectiles/Guard/Laevatain/LaevatainProjectile_3_fire";

		private const int Lifetime = 30;

		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;

			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Melee;

			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = Lifetime;
		}

		public override bool ShouldUpdatePosition() => false;

		public override void AI()
		{
			Lighting.AddLight(Projectile.Center, 1.0f, 0.5f, 0.2f);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			float progress = 1f - Projectile.timeLeft / (float)Lifetime;
			float scale = MathHelper.Lerp(0.6f, 0.9f, progress);

			// 贴图是直通(非预乘)透明度，默认 AlphaBlend 按预乘透明度处理会把半透明区域画得过浓，这里换成 NonPremultiplied
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
				Projectile.Center - Main.screenPosition,
				null,
				Color.White, // 原图输出，不叠加染色/淡出
				0f,
				tex.Size() / 2f,
				scale,
				SpriteEffects.None,
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
			return false;
		}
	}
}
