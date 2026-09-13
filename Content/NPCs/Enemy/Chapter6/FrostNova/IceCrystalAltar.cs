using System;
using System.Collections.Generic;
using ArknightsMod.Common.VisualEffects;
using ArknightsMod.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;

namespace ArknightsMod.Content.NPCs.Enemy.Chapter6.FrostNova
{
	/// <summary>
	/// 霜星二阶段：冰晶祭坛。可被玩家摧毁，每隔约 4～9 秒（随机）经短暂蓄力（贴图骤亮偏白）后释放一轮完整冲击波动画（总时长约 0.4～0.5 秒不等）：向内收缩预警环（无伤害）、短促喷发、造成少量伤害并概率冰冻玩家；粒子渐隐结束。登场后附近间歇生成飘向祭坛的风絮线条（<see cref="IceAltarInboundWisp"/>）。周身双线拖尾环与霜星无敌时特效同源（亮蓝色）。
	/// </summary>
	public class IceCrystalAltar : ModNPC
	{
		public const int MaxConcurrentAltars = 5;

		/// <summary>祭坛碰撞箱宽高（与 <see cref="SetDefaults"/> 一致）。</summary>
		public const int AltarNpcWidth = 56;
		public const int AltarNpcHeight = 72;
		/// <summary>双线拖尾环绕中心相对 <see cref="NPC.Center"/> 的 Y 偏移（略上移，与下移后的贴图重心搭配）。</summary>
		public const float AltarTwinLineAnchorOffsetY = AltarNpcHeight * 0.32f;
		/// <summary>仅影响贴图绘制下移（像素）；碰撞箱仍以 <see cref="NPC.Center"/> 为准。冲击波预警/喷发视觉锚点使用同一下移以对齐贴图。</summary>
		public const float AltarDrawGfxOffY = 14f;

		/// <summary>由祭坛生成用的左上角世界坐标推算碰撞箱中心。</summary>
		public static Vector2 HitboxCenterFromSpawnTopLeft(Vector2 spawnTopLeftWorld) =>
			spawnTopLeftWorld + new Vector2(AltarNpcWidth * 0.5f, AltarNpcHeight * 0.5f);

		/// <summary>祭坛于落点出现时：在碰撞箱中心世界坐标处爆开亮蓝粒子（仅图形端；勿在纯 dedServ 调用）。联机上需在客户端也调用（例如风絮 <see cref="IceAltarSummonWisp.Kill"/>）。</summary>
		public static void SpawnAltarAppearBurstAt(Vector2 altarHitboxCenterWorld) {
			if (Main.dedServ)
				return;
			Lighting.AddLight(altarHitboxCenterWorld, 0.45f, 0.85f, 1f);
			var coreBlue = new Color(130, 240, 255);
			var edgeBlue = new Color(60, 170, 255);
			for (int i = 0; i < 72; i++) {
				float ang = MathHelper.TwoPi * i / 72f + Main.rand.NextFloat(-0.22f, 0.22f);
				float spd = Main.rand.NextFloat(3.2f, 12f);
				Vector2 vel = ang.ToRotationVector2() * spd + Main.rand.NextVector2Circular(1f, 1f);
				int dustType = Main.rand.Next(4) switch {
					0 => DustID.IceTorch,
					1 => DustID.BlueTorch,
					2 => DustID.PortalBolt,
					_ => DustID.Ice
				};
				var col = Color.Lerp(Color.White, Color.Lerp(coreBlue, edgeBlue, Main.rand.NextFloat()), Main.rand.NextFloat(0.55f, 1f));
				Dust d = Dust.NewDustPerfect(altarHitboxCenterWorld + Main.rand.NextVector2Circular(14f, 14f), dustType, vel, 0, col, Main.rand.NextFloat(1.15f, 2.6f));
				d.noGravity = Main.rand.NextFloat() < 0.38f;
				d.fadeIn = Main.rand.NextFloat(0.08f, 0.35f);
			}
			for (int j = 0; j < 40; j++) {
				Dust.NewDustPerfect(altarHitboxCenterWorld + Main.rand.NextVector2Circular(6f, 6f), DustID.IceTorch,
					Main.rand.NextVector2Circular(7.5f, 7.5f), 0, Color.Lerp(Color.White, coreBlue, Main.rand.NextFloat(0.4f, 1f)), Main.rand.NextFloat(0.65f, 1.45f));
			}
			for (int k = 0; k < 14; k++) {
				float a = Main.rand.NextFloat() * MathHelper.TwoPi;
				Dust.NewDustPerfect(altarHitboxCenterWorld, DustID.BlueTorch, a.ToRotationVector2() * Main.rand.NextFloat(1.2f, 4f), 0,
					Color.White, Main.rand.NextFloat(0.9f, 1.4f));
			}
		}

		/// <summary>冲击波最小间隔（刻），4 秒 @60TPS（原 2 秒一倍放慢）。</summary>
		private const int MinShockwaveIntervalTicks = 240;
		/// <summary>冲击波最大间隔（刻），9 秒 @60TPS（原 4.5 秒一倍放慢）。</summary>
		private const int MaxShockwaveIntervalTicks = 540;
		/// <summary>发射冲击波前蓄力帧数（同步于 <see cref="NPC.ai"/>[0]）；先变白增亮再生成 <see cref="IceAltarShockwave"/>。</summary>
		private const int ShockwaveChargeVisualTicks = 14;

		public override string Texture => "ArknightsMod/Content/NPCs/Enemy/Chapter6/FrostNova/FNIceCrystal_dir";

		private int shockwaveTimer;
		/// <summary>距下一次冲击波的间隔（刻）。每座祭坛独立随机，范围约 4～9 秒。</summary>
		private int shockwaveIntervalTicks;
		private bool twinLineRingsSpawned;
		/// <summary>祭坛登场后环绕生成的「飞向中心风絮」冷却（刻）。</summary>
		private int _ambientWispCooldown;

		public static int CountActiveAltars() {
			int id = ModContent.NPCType<IceCrystalAltar>();
			int n = 0;
			for (int i = 0; i < Main.maxNPCs; i++) {
				if (Main.npc[i].active && Main.npc[i].type == id)
					n++;
			}
			return n;
		}

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 1;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
		}

		public override void SetDefaults() {
			NPC.width = AltarNpcWidth;
			NPC.height = AltarNpcHeight;
			NPC.lifeMax = 3000;
			NPC.life = 3000;
			NPC.damage = 0;
			NPC.defense = 12;
			NPC.HitSound = SoundID.Tink;
			NPC.DeathSound = SoundID.Shatter;
			NPC.knockBackResist = 0.1f;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.aiStyle = -1;
			NPC.friendly = false;
			NPC.netAlways = true;
		}

		/// <summary>生成后若干帧由暗到亮（<see cref="NPC.ai"/>[2]）；蓄力冲击波时 <see cref="NPC.ai"/>[0]&gt;0 贴图骤亮偏白。</summary>
		public override Color? GetAlpha(Color drawColor) {
			const float spawnFrames = 20f;
			Color c = drawColor;
			bool modified = false;
			if (NPC.ai[2] < spawnFrames) {
				float t = MathHelper.Clamp(NPC.ai[2] / spawnFrames, 0f, 1f);
				float op = MathHelper.Lerp(0.06f, 1f, t * t);
				c *= op;
				modified = true;
			}
			if (NPC.ai[0] > 0f) {
				float u = 1f - MathHelper.Clamp(NPC.ai[0] / ShockwaveChargeVisualTicks, 0f, 1f);
				float rise = MathF.Pow(Utils.GetLerpValue(0f, 0.4f, u, clamped: true), 0.48f);
				float hold = Utils.GetLerpValue(0.5f, 1f, u, clamped: true);
				float flash = MathHelper.Clamp(rise * 0.98f + hold * 0.52f, 0f, 1f);
				c = Color.Lerp(c, Color.White, flash * 0.9f);
				int add = (int)(flash * 75f);
				c = new Color(
					(byte)Math.Min(255, c.R + add),
					(byte)Math.Min(255, c.G + add),
					(byte)Math.Min(255, c.B + add),
					c.A);
				modified = true;
			}
			return modified ? c : null;
		}

