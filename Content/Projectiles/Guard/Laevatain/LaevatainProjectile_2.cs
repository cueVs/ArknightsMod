using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.Graphics.VertexStrip;

namespace ArknightsMod.Content.Projectiles.Guard.Laevatain
{
	public class LaevatainProjectile_2 : ModProjectile
	{
		public override string Texture =>
			"ArknightsMod/Content/Items/Weapons/Guard/Surtr/SurtrLaevatain";

		// ── 共享时间基准 ──
		private int totalTicks; 
		private int delayThresholdB; // 特效 B 的启动延迟（totalTicks 的 0.05 倍）
		private float aimAngle;
		private float progress; // 0→1，主时间线（刺击/特效A/白光都用它）
		private float progressB; // 0→1，特效 B 自己的时间线（延迟后才开始）
		private bool bActive;

		// ── 子系统1：刺击剑身 ──
		private const float ThrustMaxDistance = 100f;
		private Vector2 thrustCenter;
		private float thrustRotation;

		// ── 子系统2/3：特效 A / 特效 B（正弦拖尾） ──
		private const int TrailLen = 16;

		// ── 粒子束参数：向前直线射出，束本身就是命中判定 ──
		private const float BeamLength = 600f;   // 束长度
		private const float BeamMinWidth = 3f;   // 束最窄
		private const float BeamMaxWidth = 7f;   // 束最宽

		// 束时间轴：前 0.5s 束尖从根部发射到 600；后 0.5s 随剑收回，
		// 束根向束尖追赶，整条束从后至前消失；1s 后无束、无判定
		private const int BeamGrowTicks = 30;
		private const int BeamFadeTicks = 30;
		private float beamRoot;       // 当帧束根距 BeamOrigin 的距离
		private float beamTip;        // 当帧束尖距 BeamOrigin 的距离
		private float beamAlpha = 1f; // 束亮度，消失段逐渐变暗

		// ── 命中统计：整次攻击只命中一个敌人时，伤害 ×1.5 ──
		private readonly HashSet<int> hitTargets = new HashSet<int>();
		private const float WaveDistance = 600f;
		private const float A_StartAmp = 50f, A_EndAmp = 20f;
		private const float B_StartAmp = 40f, B_EndAmp = 15f;
		private const float B_DelayFraction = 0.05f;

		private static readonly Color FrontColor = new Color(230, 40, 30);
		private static readonly Color BackColor = new Color(45, 220, 175);

		private readonly Vector2[] trailA = new Vector2[TrailLen];
		private readonly Vector2[] trailB = new Vector2[TrailLen];

		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 20;

			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;

			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 9999; // OnSpawn 里会按攻击间隔重设

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 12;
		}

		public override bool ShouldUpdatePosition() => false; // 位置完全手动计算

		public override void OnSpawn(IEntitySource source)
		{
			Player player = Main.player[Projectile.owner];
			aimAngle = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX).ToRotation();

			totalTicks = Math.Max(1, player.itemAnimationMax);
			delayThresholdB = (int)(totalTicks * B_DelayFraction);
			Projectile.timeLeft = totalTicks + delayThresholdB; // 主时间线 + 特效B的延迟缓冲
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			player.heldProj = Projectile.whoAmI;
			Projectile.Center = player.MountedCenter; // 仅作基准锚点，vanilla 记账用

			int lifespan = totalTicks + delayThresholdB;
			int elapsedTicks = lifespan - Projectile.timeLeft;
			progress = MathHelper.Clamp(elapsedTicks / (float)totalTicks, 0f, 1f);

			int elapsedSinceB = elapsedTicks - delayThresholdB;
			bActive = elapsedSinceB >= 0;
			if (bActive)
				progressB = MathHelper.Clamp(elapsedSinceB / (float)totalTicks, 0f, 1f);

			Vector2 aimDir = aimAngle.ToRotationVector2();
			Vector2 perpDir = new Vector2(-aimDir.Y, aimDir.X);

			// ── 子系统1：刺击 ──
			float thrustDist = ThrustMaxDistance * (1f - Math.Abs(2f * progress - 1f));
			float hop = 15f * Envelope20_60_20(progress); // 前20%升到15，后20%落回0
			Vector2 vertOffset = new Vector2(0f, 20f - hop); // 常量下移20，叠加动态上抬
			thrustCenter = player.MountedCenter + aimDir * thrustDist + vertOffset;

			float extraRotation = -MathHelper.PiOver4 * Envelope20_60_20(progress); // 逆时针45°
			thrustRotation = aimAngle + MathHelper.PiOver2 + extraRotation;

			// ── 粒子束：向前直线射出长 600、宽 3~7，同时作为命中判定（见 Colliding）──
			// 前 0.5s 束尖发射到 600；后 0.5s 随剑收回，束根向束尖追赶、从后至前消失
			if (elapsedTicks < BeamGrowTicks)
			{
				beamRoot = 0f;
				beamTip = BeamLength * (elapsedTicks / (float)BeamGrowTicks);
				beamAlpha = 1f;
			}
			else if (elapsedTicks < BeamGrowTicks + BeamFadeTicks)
			{
				float fadeProgress = (elapsedTicks - BeamGrowTicks) / (float)BeamFadeTicks;
				beamRoot = BeamLength * fadeProgress;
				beamTip = BeamLength;
				beamAlpha = MathHelper.Lerp(1f, 0.35f, fadeProgress);
			}
			else
			{
				beamRoot = 0f;
				beamTip = 0f;
				beamAlpha = 1f;
			}

			SpawnBeamDust(player, aimDir);

			// ── 子系统2：特效 A（sin）── 只存相对偏移量，不掺入玩家位置，
			// 避免玩家移动的位移和波形本身的推进混在一起导致波形被压缩/拉伸
			float sA = progress * WaveDistance;
			float ampA = MathHelper.Lerp(A_StartAmp, A_EndAmp, progress);
			float lateralA = ampA * (float)Math.Sin(progress * MathHelper.TwoPi);
			ShiftTrail(trailA, aimDir * sA + perpDir * lateralA);