		public override void AI() {
			NPC.gfxOffY = AltarDrawGfxOffY;
			const float spawnFrames = 20f;
			if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] < spawnFrames) {
				NPC.ai[2] += 1f;
				if (Main.netMode == NetmodeID.Server)
					NPC.netUpdate = true;
			}
			if (!twinLineRingsSpawned && Main.netMode != NetmodeID.MultiplayerClient) {
				twinLineRingsSpawned = true;
				int w = NPC.whoAmI;
				var src = NPC.GetSource_FromThis();
				Projectile.NewProjectile(src, NPC.Center, Vector2.Zero, ModContent.ProjectileType<IceAltarTwinLineRing>(), 0, 0f, Main.myPlayer, w, 1f);
				Projectile.NewProjectile(src, NPC.Center, Vector2.Zero, ModContent.ProjectileType<IceAltarTwinLineRing>(), 0, 0f, Main.myPlayer, w, -1f);
			}
			NPC.velocity = Vector2.Zero;
			NPC.TargetClosest();
			if (NPC.ai[0] > 0f) {
				float u = 1f - MathHelper.Clamp(NPC.ai[0] / ShockwaveChargeVisualTicks, 0f, 1f);
				float lx = Utils.GetLerpValue(0f, 1f, u, clamped: true);
				Lighting.AddLight(NPC.Center, 0.48f + 0.5f * lx, 0.78f + 0.42f * lx, 1f);
				if (!Main.dedServ && Main.rand.NextBool(3)) {
					Dust.NewDustPerfect(
						NPC.Center + Main.rand.NextVector2Circular(NPC.width * 0.32f, NPC.height * 0.22f),
						Main.rand.NextBool(2) ? DustID.IceTorch : DustID.Snow,
						Main.rand.NextVector2Circular(0.35f, 0.35f),
						90,
						Color.Lerp(Color.White, new Color(200, 240, 255), Main.rand.NextFloat(0.15f, 0.45f)),
						Main.rand.NextFloat(0.75f, 1.15f));
				}
			}
			else if (!Main.dayTime)
				Lighting.AddLight(NPC.Center, 0.22f, 0.48f, 0.72f);

			if (NPC.ai[0] > 0f) {
				float u = 1f - MathHelper.Clamp(NPC.ai[0] / ShockwaveChargeVisualTicks, 0f, 1f);
				NPC.scale = 1f + 0.068f * MathF.Sin(u * MathHelper.Pi);
			}
			else if (NPC.scale != 1f)
				NPC.scale = 1f;

			if (Main.netMode != NetmodeID.MultiplayerClient && NPC.ai[2] >= spawnFrames) {
				if (_ambientWispCooldown <= 0) {
					int inboundType = ModContent.ProjectileType<IceAltarInboundWisp>();
					int sameAltar = 0;
					for (int i = 0; i < Main.maxProjectiles; i++) {
						Projectile p = Main.projectile[i];
						if (p.active && p.type == inboundType && (int)p.ai[0] == NPC.whoAmI)
							sameAltar++;
					}
					if (sameAltar < 11) {
						const float maxAmbientRadius = 112f;
						float ang = Main.rand.NextFloat() * MathHelper.TwoPi;
						float dist = Main.rand.NextFloat(28f, maxAmbientRadius - 4f);
						Vector2 offset = ang.ToRotationVector2() * dist + Main.rand.NextVector2Circular(12f, 12f);
						float len = offset.Length();
						if (len > maxAmbientRadius)
							offset *= maxAmbientRadius / Math.Max(len, 0.01f);
						Vector2 start = NPC.Center + offset;
						Projectile.NewProjectile(NPC.GetSource_FromThis(), start, Vector2.Zero, inboundType, 0, 0f, Main.myPlayer, NPC.whoAmI);
					}
					_ambientWispCooldown = Main.rand.Next(22, 62);
				}
				else
					_ambientWispCooldown--;

				if (shockwaveIntervalTicks <= 0)
					shockwaveIntervalTicks = Main.rand.Next(MinShockwaveIntervalTicks, MaxShockwaveIntervalTicks + 1);
				if (NPC.ai[0] > 0f) {
					NPC.ai[0] -= 1f;
					if (NPC.ai[0] <= 0f) {
						var src = NPC.GetSource_FromAI();
						int owner = NPC.target >= 0 && NPC.target < Main.maxPlayers ? NPC.target : Main.myPlayer;
						RollShockwavePhaseTicks(out int warnTicks, out int blastTicks);
						Projectile.NewProjectile(src, NPC.Center, Vector2.Zero, ModContent.ProjectileType<IceAltarShockwave>(), 32, 2f, owner, warnTicks, blastTicks);
						shockwaveIntervalTicks = Main.rand.Next(MinShockwaveIntervalTicks, MaxShockwaveIntervalTicks + 1);
						shockwaveTimer = 0;
						NPC.netUpdate = true;
					}
					else
						NPC.netUpdate = true;
				}
				else {
					shockwaveTimer++;
					if (shockwaveTimer >= shockwaveIntervalTicks) {
						NPC.ai[0] = ShockwaveChargeVisualTicks;
						shockwaveTimer = 0;
						NPC.netUpdate = true;
					}
				}
			}
		}

		public override bool CheckDead() {
			for (int i = 0; i < 12; i++) {
				Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(24f, 24f), DustID.IceTorch, Main.rand.NextVector2Circular(1.5f, 1.5f), 120, Color.White, 1.1f);
			}
			return true;
		}

		public override void HitEffect(NPC.HitInfo hit) {
			if (Main.netMode == NetmodeID.Server)
				return;
			int n = hit.Damage / 4 + 1;
			for (int i = 0; i < n; i++) {
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Ice, hit.HitDirection * 2, -1f, 100, default, 0.9f);
			}
		}

		/// <summary>单次冲击波「预警 + 喷发」总刻数约 24～30（0.4～0.5 秒 @60TPS），在此范围内随机分配两阶段时长。</summary>
		private static void RollShockwavePhaseTicks(out int warningTicks, out int blastTicks) {
			int totalTicks = Main.rand.Next(24, 31);
			blastTicks = totalTicks * 9 / 20;
			blastTicks = Math.Clamp(blastTicks, 8, 15);
			warningTicks = totalTicks - blastTicks;
			if (warningTicks < 10) {
				warningTicks = 10;
				blastTicks = totalTicks - warningTicks;
				blastTicks = Math.Clamp(blastTicks, 8, 15);
			}
		}

	}

	/// <summary>
	/// 祭坛周身双线拖尾：与 <see cref="FNInvincibleEffect"/> 相同运动，配色为亮蓝。
	/// </summary>
	public class IceAltarTwinLineRing : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;

		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 60;
		}

		public override void SetDefaults() {
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 4;
			Projectile.alpha = 0;
			Projectile.damage = 0;
			Projectile.light = 0.35f;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.scale = 1f;
		}

		private float timer;
		private float drawopacity;
		private int rChan;
		private int gChan;

		public override void AI() {
			Projectile.timeLeft = 4;
			timer++;
			Projectile.velocity = Vector2.Zero;

			int hostIndex = (int)Projectile.ai[0];
			if (hostIndex < 0 || hostIndex >= Main.maxNPCs) {
				Projectile.Kill();
				return;
			}
			NPC host = Main.npc[hostIndex];
			if (!host.active || host.type != ModContent.NPCType<IceCrystalAltar>()) {
				Projectile.Kill();
				return;
			}

			float dotX = 36f * MathF.Sin((timer / 30f + 0.4f + 0.6f * Projectile.ai[1]) * float.Pi);
			float dotY = 36f * Projectile.ai[1] * 0.5f * MathF.Cos((timer / 30f + 0.6f * Projectile.ai[1]) * float.Pi);
			Projectile.Center = host.Center + new Vector2(dotX, dotY + 8f + IceCrystalAltar.AltarTwinLineAnchorOffsetY);

			if (timer <= 60f)
				drawopacity += 1f / 60f;
			else
				drawopacity = 1f;

			rChan = 120 + 35 * (int)MathF.Sin(timer * MathHelper.Pi / 180f);
			gChan = 210 + 35 * (int)MathF.Sin(timer * MathHelper.Pi / 140f);
		}

		public override bool PreDraw(ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ {
			if (Main.dedServ)
				return false;
			Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/LineTrail").Value;
			var hi = new Color(rChan, gChan, 255) * drawopacity;
			var lo = new Color(20, 90, 220) * drawopacity;
			TrailMaker.ProjectileDrawTailByConstWidth(Projectile, trailtexture, Vector2.Zero, hi, lo, 15f, true);
			return false;
		}
	}

	/// <summary>
	/// 从霜星沿曲线飞向祭坛落点（风絮拖尾）；抵达后在落点爆开粒子并于其中生成 <see cref="IceCrystalAltar"/>。
	/// </summary>
	public class IceAltarSummonWisp : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;

		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 56;
		}

		public override void SetDefaults() {
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 900;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.damage = 0;
			Projectile.netImportant = true;
		}

		private Vector2 Target => new Vector2(Projectile.ai[0], Projectile.ai[1]);

		/// <summary>飞行贝塞尔起点；在 <see cref="OnSpawn"/> 记录（生成位置即霜星 <c>NPC.Center</c>）。NewProjectile 仅支持 3 个 ai，不能另传起点坐标。</summary>
		private Vector2 _flightCurveStartWorld;

		public override void OnSpawn(IEntitySource source) {
			_flightCurveStartWorld = Projectile.Center;
		}

		/// <summary>抵达销毁时在主机与联机客户端都会调用，在此播放落点爆开（权威端在 <see cref="AI"/> 里不再直接生成粒子）。</summary>
		public override void OnKill(int timeLeft) {
			if (Main.dedServ)
				return;
			Vector2 target = Target;
			Vector2 hitCenter = IceCrystalAltar.HitboxCenterFromSpawnTopLeft(target);
			bool summoned = Projectile.ai[2] == 1f;
			bool nearLanding = Vector2.Distance(Projectile.Center, hitCenter) < 44f || Vector2.Distance(Projectile.Center, target) < 22f;
			if (summoned || (nearLanding && timeLeft > 100))
				IceCrystalAltar.SpawnAltarAppearBurstAt(hitCenter);
		}

		public override void AI() {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			Vector2 target = Target;
			// ai[0..1] 祭坛左上角落点；localAI[0] 为沿贝塞尔参数 u∈[0,1]
			Vector2 start = _flightCurveStartWorld;
			float chord = Math.Max(Vector2.Distance(start, target), 6f);
			float du = 2.85f / (chord * 1.28f);
			float u = Projectile.localAI[0];
			Vector2 ctrl = WispFlightBezierControl(start, target);
			Vector2 onCurve = QuadBezier(start, ctrl, target, u) + WispFlightChordQuadraticWobble(start, target, u);
			Projectile.Center = onCurve;
			Projectile.velocity = Vector2.Zero;
			Projectile.localAI[0] = MathHelper.Clamp(u + du, 0f, 1f);

			if (u >= 0.992f || Vector2.Distance(Projectile.Center, target) < 6f) {
				Vector2 altarHitboxCenter = IceCrystalAltar.HitboxCenterFromSpawnTopLeft(target);
				Projectile.ai[2] = 1f;
				if (IceCrystalAltar.CountActiveAltars() < IceCrystalAltar.MaxConcurrentAltars) {
					NPC.NewNPC(Projectile.GetSource_FromThis(), (int)target.X, (int)target.Y, ModContent.NPCType<IceCrystalAltar>());
				}
				SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/SpawnDevice") with { Volume = 0.95f, Pitch = 0.05f }, altarHitboxCenter);
				Projectile.Kill();
				return;
			}
			Projectile.netUpdate = true;
		}

		public override bool PreDraw(ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ {
			if (Main.dedServ)
				return false;
			Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/WindTrail").Value;
			float lt = Main.netMode == NetmodeID.MultiplayerClient
				? 1f
				: MathHelper.Clamp(Projectile.localAI[0] * 1.85f, 0.25f, 1f);
			var cHi = new Color(120, 235, 255);
			var cLo = new Color(40, 140, 255);
			float w = 20f * lt;
			TrailMaker.ProjectileDrawTailByConstWidth(Projectile, trailtexture, Vector2.Zero, cHi, cLo, w, true);
			return false;
		}

		internal static Vector2 QuadBezier(Vector2 p0, Vector2 p1, Vector2 p2, float u) {
			float om = 1f - u;
			return om * om * p0 + 2f * om * u * p1 + u * u * p2;
		}

		/// <summary>由起点/终点种子决定的侧向控制点，使轨迹为不规则二次曲线（Bezier）。</summary>
		internal static Vector2 WispFlightBezierControl(Vector2 start, Vector2 target) {
			Vector2 mid = (start + target) * 0.5f;
			Vector2 delta = target - start;
			float len = delta.Length();
			if (len < 1f)
				return mid;
			Vector2 perp = new Vector2(-delta.Y, delta.X);
			perp.Normalize();
			float h = Frac01Flight(MathF.Sin(start.X * 0.0213f + start.Y * 0.0187f + target.X * 0.0131f + target.Y * 0.0249f) * 73891.41f);
			float h2 = Frac01Flight(len * 0.31f + h * MathHelper.TwoPi);
			float amp = MathHelper.Lerp(52f, 148f, h);
			if (h2 < 0.5f)
				amp = -amp;
			return mid + perp * amp;
		}

		/// <summary>沿弦方向的二次包络扰动（u(1-u)），叠加不规则正弦，避免像单段圆弧。</summary>
		internal static Vector2 WispFlightChordQuadraticWobble(Vector2 start, Vector2 target, float u) {
			Vector2 delta = target - start;
			float len = delta.Length();
			if (len < 1f)
				return Vector2.Zero;
			Vector2 tan = delta / len;
			float q = u * (1f - u) * 4f;
			float h = Frac01Flight(MathF.Sin(start.X * 0.015f + target.Y * 0.019f + len * 0.008f) * 31827.1f);
			float mag = MathHelper.Lerp(10f, 42f, h) * q;
			float waves = 2f + h * 5f;
			float wave = MathF.Sin(u * MathHelper.Pi * waves + h * 11.17f);
			return tan * (mag * wave);
		}

		internal static float Frac01Flight(float x) => x - MathF.Floor(x);
	}

	/// <summary>
	/// 祭坛存在时从附近随机位置飘向视觉中心：整体为一段柔和二次曲线，弦向侧摆仅 1～2 个波瓣，无高频缠绕；末段<strong>直线</strong>贴向中心后短渐隐。弹幕仅 <c>localAI[0..1]</c> 可用；步进尺度与侧摆周期由 <see cref="Projectile.identity"/> 确定性推导。
	/// </summary>
	public class IceAltarInboundWisp : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;

		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 52;
		}

		public override void SetDefaults() {
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 160;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.damage = 0;
			Projectile.netImportant = true;
		}

		private Vector2 _curveStartWorld;
		/// <summary>渐隐剩余刻数（存于 <see cref="Projectile.ai"/>[1]），&gt;0 时不再推进曲线，仅倒计时直至消失。</summary>
		private const int InboundFadeOutTicks = 17;
		/// <summary>贴向祭坛中心时小于该距离（像素）则开始渐隐。</summary>
		private const float InboundArriveCenterDist = 8.5f;

		/// <summary>tML 弹幕 <c>localAI</c> 仅长度为 2；原存于 [2][3] 的曲线步进系数与侧摆周期改为由 identity 推导，全端一致。</summary>
		private static float InboundDuChordScaleFromIdentity(int identity) =>
			MathHelper.Lerp(0.78f, 1.42f, IceAltarSummonWisp.Frac01Flight(identity * 0.173318f));

		private static float InboundSwayCyclesFromIdentity(int identity) => 1f + (identity & 1);

		public override void OnSpawn(IEntitySource source) {
			_curveStartWorld = Projectile.Center;
			// 曲线走完绝大部分路径，剩余由直线贴向中心完成（仅使用 localAI[0]=u、localAI[1]=uEnd）
			Projectile.localAI[1] = Main.rand.NextFloat(0.86f, 0.97f);
			Projectile.ai[1] = 0f;
			Projectile.ai[2] = 0f;
			Projectile.timeLeft = Main.rand.Next(64, 148);
		}

		/// <summary>较召唤风絮更小的单侧弯控制点，避免大弧线叠多层抖动。</summary>
		private static Vector2 InboundMildBezierControl(Vector2 start, Vector2 target) {
			Vector2 mid = (start + target) * 0.5f;
			Vector2 delta = target - start;
			float len = delta.Length();
			if (len < 1f)
				return mid;
			Vector2 perp = new Vector2(-delta.Y, delta.X);
			perp.Normalize();
			float h = IceAltarSummonWisp.Frac01Flight(MathF.Sin(start.X * 0.019f + start.Y * 0.017f + target.X * 0.014f + target.Y * 0.02f) * 62831f);
			float h2 = IceAltarSummonWisp.Frac01Flight(len * 0.27f + h * 3.7f);
			float amp = MathHelper.Lerp(16f, 38f, h);
			if (h2 < 0.5f)
				amp = -amp;
			return mid + perp * amp;
		}

		/// <summary>沿弦法向的正弦摆动，<paramref name="swayCycles"/> 为 1 或 2（整段路径上最多 1～2 次侧向拐弯感）。</summary>
		private static Vector2 InboundLimitedPerpSway(Vector2 start, Vector2 target, float u, float swayCycles, int identity) {
			Vector2 delta = target - start;
			float len = delta.Length();
			if (len < 1f)
				return Vector2.Zero;
			Vector2 perp = new Vector2(-delta.Y, delta.X);
			perp.Normalize();
			float envelope = u * (1f - u) * 4f;
			float c = MathHelper.Clamp(swayCycles, 1f, 2f);
			float wave = MathF.Sin(u * MathHelper.Pi * c + identity * 0.37f);
			float mag = MathHelper.Lerp(9f, 21f, IceAltarSummonWisp.Frac01Flight(identity * 0.618f)) * envelope;
			return perp * (wave * mag);
		}

		public override void AI() {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			int hostIndex = (int)Projectile.ai[0];
			if (hostIndex < 0 || hostIndex >= Main.maxNPCs) {
				Projectile.Kill();
				return;
			}
			NPC host = Main.npc[hostIndex];
			if (!host.active || host.type != ModContent.NPCType<IceCrystalAltar>()) {
				Projectile.Kill();
				return;
			}

			if (Projectile.ai[1] > 0f) {
				Projectile.ai[1] -= 1f;
				if (Projectile.ai[1] <= 0f) {
					Projectile.Kill();
					return;
				}
				Projectile.timeLeft = Math.Max(Projectile.timeLeft, 4);
				Projectile.netUpdate = true;
				return;
			}

			Vector2 target = host.Center + new Vector2(0f, IceCrystalAltar.AltarDrawGfxOffY);

			// 曲线阶段结束后：沿指向祭坛中心的方向直线靠近，拖尾朝向与之一致后再渐隐
			if (Projectile.ai[2] >= 1f) {
				Projectile.localAI[0] = 1f;
				Vector2 to = target - Projectile.Center;
				float dist = to.Length();
				Projectile.timeLeft = Math.Max(Projectile.timeLeft, 28);
				if (dist < InboundArriveCenterDist) {
					Projectile.Center = target;
					Projectile.ai[2] = 0f;
					Projectile.ai[1] = InboundFadeOutTicks;
					Projectile.timeLeft = Math.Max(Projectile.timeLeft, InboundFadeOutTicks + 3);
					Projectile.netUpdate = true;
					return;
				}
				Vector2 dir = to / dist;
				float step = MathHelper.Clamp(dist * 0.24f, 3.2f, 11f);
				Projectile.Center += dir * step;
				Projectile.velocity = Vector2.Zero;
				Projectile.netUpdate = true;
				return;
			}

			Vector2 start = _curveStartWorld;
			float chord = Math.Max(Vector2.Distance(start, target), 8f);
			float du = InboundDuChordScaleFromIdentity(Projectile.identity) / (chord * 1.12f);
			float u = Projectile.localAI[0];
			float uEnd = Projectile.localAI[1];
			Vector2 ctrl = InboundMildBezierControl(start, target);
			Vector2 onCurve = IceAltarSummonWisp.QuadBezier(start, ctrl, target, u)
				+ InboundLimitedPerpSway(start, target, u, InboundSwayCyclesFromIdentity(Projectile.identity), Projectile.identity);
			Projectile.Center = onCurve;
			Projectile.velocity = Vector2.Zero;

			float newU = MathHelper.Clamp(u + du, 0f, 1f);
			Projectile.localAI[0] = newU;
			if (newU >= uEnd - 0.0005f) {
				Projectile.localAI[0] = uEnd;
				Projectile.ai[2] = 1f;
				Projectile.timeLeft = Math.Max(Projectile.timeLeft, 32);
				Projectile.netUpdate = true;
				return;
			}
			if (Projectile.timeLeft <= 4) {
				Projectile.ai[2] = 1f;
				Projectile.timeLeft = Math.Max(Projectile.timeLeft, 32);
				Projectile.netUpdate = true;
				return;
			}
			Projectile.netUpdate = true;
		}

		public override bool PreDraw(ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ {
			if (Main.dedServ)
				return false;
			Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/WindTrail").Value;
			bool finalRunIn = Projectile.ai[2] >= 1f && Projectile.ai[1] <= 0f;
			float u = finalRunIn ? 1f : Projectile.localAI[0];
			float uEnd = Math.Max(Projectile.localAI[1], 0.15f);
			float headFade = Utils.GetLerpValue(0f, 0.1f, u, clamped: true);
			float tailFade = finalRunIn
				? 1f
				: Utils.GetLerpValue(uEnd, uEnd - Math.Max(0.14f, uEnd * 0.32f), u, clamped: true);
			float fade = headFade * tailFade;
			float lifeFade = Projectile.ai[1] > 0f
				? MathHelper.Clamp(Projectile.ai[1] / InboundFadeOutTicks, 0f, 1f)
				: 1f;
			fade *= lifeFade;
			float lt = MathHelper.Clamp(u * 1.7f, 0.2f, 1f) * lifeFade;
			var cHi = new Color(120, 235, 255) * fade;
			var cLo = new Color(40, 140, 255) * fade;
			float w = 19f * lt * (0.88f + 0.12f * fade);
			TrailMaker.ProjectileDrawTailByConstWidth(Projectile, trailtexture, Vector2.Zero, cHi, cLo, w, true);
			return false;
		}
	}

	/// <summary>
	/// 祭坛脉冲：先由外向内收缩的预警亮线（无伤害，弱扭曲），再在喷发阶段于 <see cref="ShockwaveMaxRadius"/> 内叠多层气雾（仅 <c>Aerosol/Aerosol00</c>～<c>Aerosol03</c>，黑底加法混合），并叠加自中心外扩的亮蓝圆环（纯顶点色）；喷发伤害结算后仍可延长纯视觉时间（<see cref="BlastVisualExtraTicks"/>），圆环与气雾扩张为 ease-out、透明度线性降至零。单次预警+喷发刻数仍由生成时随机（总时长约 0.4～0.5 秒量级）。范围内玩家受到少量伤害并有 <see cref="FreezeChance"/> 概率获得 <see cref="BuffID.Frozen"/>。预警环粒子仍在寿命末段渐隐外散。
	/// </summary>
	public class IceAltarShockwave : ModProjectile, IIceAltarWarpDraw
	{
		public override string Texture => ArknightsMod.noTexture;

		private const int AnnulusPoints = 96;
		/// <summary>冲击波作用范围；气雾贴图<strong>中心</strong>相对锚点的偏移亦不得超出此距离。</summary>
		private const float ShockwaveMaxRadius = 210f;
		private const float ShockwaveStartRadius = 40f;
		/// <summary>首次命中附加寒冷时长；寒冷本身不限制行动，作为下一次命中转冻结的前置。</summary>
		private const int ChilledOnFirstHitTicks = 180;
		/// <summary>玩家已处于寒冷时，再次被冲击波命中则冻结的时长。</summary>
		private const int FrozenOnChilledHitTicks = 60;
		/// <summary>由祭坛生成时经 ai[0]/ai[1] 写入，每发冲击波不同。</summary>
		private int _warningDurationTicks = 14;
		private int _blastDurationTicks = 12;
		private float _shockwaveRadiusStep;
		/// <summary>冲击波 Dust 最大存活刻数（与预警+喷发+渐隐对齐，上限约 0.5 秒）。</summary>
		private int _dustMaxLifeTicks = 30;
		/// <summary>在 <see cref="_dustMaxLifeTicks"/> 末尾用于渐隐+外散的刻数。</summary>
		private const int ShockwaveDustFadeTicks = 10;
		/// <summary>喷发阶段辅佐冰霜 Dust 更短总寿命，避免与气雾尾段叠太久。</summary>
		private const int FrostBlastDustMaxLifeTicks = 16;
		/// <summary>喷发辅佐 Dust 渐隐段刻数（略短于预警环，消失更利落）。</summary>
		private const int FrostBlastDustFadeTicks = 6;
		/// <summary>喷发辅佐 Dust 超过寿命后强制清除前的宽限刻数。</summary>
		private const int FrostBlastDustForceCullGraceTicks = 5;
		/// <summary>喷发「伤害已结算」后额外保留的纯视觉刻数（圆环/气雾渐隐、扩张减速），略加长便于尾段多帧低不透明度渐隐。</summary>
		private const int BlastVisualExtraTicks = 72;
		/// <summary>弹幕仅有 ai[0..2]。预警阶段 ai[0] 为收缩半径；进入喷发时改为 &gt;= 本哨兵（与最大半径 300 不冲突）。</summary>
		private const float PhaseBlastAi0Marker = 5000f;

		private static bool IsBlastPhase(float ai0) => ai0 >= PhaseBlastAi0Marker;

		/// <inheritdoc />
		public bool IceAltarWarpMaskActive => !IsBlastPhase(Projectile.ai[0]);

		private readonly List<(int dustIndex, ulong spawnedAt, bool blastShortLife)> _trackedShockDust = new();

		/// <summary>喷发气雾：<c>Aerosol/Aerosol00</c>～<c>Aerosol03</c>；客户端首次绘制前延迟加载，避免纯服务端或加载顺序导致异常。</summary>
		private const int AerosolTextureCount = 4;
		private static Texture2D[] _aerosolTextures;
		/// <summary>喷发阶段首帧已刷过冰霜爆发，避免每帧重复。</summary>
		private bool _frostAssistBlastBurstSpawned;

		/// <summary>气雾整体尺寸系数（与每层随机缩放、<see cref="MistLayerSpriteBoost"/> 相乘）。</summary>
		private const float MistDrawGlobalScaleMul = 1f;
		/// <summary>相对原先过小的补偿，与随机层缩放相乘。</summary>
		private const float MistLayerSpriteBoost = 1.32f;
		/// <summary>气雾渐隐专用：计算可见度时在 <see cref="Projectile.timeLeft"/> 上等效增加若干刻，补偿 <c>layerWeight×mistAlphaPeak</c> 使尾部更早低于可见阈值；扩张与圆环仍共用真实 <c>timeProgress</c>。</summary>
		private const int MistVisualOpacityBiasTicks = 14;
		/// <summary>黑底亮雾：<c>SourceAlpha×One</c> 加法叠化；黑像素不增亮故底不显露，仅用顶点 Alpha 控制雾强（略低即半透明感）。</summary>
		private static readonly BlendState MistSoftAdditiveBlend = new BlendState {
			ColorSourceBlend = Blend.SourceAlpha,
			ColorDestinationBlend = Blend.One,
			ColorBlendFunction = BlendFunction.Add,
			AlphaSourceBlend = Blend.One,
			AlphaDestinationBlend = Blend.One,
			AlphaBlendFunction = BlendFunction.Add,
		};

		public override void SetStaticDefaults() {
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2400;
		}

		/// <summary>喷发视觉总刻数分母（在喷发首帧写入 <see cref="Projectile.localAI"/>[1]，避免联机 <see cref="Projectile.timeLeft"/> 不同步时 tNorm 错误）。</summary>
		private const float BlastVisualDurationLatchSentinel = 0.5f;

		public override void SetDefaults() {
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 2400;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.damage = 0;
			Projectile.netImportant = true;
		}

		private Vector2 Anchor => new Vector2(Projectile.ai[1], Projectile.ai[2]);

		public override void OnSpawn(IEntitySource source) {
			_warningDurationTicks = Math.Clamp((int)Projectile.ai[0], 10, 22);
			_blastDurationTicks = Math.Clamp((int)Projectile.ai[1], 8, 15);
			_shockwaveRadiusStep = (ShockwaveMaxRadius - ShockwaveStartRadius) / _warningDurationTicks;
			int totalAnim = _warningDurationTicks + _blastDurationTicks;
			_dustMaxLifeTicks = Math.Min(30, totalAnim + ShockwaveDustFadeTicks);

			float ax = Projectile.Center.X;
			float ay = Projectile.Center.Y + IceCrystalAltar.AltarDrawGfxOffY;
			Projectile.ai[1] = ax;
			Projectile.ai[2] = ay;
			Projectile.Center = new Vector2(ax, ay);
			Projectile.ai[0] = ShockwaveMaxRadius;
			Projectile.localAI[0] = Projectile.ai[0];
			_frostAssistBlastBurstSpawned = false;
		}

		public override void AI() {
			CullShockDust();
			SpawnShockwaveFrostAssistParticles();
			Projectile.Center = Anchor;
			if (IsBlastPhase(Projectile.ai[0]) && Projectile.localAI[1] < BlastVisualDurationLatchSentinel) {
				// 锁存首帧剩余刻数作分母，供 PreDraw 渐隐与扩张；联机以同步后的 timeLeft 为准
				Projectile.localAI[1] = Math.Max(Projectile.timeLeft, 1);
				Projectile.netUpdate = true;
			}
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				if (!IsBlastPhase(Projectile.ai[0])) {
					Projectile.ai[0] -= _shockwaveRadiusStep;
					if (Projectile.ai[0] <= ShockwaveStartRadius) {
						Projectile.ai[0] = PhaseBlastAi0Marker;
						Projectile.timeLeft = _blastDurationTicks + BlastVisualExtraTicks;
						Projectile.localAI[1] = Projectile.timeLeft;
						ApplyBlastHit();
					}
				}
				Projectile.localAI[0] = Projectile.ai[0];
			}
			else {
				if (!IsBlastPhase(Projectile.ai[0])) {
					Projectile.localAI[0] -= _shockwaveRadiusStep;
					if (Projectile.localAI[0] < ShockwaveStartRadius)
						Projectile.localAI[0] = ShockwaveStartRadius;
				}
				else
					Projectile.localAI[0] = Projectile.ai[0];
			}
			Projectile.netUpdate = true;

			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;
		}

		public override void OnKill(int timeLeft) {
			// 仅停止追踪，不瞬间清除粒子，避免“啪一下没了”；余下由原版 Dust 或已开始的渐隐自然结束
			_trackedShockDust.Clear();
			base.OnKill(timeLeft);
		}

		private void RegisterShockDust(Dust d, bool blastShortLife = false) {
			if (Main.dedServ || d.dustIndex < 0 || d.dustIndex >= Main.maxDust)
				return;
			_trackedShockDust.Add((d.dustIndex, Main.GameUpdateCount, blastShortLife));
		}

		private static int PickFrostAssistDustType() =>
			Main.rand.Next(6) switch {
				0 or 1 or 2 => DustID.Snow,
				3 => DustID.Ice,
				4 => DustID.IceTorch,
				_ => DustID.BlueTorch,
			};

		/// <summary>预警环与喷发冲击沿环、区内生成冰霜系 Dust，辅佐气雾与圆环（仅客户端）。</summary>
		private void SpawnShockwaveFrostAssistParticles() {
			if (Main.dedServ || !Projectile.active || Main.gamePaused)
				return;

			Vector2 c = Anchor;
			Color FrostTint(float t) => Color.Lerp(Color.White, new Color(200, 232, 255), MathHelper.Clamp(t, 0f, 1f));

			if (IsBlastPhase(Projectile.ai[0])) {
				float denom = Projectile.localAI[1] >= BlastVisualDurationLatchSentinel
					? Projectile.localAI[1]
					: (_blastDurationTicks + BlastVisualExtraTicks);
				float totalVisual = Math.Max(1f, denom);
				float timeProgress = 1f - Projectile.timeLeft / totalVisual;
				timeProgress = MathHelper.Clamp(timeProgress, 0f, 1f);
				float easeExpand = 1f - (1f - timeProgress) * (1f - timeProgress);
				float globalVis = MathHelper.Clamp(1f - timeProgress, 0f, 1f);
				float ringR = MathHelper.Lerp(ShockwaveStartRadius * 0.82f, ShockwaveMaxRadius - 8f, easeExpand);

				if (!_frostAssistBlastBurstSpawned) {
					_frostAssistBlastBurstSpawned = true;
					int spokes = Main.rand.Next(20, 32);
					for (int n = 0; n < spokes; n++) {
						float ang = MathHelper.TwoPi * (n + Main.rand.NextFloat(-0.12f, 0.12f)) / spokes;
						Vector2 dir = ang.ToRotationVector2();
						float rad = ringR * Main.rand.NextFloat(0.55f, 1.02f);
						Vector2 pos = c + dir * rad;
						int tid = PickFrostAssistDustType();
						Dust d = Dust.NewDustPerfect(pos, tid, dir * Main.rand.NextFloat(1f, 3.6f), 35, FrostTint(Main.rand.NextFloat(0.12f, 0.48f)), Main.rand.NextFloat(0.8f, 1.4f));
						d.noGravity = Main.rand.NextBool(4);
						d.fadeIn = 0.12f;
						RegisterShockDust(d, blastShortLife: true);
					}
					for (int n = 0; n < Main.rand.Next(10, 18); n++) {
						Vector2 pos = c + Main.rand.NextVector2Circular(ShockwaveMaxRadius * 0.42f, ShockwaveMaxRadius * 0.42f);
						Dust d = Dust.NewDustPerfect(pos, Main.rand.NextBool(3) ? DustID.Snow : DustID.IceTorch, Main.rand.NextVector2Circular(0.8f, 0.8f), 55, FrostTint(Main.rand.NextFloat(0.08f, 0.4f)), Main.rand.NextFloat(0.55f, 1.05f));
						d.noGravity = Main.rand.NextBool(3);
						d.fadeIn = 0.2f;
						RegisterShockDust(d, blastShortLife: true);
					}
				}

				int perTick = 2 + Main.rand.Next(0, 3 + (int)(globalVis * 4f));
				for (int k = 0; k < perTick; k++) {
					float ang = Main.rand.NextFloat() * MathHelper.TwoPi;
					Vector2 dir = ang.ToRotationVector2();
					Vector2 pos = c + dir * ringR * Main.rand.NextFloat(0.9f, 1.1f);
					int tid = PickFrostAssistDustType();
					Dust d = Dust.NewDustPerfect(pos, tid, dir * Main.rand.NextFloat(0.35f, 1.9f) + Main.rand.NextVector2Circular(0.15f, 0.15f), 70, FrostTint(Main.rand.NextFloat(0.1f, 0.42f)), Main.rand.NextFloat(0.6f, 1.15f));
					d.noGravity = Main.rand.NextBool(5);
					d.fadeIn = 0.08f;
					RegisterShockDust(d, blastShortLife: true);
				}
				if (Main.rand.NextBool(3)) {
					Vector2 pos = c + Main.rand.NextVector2Circular(ringR * 0.9f, ringR * 0.9f);
					Dust sd = Dust.NewDustPerfect(pos, DustID.Snow, Main.rand.NextVector2Circular(0.35f, 0.35f), 95, Color.White * 0.85f, Main.rand.NextFloat(0.45f, 0.9f));
					RegisterShockDust(sd, blastShortLife: true);
				}
				return;
			}

			_frostAssistBlastBurstSpawned = false;
			float r = Main.netMode == NetmodeID.MultiplayerClient ? Math.Min(Projectile.ai[0], Projectile.localAI[0]) : Projectile.ai[0];
			if (r < ShockwaveStartRadius + 3f)
				return;
			if (Main.rand.NextBool(5))
				return;

			float wAng = Main.rand.NextFloat() * MathHelper.TwoPi;
			Vector2 wDir = wAng.ToRotationVector2();
			Vector2 wPos = c + wDir * (r + Main.rand.NextFloat(-8f, 6f));
			Dust wd = Dust.NewDustPerfect(wPos, DustID.IceTorch, wDir * Main.rand.NextFloat(-0.5f, 0.2f) + Main.rand.NextVector2Circular(0.25f, 0.25f), 105, FrostTint(Main.rand.NextFloat(0.18f, 0.5f)), Main.rand.NextFloat(0.7f, 1.15f));
			wd.noGravity = true;
			wd.fadeIn = 0.22f;
			RegisterShockDust(wd);
			if (Main.rand.NextBool(4)) {
				Dust sd = Dust.NewDustPerfect(wPos + Main.rand.NextVector2Circular(5f, 5f), DustID.Snow, wDir * 0.15f + Main.rand.NextVector2Circular(0.4f, 0.4f), 90, Color.White, Main.rand.NextFloat(0.45f, 0.85f));
				sd.fadeIn = 0.15f;
				RegisterShockDust(sd);
			}
		}

		private void CullShockDust() {
			if (Main.dedServ || _trackedShockDust.Count == 0)
				return;
			ulong now = Main.GameUpdateCount;
			Vector2 anchor = Anchor;

			for (int i = _trackedShockDust.Count - 1; i >= 0; i--) {
				(int idx, ulong born, bool blastShort) = _trackedShockDust[i];
				if (idx < 0 || idx >= Main.maxDust) {
					_trackedShockDust.RemoveAt(i);
					continue;
				}
				ref Dust dust = ref Main.dust[idx];
				if (!dust.active) {
					_trackedShockDust.RemoveAt(i);
					continue;
				}

				int maxLife = blastShort ? FrostBlastDustMaxLifeTicks : _dustMaxLifeTicks;
				int fadeTicks = blastShort ? FrostBlastDustFadeTicks : ShockwaveDustFadeTicks;
				int fadeStart = Math.Max(0, maxLife - fadeTicks);
				float fadeDenomLocal = Math.Max(1f, fadeTicks - 1);
				int age = (int)(now - born);

				// 渐隐段：提高 alpha（更透明）、略缩小、沿径向轻微外飘
				if (age >= fadeStart && age < maxLife) {
					float ft = (age - fadeStart) / fadeDenomLocal;
					ft = MathHelper.Clamp(ft, 0f, 1f);
					float ease = 1f - (float)Math.Pow(1f - ft, blastShort ? 1.35 : 1.65);
					int alphaStep = (blastShort ? 14 : 10) + (int)(ease * (blastShort ? 22 : 14));
					dust.alpha = Math.Min(255, dust.alpha + alphaStep);
					dust.scale *= MathHelper.Lerp(0.985f, blastShort ? 0.88f : 0.94f, ease);
					Vector2 radial = dust.position - anchor;
					float lenSq = radial.LengthSquared();
					if (lenSq > 64f) {
						float inv = 0.18f / MathF.Sqrt(lenSq);
						dust.velocity += radial * inv;
					}
					else if (lenSq > 0.01f)
						dust.velocity += Vector2.Normalize(radial) * (0.06f + 0.1f * ease);
					dust.velocity *= MathHelper.Lerp(1f, 0.985f, ease);
				}

				// 寿命末尾：仅在已足够透明后回收；若仍偏实色则再推一两帧渐隐，超时再强制结束以免卡死
				if (age >= maxLife) {
					int grace = blastShort ? FrostBlastDustForceCullGraceTicks : 12;
					bool forceEnd = age >= maxLife + grace;
					if (!forceEnd && (dust.alpha < 245 || dust.scale > 0.12f)) {
						dust.alpha = Math.Min(255, dust.alpha + (blastShort ? 38 : 28));
						dust.scale *= blastShort ? 0.76f : 0.82f;
						Vector2 radial = dust.position - anchor;
						if (radial.LengthSquared() > 0.01f)
							dust.velocity += Vector2.Normalize(radial) * 0.12f;
					}
					else {
						dust.active = false;
						_trackedShockDust.RemoveAt(i);
					}
				}
			}
		}

		private void ApplyBlastHit() {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;
			for (int p = 0; p < Main.maxPlayers; p++) {
				Player plr = Main.player[p];
				if (!plr.active || plr.dead || plr.ghost)
					continue;
				if (Vector2.Distance(plr.Center, Anchor) > ShockwaveMaxRadius)
					continue;
				plr.buffImmune[BuffID.Chilled] = false;
				plr.buffImmune[BuffID.Frozen] = false;
				if (plr.HasBuff(BuffID.Frozen))
					continue;
				if (plr.HasBuff(BuffID.Chilled)) {
					plr.AddBuff(BuffID.Frozen, FrozenOnChilledHitTicks);
					SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/Frozen") with { Volume = 0.85f, Pitch = Main.rand.NextFloat(-0.08f, 0.08f) }, plr.Center);
					continue;
				}
				plr.AddBuff(BuffID.Chilled, ChilledOnFirstHitTicks);
			}
		}

		private static float WarningRingOpacity(float r) {
			return MathHelper.Clamp(Utils.GetLerpValue(ShockwaveStartRadius * 0.4f, ShockwaveMaxRadius * 0.95f, r, clamped: true), 0.12f, 1f);
		}

		/// <summary>预警环收缩时，自当前半径上的采样点向祭坛贴图视觉中心（<see cref="IceCrystalAltar.AltarDrawGfxOffY"/>）聚拢的亮线。</summary>
		private const int WarningInwardLineCount = 44;
		/// <summary>聚拢线末端距视觉中心的最小留白（像素），避免完全糊在一点。</summary>
		private const float WarningInwardLineEndGapMin = 14f;
		private const float WarningInwardLineEndGapMax = 32f;

		private void DrawWarningInwardConvergenceLines(float ringRadius, float opacityDraw, Color cHi, Color cLo) {
			if (opacityDraw < 0.06f || ringRadius < ShockwaveStartRadius * 0.35f)
				return;
			Vector2 convergeWorld = Anchor + new Vector2(0f, IceCrystalAltar.AltarDrawGfxOffY);
			Texture2D lineTex = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/LineTrail").Value;
			if (lineTex == null || lineTex.Width < 1)
				return;
			float t = Main.GlobalTimeWrappedHourly;
			float stopGap = MathHelper.Lerp(WarningInwardLineEndGapMax, WarningInwardLineEndGapMin, opacityDraw);
			float idJ = (Projectile.identity % 1200) * 0.0007f;
			SpriteBatch sb = Main.spriteBatch;
			bool batchRestarted = false;
			try {
				try { sb.End(); } catch { }
				sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
				batchRestarted = true;
				int texW = lineTex.Width;
				int texH = lineTex.Height;
				var origin = new Vector2(0f, texH * 0.5f);
				for (int i = 0; i < WarningInwardLineCount; i++) {
					float ang = MathHelper.TwoPi * i / WarningInwardLineCount + t * 0.14f + idJ;
					ang += MathF.Sin(t * 1.45f + i * 0.48f) * 0.055f;
					Vector2 dir = ang.ToRotationVector2();
					Vector2 pOuter = Anchor + dir * ringRadius;
					Vector2 toConverge = convergeWorld - pOuter;
					float segLen = toConverge.Length();
					if (segLen < 10f)
						continue;
					Vector2 inward = toConverge / segLen;
					float lineLen = segLen - stopGap;
					lineLen = MathHelper.Clamp(lineLen, segLen * 0.22f, segLen - 5f);
					if (lineLen < 12f)
						continue;
					Vector2 startScr = pOuter - Main.screenPosition;
					float rot = inward.ToRotation();
					float thick = (2.2f + (i % 7) * 0.32f) * (0.72f + 0.38f * opacityDraw);
					float pulse = 0.85f + 0.15f * MathF.Sin(t * 2.4f + i * 0.31f);
					Color col = Color.Lerp(cHi, cLo, (float)(i % 9) / 9f * 0.5f) * (0.36f * opacityDraw * pulse);
					sb.Draw(lineTex, startScr, null, col, rot, origin, new Vector2(lineLen / texW, thick / texH), SpriteEffects.None, 0f);
				}
			}
			finally {
				if (batchRestarted) {
					try { sb.End(); } catch { }
					sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ {
			if (Main.dedServ)
				return false;
			if (IsBlastPhase(Projectile.ai[0])) {
				try {
					DrawBlastExpandingBlueRing();
				}
				catch {
					// 避免 BasicEffect / 顶点绘制异常导致整局崩溃
				}
				try {
					DrawBlastMistLayers();
				}
				catch {
					// 避免气雾批处理异常导致崩溃；确保批次恢复在 DrawBlastMistLayers 内部 finally
				}
				return false;
			}
			float r = Main.netMode == NetmodeID.MultiplayerClient ? Math.Min(Projectile.ai[0], Projectile.localAI[0]) : Projectile.ai[0];
			float opacityDraw = WarningRingOpacity(r);
			if (opacityDraw < 0.05f)
				return false;
			var c1 = new Color(255, 245, 180) * (0.55f * opacityDraw);
			var c2 = new Color(200, 230, 255) * (0.5f * opacityDraw);
			Texture2D wind = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/WindTrail").Value;
			float phase = Main.GlobalTimeWrappedHourly;
			TrailMaker.DrawCircleWindRibbon(wind, Anchor, r, 120, 22f * opacityDraw, c1, c2, true, zeroVertexAlpha: false, wobblePhase: phase);

			try {
				DrawWarningInwardConvergenceLines(r, opacityDraw, c1, c2);
			}
			catch {
				// 聚拢线与批次切换失败时跳过，避免拖尾整局绘制链
			}

			if (Main.rand.NextBool(5)) {
				float a = Main.rand.NextFloat() * MathHelper.TwoPi;
				Vector2 pos = Anchor + a.ToRotationVector2() * (r + Main.rand.NextFloat(-6f, 8f)) + Main.rand.NextVector2Circular(4f, 4f);
				Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, Vector2.Zero, 100, Color.Lerp(c1, c2, Main.rand.NextFloat()) * 0.75f, Main.rand.NextFloat(0.5f, 0.9f) * opacityDraw);
				d.noGravity = true;
				d.fadeIn = 0.4f;
				RegisterShockDust(d);
			}

			IceAltarWarpPostProcess.EnableIceAltarWarpThisFrame = true;
			if (opacityDraw > 0.12f)
				IceAltarWarpPostProcess.IceAltarWarpIntensity = Math.Max(IceAltarWarpPostProcess.IceAltarWarpIntensity, 0.05f + 0.04f * opacityDraw);

			return false;
		}

		private struct MistLayer {
			/// <summary>基础极角（弧度）。</summary>
			public float PhaseAng;
			/// <summary>在 <see cref="PhaseAng"/> 上叠加的固定角向错开（弧度），减轻层与层角度重合。</summary>
			public float AngSpread;
			/// <summary><c>[0,1)</c>；经幂次映射后落入外圈环带的径向位置。</summary>
			public float RadialT;
			/// <summary>沿切向的固定错开（像素），避免随机全方向偏移把雾拉回锚点附近叠成一团。</summary>
			public float StaggerTangent;
			/// <summary><c>[0,1)</c>；与喷发扩张进度叠加错开径向外推时机，减轻各片同步移动导致重叠。</summary>
			public float OutwardPhase;
			/// <summary>单层随机尺寸系数，仅与全局系数相乘得到<strong>等比</strong>缩放，避免非均匀拉伸。</summary>
			public float UniformScaleFactor;
			public float Opacity;
			public int TexVariant;
		}

		/// <summary>喷发圆环：纯顶点色三角带；径向厚度适中。扩张用 ease-out（前快后慢），整体透明度随时间线性降低至消失。</summary>
		private const int BlastRingSegments = 112;
		/// <summary>圆环径向厚度（像素）。</summary>
		private const float BlastRingBandWidth = 38f;

		private void DrawBlastExpandingBlueRing() {
			float denom = Projectile.localAI[1] >= BlastVisualDurationLatchSentinel
				? Projectile.localAI[1]
				: (_blastDurationTicks + BlastVisualExtraTicks);
			float totalVisual = Math.Max(1f, denom);
			// timeProgress：0=喷发视觉起点，1=终点（与 timeLeft 耗尽对齐）
			float timeProgress = 1f - Projectile.timeLeft / totalVisual;
			timeProgress = MathHelper.Clamp(timeProgress, 0f, 1f);
			// 扩张仍用 ease-out，与透明度解耦
			float easeExpand = 1f - (1f - timeProgress) * (1f - timeProgress);
			float rOuter = MathHelper.Lerp(ShockwaveStartRadius * 0.88f, ShockwaveMaxRadius + 16f, easeExpand);
			// 圆环整体不透明度：与剩余寿命线性（1→0）
			float intensity = MathHelper.Clamp(1f - timeProgress, 0f, 1f);
			DrawBlastBlueRingSolidAnnulus(rOuter, intensity);
		}

		/// <summary>使用 <see cref="BasicEffect"/> 绘制无纹理圆环，仅顶点 RGBA 渐变。</summary>
		private void DrawBlastBlueRingSolidAnnulus(float rOuter, float intensity) {
			if (Main.dedServ)
				return;

			float rInner = Math.Max(rOuter - BlastRingBandWidth, 6f);
			Vector2 c = Anchor;
			float f = MathHelper.Clamp(intensity, 0f, 1f);
			// 整体渐隐时同步缩放 RGB 与 A（类预乘），避免仅用 A 缩放时与非预乘 AlphaBlend 组合在极低 alpha 下整段「突然没了」
			var colInner = new Color(0, 0, 0, 0);
			var colMid = new Color(
				(byte)MathHelper.Clamp(110f * f, 0f, 255f),
				(byte)MathHelper.Clamp(220f * f, 0f, 255f),
				(byte)MathHelper.Clamp(255f * f, 0f, 255f),
				(byte)MathHelper.Clamp(120f * f, 0f, 255f));
			var colOuter = new Color(
				(byte)MathHelper.Clamp(220f * f, 0f, 255f),
				(byte)MathHelper.Clamp(252f * f, 0f, 255f),
				(byte)MathHelper.Clamp(255f * f, 0f, 255f),
				(byte)MathHelper.Clamp(245f * f, 0f, 255f));
			float rMid = MathHelper.Lerp(rInner, rOuter, 0.55f);

			var verts = new VertexPositionColor[BlastRingSegments * 12];
			int vi = 0;
			for (int i = 0; i < BlastRingSegments; i++) {
				int j = (i + 1) % BlastRingSegments;
				float ti = i / (float)BlastRingSegments;
				float tj = j / (float)BlastRingSegments;
				float angI = MathHelper.TwoPi * ti;
				float angJ = MathHelper.TwoPi * tj;
				Vector2 dI = angI.ToRotationVector2();
				Vector2 dJ = angJ.ToRotationVector2();
				Vector2 pIi = c + dI * rInner;
				Vector2 pIm = c + dI * rMid;
				Vector2 pIo = c + dI * rOuter;
				Vector2 pJi = c + dJ * rInner;
				Vector2 pJm = c + dJ * rMid;
				Vector2 pJo = c + dJ * rOuter;
				// 内层带：内缘全透明 → 中带
				verts[vi++] = Vpc(pIm, colMid);
				verts[vi++] = Vpc(pIi, colInner);
				verts[vi++] = Vpc(pJm, colMid);
				verts[vi++] = Vpc(pIi, colInner);
				verts[vi++] = Vpc(pJm, colMid);
				verts[vi++] = Vpc(pJi, colInner);
				// 外层带：中带 → 外缘亮边
				verts[vi++] = Vpc(pIo, colOuter);
				verts[vi++] = Vpc(pIm, colMid);
				verts[vi++] = Vpc(pJo, colOuter);
				verts[vi++] = Vpc(pIm, colMid);
				verts[vi++] = Vpc(pJo, colOuter);
				verts[vi++] = Vpc(pJm, colMid);
			}

			static VertexPositionColor Vpc(Vector2 p, Color col) =>
				new VertexPositionColor(new Vector3(p.X, p.Y, 0f), col);

			GraphicsDevice gd = Main.graphics?.GraphicsDevice;
			if (gd == null)
				return;

			BlendState oldBlend = gd.BlendState;
			RasterizerState oldRaster = gd.RasterizerState;
			DepthStencilState oldDepth = gd.DepthStencilState;
			SpriteBatch sb = Main.spriteBatch;
			try {
				try { sb.End(); } catch { }
				// 顶点色为「直通道」RGBA 时，用 NonPremultiplied 与整体缩放后的颜色一致，渐隐更均匀
				gd.BlendState = BlendState.NonPremultiplied;
				gd.RasterizerState = RasterizerState.CullNone;
				gd.DepthStencilState = DepthStencilState.None;

				Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, 0f, 1f);
				Matrix model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0f)) * Main.GameViewMatrix.ZoomMatrix;
				try {
					using (BasicEffect effect = new BasicEffect(gd)) {
						effect.VertexColorEnabled = true;
						effect.TextureEnabled = false;
						effect.World = model;
						effect.View = Matrix.Identity;
						effect.Projection = projection;
						foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
							pass.Apply();
							gd.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, verts.Length / 3);
						}
					}
				}
				catch {
					// BasicEffect / DrawUserPrimitives 失败时跳过圆环，避免崩溃
				}
			}
			finally {
				gd.BlendState = oldBlend;
				gd.RasterizerState = oldRaster;
				gd.DepthStencilState = oldDepth;
				try {
					sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
				}
				catch {
					// 若批次状态异常，不再二次抛出
				}
			}
		}

		/// <summary>tModLoader 在资源路径无效时常返回 1×1 占位贴图且不抛错。</summary>
		private static bool IsTmlMissingTexturePlaceholder(Texture2D t) {
			if (t == null || t.IsDisposed)
				return true;
			return t.Width == 1 && t.Height == 1;
		}

		/// <summary>仅从 <c>Aerosol/Aerosol00～03</c> 加载，不使用其它目录回退贴图。</summary>
		private static Texture2D TryRequestAerosolSlotOnly(int index, out string resolvedFromPath) {
			resolvedFromPath = null;
			string id = index.ToString("D2");
			string[] candidates = new[] {
				$"ArknightsMod/NPCs/Enemy/Chapter6/FrostNova/Aerosol/Aerosol{id}",
				$"ArknightsMod/Content/NPCs/Enemy/Chapter6/FrostNova/Aerosol/Aerosol{id}",
			};
			foreach (string path in candidates) {
				try {
					if (!ModContent.HasAsset(path))
						continue;
					Texture2D t = ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad).Value;
					if (!IsTmlMissingTexturePlaceholder(t)) {
						resolvedFromPath = path;
						return t;
					}
				}
				catch {
					// 尝试下一路径
				}
			}
			resolvedFromPath = "(未找到有效Aerosol)";
			return null;
		}

		private static void EnsureAerosolTexturesLoaded() {
			if (Main.dedServ)
				return;
			if (_aerosolTextures != null) {
				for (int i = 0; i < _aerosolTextures.Length; i++) {
					if (_aerosolTextures[i] == null || IsTmlMissingTexturePlaceholder(_aerosolTextures[i])) {
						_aerosolTextures = null;
						break;
					}
				}
			}
			if (_aerosolTextures != null && _aerosolTextures[0] != null)
				return;

			var arr = new Texture2D[AerosolTextureCount];
			bool allOk = true;
			for (int i = 0; i < AerosolTextureCount; i++) {
				arr[i] = TryRequestAerosolSlotOnly(i, out _);
				if (arr[i] == null)
					allOk = false;
			}
			_aerosolTextures = allOk ? arr : null;
		}

		/// <summary>将偏移限制在长度 ≤ <paramref name="maxLength"/>（用于中心点：整块精灵须落在圆内时取更小上限）。</summary>
		private static Vector2 ClampVector2Length(Vector2 v, float maxLength) {
			if (maxLength <= 0f)
				return Vector2.Zero;
			float lenSq = v.LengthSquared();
			float maxSq = maxLength * maxLength;
			if (lenSq <= maxSq || lenSq < 1e-8f)
				return v;
			return v * (maxLength / MathF.Sqrt(lenSq));
		}

		/// <summary>等比缩放后，精灵中心到最远角距离为 <paramref name="halfDiagonalAtScale"/> 时，中心允许距锚点的最大值。</summary>
		private static float MaxMistCenterOffsetLength(float halfDiagonalAtScale) {
			const float margin = 4f;
			return MathHelper.Max(0f, ShockwaveMaxRadius - margin - halfDiagonalAtScale);
		}

		/// <summary>即使贴图较大，也至少保留该「中心可偏移」半径，避免全部被夹到锚点叠成一点。</summary>
		private const float MistMinCenterSpreadBudget = 60f;
		/// <summary>气雾中心距锚点落在 <c>[maxLen×内缘, maxLen×外缘]</c> 的环带内，对齐扩张亮蓝环外围而非圆心。</summary>
		private const float MistAnnulusInnerFrac = 0.74f;
		private const float MistAnnulusOuterFrac = 0.99f;
		/// <summary>对 <see cref="MistLayer.RadialT"/> 做 <c>Pow(t, p)</c>；<c>p</c> 越小越靠环带外沿。</summary>
		private const float MistRadialAlongAnnulusPower = 0.28f;
		/// <summary>黄金角分角基础偏移（弧度），叠小抖动减轻角向重叠。</summary>
		private const float MistAngleJitterRad = 0.32f;
		/// <summary>喷发初期径向距离乘子（相对环带半径）；过低会把首帧雾全压在祭坛心附近，需与内缘环带配合。</summary>
		private const float MistOutwardRadialMulStart = 0.72f;
		/// <summary>扩张末段径向乘子上限（&gt;1 以顶满圆内夹紧，视觉外推更明显）。</summary>
		private const float MistOutwardRadialMulEnd = 1.18f;
		/// <summary>对 <c>timeProgress</c> 的 ease-out 指数：<c>1-(1-t)^p</c>；<c>p</c> 越大越早完成大部分外移（先快后慢越明显）。</summary>
		private const float MistOutwardEaseOutPower = 3.75f;
		/// <summary>每层在扩张进度上的错相幅度，使外推不同步从而拉开间距。</summary>
		private const float MistOutwardLayerEaseSpread = 0.26f;

		/// <summary>喷发阶段：多片气雾 <c>Aerosol</c>；SourceAlpha×One；不旋转贴图；与圆环共用锁存分母。</summary>
		private void DrawBlastMistLayers() {
			EnsureAerosolTexturesLoaded();
			if (_aerosolTextures == null || _aerosolTextures[0] == null)
				return;

			float denom = Projectile.localAI[1] >= BlastVisualDurationLatchSentinel
				? Projectile.localAI[1]
				: (_blastDurationTicks + BlastVisualExtraTicks);
			float totalVisual = Math.Max(1f, denom);
			float timeProgress = 1f - Projectile.timeLeft / totalVisual;
			timeProgress = MathHelper.Clamp(timeProgress, 0f, 1f);
			float easeExpand = 1f - (1f - timeProgress) * (1f - timeProgress);
			// 气雾径向「外冲」单独用更高次 ease-out（先快后慢），避免再叠一层 quad(easeExpand) 导致中段已贴边、位移不明显
			float mistOutwardDrive = 1f - MathF.Pow(1f - timeProgress, MistOutwardEaseOutPower);
			// 与圆环同用 linear intensity=timeLeft/total 时，气雾还乘 layerWeight 与较低 mistAlphaPeak，尾段会先于圆环「看不见」；此处仅抬高可见度曲线尾部以对齐圆环收尾
			float globalVis = MathHelper.Clamp(
				(Projectile.timeLeft + MistVisualOpacityBiasTicks) / totalVisual,
				0f, 1f);

			const int patchCount = 17;
			var layers = new MistLayer[patchCount];
			int seed = unchecked(Projectile.identity * 73856093 + Type * 19349663 + Projectile.whoAmI * 83492791);
			var prng = new Random(seed);
			float golden = MathHelper.Pi * (3f - MathF.Sqrt(5f));
			for (int i = 0; i < patchCount; i++) {
				float baseAng = (i * golden) % MathHelper.TwoPi;
				if (baseAng < 0f)
					baseAng += MathHelper.TwoPi;
				layers[i].PhaseAng = baseAng + (float)(prng.NextDouble() * 2f - 1f) * MistAngleJitterRad;
				layers[i].AngSpread = (float)(prng.NextDouble() * 0.5f - 0.25f);
				// 偏向环带外侧，减少「均匀随机」落在内圈导致视觉上挤在祭坛中心
				{
					double u = prng.NextDouble();
					layers[i].RadialT = 1f - (float)Math.Pow(u, 1.55);
				}
				layers[i].StaggerTangent = (float)(prng.NextDouble() * 26f - 13f);
				layers[i].OutwardPhase = (float)prng.NextDouble();
				layers[i].UniformScaleFactor = 0.36f + (float)prng.NextDouble() * 1.28f;
				layers[i].Opacity = 0.14f + (float)prng.NextDouble() * 0.42f;
				layers[i].TexVariant = prng.Next(AerosolTextureCount);
			}
			Array.Sort(layers, (a, b) => a.Opacity.CompareTo(b.Opacity));

			Vector2 screenAnchor = Anchor - Main.screenPosition;
			float time = Main.GlobalTimeWrappedHourly;

			SpriteBatch sb = Main.spriteBatch;
			bool mistBatchActive = false;
			try {
				try { sb.End(); } catch { }
				sb.Begin(SpriteSortMode.Deferred, MistSoftAdditiveBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
				mistBatchActive = true;
				for (int i = 0; i < patchCount; i++) {
					float wobbleDamp = 1f - timeProgress * 0.82f;
					float wobAmp = (3.2f + (i % 5) * 1.1f + layers[i].AngSpread * 2.6f) * wobbleDamp;
					Vector2 wobble = new Vector2(
						MathF.Sin(time * 1.85f + i * 0.93f + layers[i].PhaseAng * 0.5f),
						MathF.Cos(time * 1.52f + i * 0.71f + layers[i].RadialT * 6.28f)) * wobAmp;
					float breathe = 0.88f + 0.16f * MathF.Sin(time * 2.2f + i * 0.45f);
					float sizePulse = 1f + 0.11f * MathF.Sin(time * 1.55f + i * 0.62f);
					float baseMul = breathe * sizePulse * (0.58f + 0.62f * easeExpand) * MistLayerSpriteBoost * MistDrawGlobalScaleMul;
					// 等比缩放（单标量），禁止 Vector2 非均匀拉伸导致贴图变形
					float uniformScale = layers[i].UniformScaleFactor * baseMul;

					Texture2D tex = _aerosolTextures[layers[i].TexVariant % AerosolTextureCount];
					float hw = tex.Width * 0.5f;
					float hh = tex.Height * 0.5f;
					float halfDiagAtScale1 = MathF.Sqrt(hw * hw + hh * hh);
					const float fitMargin = 4f;
					float maxCorner = ShockwaveMaxRadius - fitMargin;
					if (halfDiagAtScale1 > 1e-4f) {
						float cornerReach = halfDiagAtScale1 * uniformScale;
						if (cornerReach > maxCorner)
							uniformScale = maxCorner / halfDiagAtScale1;
						// 为随机落点保留最小径向预算：必要时略缩小贴图，避免 maxCenterLen→0 全叠在锚点
						float maxScaleForSpread = (ShockwaveMaxRadius - fitMargin - MistMinCenterSpreadBudget) / halfDiagAtScale1;
						if (maxScaleForSpread > 1e-4f && uniformScale > maxScaleForSpread)
							uniformScale = maxScaleForSpread;
					}

					float cornerReachFinal = halfDiagAtScale1 * uniformScale;
					float maxCenterLen = MaxMistCenterOffsetLength(cornerReachFinal);
					float angWobble = 0.26f * MathF.Sin(time * 0.72f + i * 1.17f + layers[i].AngSpread * 4f);
					float effAng = layers[i].PhaseAng + layers[i].AngSpread + angWobble;
					float tRad = MathHelper.Clamp(layers[i].RadialT, 0f, 1f);
					float tAnn = MathF.Pow(tRad, MistRadialAlongAnnulusPower);
					float rMin = maxCenterLen * MistAnnulusInnerFrac;
					float rMax = maxCenterLen * MistAnnulusOuterFrac;
					float radial = MathHelper.Lerp(rMin, rMax, tAnn);
					float easeLayer = MathHelper.Clamp(
						mistOutwardDrive + (layers[i].OutwardPhase - 0.5f) * MistOutwardLayerEaseSpread,
						0f, 1f);
					float radialMul = MathHelper.Lerp(MistOutwardRadialMulStart, MistOutwardRadialMulEnd, easeLayer);
					radial *= radialMul;
					Vector2 dir = effAng.ToRotationVector2();
					Vector2 perp = new Vector2(-dir.Y, dir.X);
					Vector2 drift = dir * radial + perp * layers[i].StaggerTangent;
					float tangWander = (2.3f + (i % 7) * 0.35f) * MathF.Sin(time * 0.98f + i * 0.74f + layers[i].PhaseAng);
					Vector2 tang = perp * tangWander;
					// 抖动与切向摆动过大时，Clamp 会把整段向量压短，视觉上易被「吸回」锚点；先按可用半径预算缩放次要偏移
					float jitterBudget = MathHelper.Clamp(maxCenterLen * 0.14f, 6f, 38f);
					Vector2 secondary = wobble + tang;
					float secLen = secondary.Length();
					if (secLen > jitterBudget && secLen > 1e-4f)
						secondary *= jitterBudget / secLen;
					Vector2 centerOffsetWorld = ClampVector2Length(drift + secondary, maxCenterLen);
					Vector2 pos = screenAnchor + centerOffsetWorld;

					float layerWeight = 0.2f + layers[i].Opacity * 0.5f;
					float strength = MathHelper.Clamp(globalVis * layerWeight, 0f, 1f);
					// 加法雾：用较低 Alpha 减弱叠加强度（半透明感）；勿用预乘+InvSrcAlpha，否则黑底贴图会透出黑块
					const float mistAlphaPeak = 0.48f;
					byte a = (byte)MathHelper.Clamp(255f * strength * mistAlphaPeak, 8f, 130f);
					var tint = Color.Lerp(new Color(245, 250, 255), new Color(220, 235, 255), (float)(i % 5) / 5f * 0.35f);
					var col = new Color(tint.R, tint.G, tint.B, a);

					Vector2 origin = tex.Size() * 0.5f;
					sb.Draw(tex, pos, null, col, 0f, origin, uniformScale, SpriteEffects.None, 0f);
				}
			}
			finally {
				if (mistBatchActive) {
					try { sb.End(); } catch { }
				}
				try {
					sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
				}
				catch {
				}
			}

			if (Main.rand.NextBool(2))
				Lighting.AddLight(Anchor, 0.28f * globalVis, 0.36f * globalVis, 0.48f * globalVis);
		}

		public void DrawIceAltarWarp() {
			if (Main.dedServ || !Projectile.active || IsBlastPhase(Projectile.ai[0]))
				return;
			float r = Main.netMode == NetmodeID.MultiplayerClient ? Math.Min(Projectile.ai[0], Projectile.localAI[0]) : Projectile.ai[0];
			float opacity = WarningRingOpacity(r) * Utils.GetLerpValue(0f, 80f, r, clamped: true);
			if (opacity < 0.04f)
				return;

			Vector2 center = Anchor;
			GraphicsDevice device = Main.instance.GraphicsDevice;
			float inner = Math.Max(r - 72f, 8f);
			float outer = r + 40f;
			float time = Main.GlobalTimeWrappedHourly;

			Vector2[] outerPts = new Vector2[AnnulusPoints];
			Vector2[] innerPts = new Vector2[AnnulusPoints];
			for (int i = 0; i < AnnulusPoints; i++) {
				float t = i / (float)AnnulusPoints;
				float ang = MathHelper.TwoPi * t + time * 0.08f;
				float bend = 0.018f * (float)Math.Sin(ang * 6f - time * 5f) + 0.014f * (float)Math.Cos(ang * 14f + time * 3f);
				ang += bend;
				float radWobO = 1f + 0.04f * (float)Math.Sin(MathHelper.TwoPi * t * 7f - time * 4f) + 0.022f * (float)Math.Cos(MathHelper.TwoPi * t * 13f + time * 5f);
				float radWobI = 1f + 0.03f * (float)Math.Sin(MathHelper.TwoPi * t * 5f + time * 6f);
				outerPts[i] = center + ang.ToRotationVector2() * (outer * radWobO);
				innerPts[i] = center + ang.ToRotationVector2() * (inner * radWobI);
			}

			VertexPositionColor VertexFor(Vector2 worldPos, float baseAng, bool isOuter) {
				baseAng %= MathHelper.TwoPi;
				if (baseAng < 0f)
					baseAng += MathHelper.TwoPi;
				float wave = 0.72f + 0.28f * (float)Math.Sin(baseAng * 6f + time * 10f);
				float micro = 0.25f * (float)(Math.Sin(baseAng * 11f - time * 8f) * Math.Cos(baseAng * 7f + time * 5f));
				float dirAng = baseAng + MathHelper.PiOver2 + 0.035f * (float)Math.Sin(baseAng * 5f - time * 6f) + micro * 0.18f;
				dirAng %= MathHelper.TwoPi;
				if (dirAng < 0f)
					dirAng += MathHelper.TwoPi;
				float rEnc = dirAng / MathHelper.TwoPi;
				float gOuter = MathHelper.Clamp(0.52f * opacity * (0.65f + 0.35f * wave) * (0.85f + 0.15f * (float)Math.Sin(baseAng * 3f + time * 4f)), 0f, 1f);
				float gInner = gOuter * 0.42f;
				float g = isOuter ? gOuter : gInner;
				float rChannel = isOuter ? rEnc : rEnc * 0.97f;
				// 与 Entelechia DrawWarp 一致：世界坐标顶点，由 BasicEffect.World 做 -screenPosition 与缩放
				return new VertexPositionColor(new Vector3(worldPos.X, worldPos.Y, 0f), new Color(rChannel, g, 0f, 1f));
			}

			var verts = new VertexPositionColor[AnnulusPoints * 6];
			int vi = 0;
			for (int i = 0; i < AnnulusPoints; i++) {
				int j = (i + 1) % AnnulusPoints;
				float bi = MathHelper.TwoPi * (i / (float)AnnulusPoints) + time * 0.08f;
				float bj = MathHelper.TwoPi * (j / (float)AnnulusPoints) + time * 0.08f;
				VertexPositionColor oi = VertexFor(outerPts[i], bi, true);
				VertexPositionColor ii = VertexFor(innerPts[i], bi, false);
				VertexPositionColor oj = VertexFor(outerPts[j], bj, true);
				VertexPositionColor ij = VertexFor(innerPts[j], bj, false);
				verts[vi++] = oi;
				verts[vi++] = ii;
				verts[vi++] = oj;
				verts[vi++] = ii;
				verts[vi++] = oj;
				verts[vi++] = ij;
			}

			Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, 0f, 1f);
			Matrix model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0f)) * Main.GameViewMatrix.ZoomMatrix;
			using (BasicEffect effect = new BasicEffect(device)) {
				effect.VertexColorEnabled = true;
				effect.TextureEnabled = false;
				effect.World = model;
				effect.View = Matrix.Identity;
				effect.Projection = projection;
				device.RasterizerState = RasterizerState.CullNone;
				device.BlendState = BlendState.Opaque;
				device.DepthStencilState = DepthStencilState.None;
				foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
					pass.Apply();
					if (verts.Length >= 9)
						device.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, verts.Length / 3);
				}
			}
		}
	}
}