			// ── 子系统3：特效 B（-sin，延迟启动）── 同样只存相对偏移量
			if (bActive)
			{
				float sB = progressB * WaveDistance;
				float ampB = MathHelper.Lerp(B_StartAmp, B_EndAmp, progressB);
				float lateralB = -ampB * (float)Math.Sin(progressB * MathHelper.TwoPi);
				ShiftTrail(trailB, aimDir * sB + perpDir * lateralB);
			}
		}

		private static float Envelope20_60_20(float p)
		{
			if (p < 0.2f)
				return p / 0.2f;
			if (p < 0.8f)
				return 1f;
			return MathHelper.Clamp(1f - (p - 0.8f) / 0.2f, 0f, 1f);
		}

		private static void ShiftTrail(Vector2[] trail, Vector2 newPos)
		{
			for (int i = trail.Length - 1; i > 0; i--)
				trail[i] = trail[i - 1];
			trail[0] = newPos;
		}

		// ── 粒子束 ──

		// 束的起点：对齐剑身那条"常量下移 20"的线
		private static Vector2 BeamOrigin(Player player) =>
			player.MountedCenter;

		// 束宽在 3~5 之间随挥舞脉动
		private float BeamWidth =>
			MathHelper.Lerp(BeamMinWidth, BeamMaxWidth,
				(float)Math.Sin(progress * MathHelper.TwoPi * 2f) * 0.5f + 0.5f);

		// 束内的流动粒子：每帧沿束随机位置生成若干颗，朝束前方飘
		private void SpawnBeamDust(Player player, Vector2 aimDir)
		{
			float span = beamTip - beamRoot;
			if (span <= 1f)
				return; // 束还没发射/已消失，不产粒子

			Vector2 start = BeamOrigin(player);
			Vector2 perp = new Vector2(-aimDir.Y, aimDir.X);
			float width = BeamWidth;

			int count = Main.rand.Next(4, 7);
			for (int i = 0; i < count; i++)
			{
				Vector2 pos = start
					+ aimDir * Main.rand.NextFloat(beamRoot, beamTip)
					+ perp * Main.rand.NextFloat(-width, width) / 2f;
				Vector2 vel = aimDir * Main.rand.NextFloat(2f, 5f)
					+ perp * Main.rand.NextFloat(-0.3f, 0.3f);

				// 金白火色，呼应束体
				float roll = Main.rand.NextFloat();
				Color dustColor = roll < 0.6f ? Color.Gold : Color.White;

				Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 100, dustColor, Main.rand.NextFloat(0.5f, 0.9f));
				d.noGravity = true;
			}
		}

		// 束体：贴图拉成一条直线，加法混合发光
		private void DrawBeam(Player player)
		{
			float span = beamTip - beamRoot;
			if (span <= 1f)
				return; // 束还没发射/已消失，不绘制

			float width = BeamWidth;
			Vector2 aimDir = aimAngle.ToRotationVector2();
			Texture2D tex = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/duaog/wbjex8").Value;

			// 切到加法混合，画完换回 AlphaBlend，不影响后面的绘制
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

			// 以贴图左缘中点为原点，沿 aimAngle 拉伸：长 600、宽 3~7
			Main.spriteBatch.Draw(
				tex,
				BeamOrigin(player) + aimDir * beamRoot - Main.screenPosition,
				null,
				Color.Gold * (0.85f * beamAlpha),
				aimAngle,
				new Vector2(0f, tex.Height / 2f),
				new Vector2(span / tex.Width, width / tex.Height),
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

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			// 判定 = 粒子束当前可见段；未发射/已消失时无判定
			float span = beamTip - beamRoot;
			if (span <= 1f)
				return false;

			Player player = Main.player[Projectile.owner];
			Vector2 dir = aimAngle.ToRotationVector2();
			Vector2 start = BeamOrigin(player) + dir * beamRoot;
			Vector2 end = BeamOrigin(player) + dir * beamTip;
			float collisionPoint = 0f;

			return Collision.CheckAABBvLineCollision(
				targetHitbox.TopLeft(),
				targetHitbox.Size(),
				start,
				end,
				BeamMaxWidth,
				ref collisionPoint
			);
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			// 单目标增伤：还没命中过任何敌人（本次攻击第一击），
			// 或目前为止只命中过这一个敌人（对同一目标的持续命中），伤害 ×1.5。
			// 注意：若第一击之后又命中了别的敌人，第一击的加成已结算、无法收回
			if (hitTargets.Count == 0 || (hitTargets.Count == 1 && hitTargets.Contains(target.whoAmI)))
				modifiers.FinalDamage *= 1.5f;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			// 命中实际生效后才记入统计，避免被无敌帧挡下的接触被误算
			hitTargets.Add(target.whoAmI);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Player player = Main.player[Projectile.owner];

			// 粒子束体：画在拖尾和剑本体之下
			DrawBeam(player);

			// ── 特效 A / B 拖尾 ──
			DrawSineTrail(trailA, progress, A_StartAmp, A_EndAmp, player.MountedCenter);
			if (bActive)
				DrawSineTrail(trailB, progressB, B_StartAmp, B_EndAmp, player.MountedCenter);

			// ── 刺击剑身 ──
			// player.direction == -1 时，沿贴图左下-右上对角线镜像：
			// 数学上等价于「旋转90° + 单轴翻转」，具体方向如效果不对可尝试改成 +90° 或换成 FlipVertically
			Texture2D swordTex = ModContent.Request<Texture2D>(Texture).Value;
			SpriteEffects swordEffects = SpriteEffects.None;
			float mirrorRotationAdjust = 0f;
			if (player.direction == -1)
			{
				swordEffects = SpriteEffects.FlipHorizontally;
				mirrorRotationAdjust = MathHelper.PiOver2;
			}

			Main.spriteBatch.Draw(
				swordTex,
				thrustCenter - Main.screenPosition,
				null,
				Color.White,
				thrustRotation + mirrorRotationAdjust,
				swordTex.Size() / 2f,
				Projectile.scale,
				swordEffects,       
				0f
			);

			return false;
		}

		private void DrawSineTrail(Vector2[] trail, float p, float startAmp, float endAmp, Vector2 currentAnchor)
		{
			if (trail[1] == Vector2.Zero)
				return; // 拖尾还没攒够点

			var rotations = trail.Zip(trail.Skip(1), (a, b) => a - b).Select(v => v.ToRotation());
			float[] rotArray = rotations.Prepend(rotations.FirstOrDefault()).ToArray();

			float currentAmp = MathHelper.Lerp(startAmp, endAmp, p);

			// x: 0=拖尾最新点，1=拖尾最旧点；整体透明度随攻击进度由 0.3 逐渐提升到 0.5
			StripColorFunction stripColor = (x) =>
			{
				float tailFade = 1f - x;
				int alpha = (int)MathHelper.Clamp(MathHelper.Lerp(255f * 0.3f, 255f * 0.5f, p) * tailFade, 0f, 255f);
				Color c = Color.Lerp(FrontColor, BackColor, x);
				c.A = (byte)alpha;
				return c;
			};

			Texture2D 贴图 = ModContent
				.Request<Texture2D>("ArknightsMod/Content/Textures/duaog/wbjex8")
				.Value;

			VertexStrip strip = new VertexStrip();
			VertexStrip strip2 = new VertexStrip();

			// trail 里存的是纯偏移量，这里统一加上玩家当前位置，让整条波形跟随玩家平移而不改变形状
			Vector2 drawOffset = currentAnchor - Main.screenPosition;
			strip.PrepareStrip(trail, rotArray, stripColor, (x) => currentAmp * 0.8f, drawOffset);
			strip2.PrepareStrip(trail, rotArray, stripColor, (x) => currentAmp * 0.45f, drawOffset);

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
			strip.DrawTrail();
			strip.DrawTrail();
			Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
			strip2.DrawTrail();
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
		}
	}
}
