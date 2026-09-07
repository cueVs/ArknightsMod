using System;
using System.Collections.Generic;
using System.IO;
using ArknightsMod.Common.Particle;
using ArknightsMod.Content.Items.Weapons.Medic.Shining;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Medic.Shining
{
	public sealed class ShiningBladeProjectile : ModProjectile
	{
		private int Mode => (int)Projectile.ai[0];
		private ref float TargetIndex => ref Projectile.ai[1];
		private ref float Age => ref Projectile.ai[2];
		private const float HomingRange = 980f;
		private const float MaxSpeed = 17.5f;
		private const int HomingDelay = 10;
		private const int MaxHits = 3;
		private const int DissolveDuration = 30;
		private int hitCount;
		private int lastHitIndex = -1;
		private int retargetLock;
		private int dissolveAge = -1;

		private bool IsDissolving => dissolveAge >= 0;

		private float VisualOpacity
		{
			get
			{
				float fadeIn = MathHelper.SmoothStep(0f, 1f,
					MathHelper.Clamp(Age / 10f, 0f, 1f));
				if (!IsDissolving)
					return fadeIn;
				float fadeOut = 1f - MathHelper.SmoothStep(0f, 1f,
					MathHelper.Clamp(dissolveAge / (float)DissolveDuration, 0f, 1f));
				return fadeIn * fadeOut;
			}
		}

		public override string Texture => ArknightsMod.noTexture;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 24;
			ProjectileID.Sets.TrailingMode[Type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 22;
			Projectile.height = 14;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			// 闪灵的源石剑不受物块阻挡，追踪目标时可以完整穿墙。
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			// 穿透由本弹幕自行计数，第三次命中后进入可见的消散阶段，而不是瞬间死亡。
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			// extraUpdates = 1：同一敌人至少间隔十帧，不在重叠瞬间耗尽多次伤害。
			Projectile.localNPCHitCooldown = 20;
			Projectile.timeLeft = 150;
			Projectile.extraUpdates = 1;
		}

		public override void AI()
		{
			Age++;
			if (IsDissolving)
			{
				dissolveAge++;
				Projectile.velocity *= 0.88f;
				Projectile.rotation += 0.035f * (Projectile.identity % 2 == 0 ? 1f : -1f);
				if (!Main.dedServ && dissolveAge % 5 == 0 && VisualOpacity > 0.08f)
					ShiningVisuals.SpawnBladeDissolveMote(Projectile.Center, Projectile.velocity, VisualOpacity);
				if (dissolveAge >= DissolveDuration)
					Projectile.Kill();
				return;
			}

			if (Projectile.timeLeft <= DissolveDuration + 2)
			{
				BeginDissolve();
				return;
			}

			if (retargetLock > 0)
				retargetLock--;
			HomeTowardTarget();
			Projectile.rotation = Projectile.velocity.ToRotation();
			Lighting.AddLight(Projectile.Center, Mode == 3
				? new Vector3(0.35f, 0.42f, 0.52f)
				: new Vector3(0.22f, 0.28f, 0.36f));
			if (!Main.dedServ)
				ShiningVisuals.SpawnBladeTrail(Projectile, (int)Age, VisualOpacity);
		}

		private void HomeTowardTarget()
		{
			if (Age <= HomingDelay)
			{
				FreeDrift(0.996f);
				return;
			}

			// 只由持有者选择目标；ai 同步目标及轨迹进度，其他端执行相同的运动预测。
			if (Projectile.owner == Main.myPlayer)
			{
				int closest = -1;
				float distance = HomingRange;
				foreach (NPC npc in Main.ActiveNPCs)
				{
					if (!npc.CanBeChasedBy(Projectile)
						|| (retargetLock > 0 && npc.whoAmI == lastHitIndex))
						continue;
					float candidateDistance = Projectile.Distance(npc.Center);
					if (candidateDistance >= distance)
						continue;
					closest = npc.whoAmI;
					distance = candidateDistance;
				}
				if (TargetIndex != closest + 1 || (int)Age % 30 == 0)
					Projectile.netUpdate = true;
				TargetIndex = closest + 1;
			}

			int index = (int)TargetIndex - 1;
			if (!Main.npc.IndexInRange(index) || !Main.npc[index].CanBeChasedBy(Projectile)
				|| Projectile.Distance(Main.npc[index].Center) >= HomingRange)
			{
				FreeDrift(0.992f);
				return;
			}

			NPC target = Main.npc[index];
			Vector2 currentVelocity = Projectile.velocity;
			if (currentVelocity.Length() < 0.1f)
				currentVelocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 4f;
			Vector2 desiredDirection = (target.Center - Projectile.Center)
				.SafeNormalize(currentVelocity.SafeNormalize(Vector2.UnitX));
			float warmup = Utils.GetLerpValue(HomingDelay, HomingDelay + 36f, Age, true);
			float closePressure = Utils.GetLerpValue(360f, 70f, Projectile.Distance(target.Center), true);
			float pullStrength = MathHelper.Lerp(0.35f, 1f, Math.Max(warmup, closePressure * 0.75f));
			float desiredSpeed = MathHelper.Lerp(11.5f, MaxSpeed, pullStrength);
			float turnRate = MathHelper.Lerp(0.034f, 0.125f, pullStrength);
			float currentAngle = currentVelocity.ToRotation();
			float desiredAngle = desiredDirection.ToRotation();
			float newAngle = currentAngle.AngleTowards(desiredAngle, turnRate);
			float newSpeed = MathHelper.Lerp(currentVelocity.Length(), desiredSpeed,
				MathHelper.Lerp(0.05f, 0.14f, pullStrength));
			Projectile.velocity = newAngle.ToRotationVector2() * newSpeed;
			float sway = MathF.Sin((Age + Projectile.identity * 7f) * 0.075f)
				* MathHelper.Lerp(0.012f, 0.004f, pullStrength);
			Projectile.velocity = Projectile.velocity.RotatedBy(sway);
			if (Projectile.velocity.Length() > MaxSpeed)
				Projectile.velocity = Projectile.velocity.SafeNormalize(desiredDirection) * MaxSpeed;
		}

		private void FreeDrift(float damping)
		{
			float wander = MathF.Sin((Age + Projectile.identity * 5f) * 0.08f) * 0.006f;
			Projectile.velocity = Projectile.velocity.RotatedBy(wander) * damping;
		}

		public override bool? CanDamage() => IsDissolving ? false : null;

		public override bool? CanHitNPC(NPC target)
		{
			if (IsDissolving || (retargetLock > 0 && target.whoAmI == lastHitIndex))
				return false;
			return null;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			hitCount++;
			lastHitIndex = target.whoAmI;
			retargetLock = 12;
			TargetIndex = 0f;
			Projectile.timeLeft = Math.Max(Projectile.timeLeft, 52);
			float deflection = (Projectile.identity + hitCount) % 2 == 0 ? 0.16f : -0.16f;
			Projectile.velocity = Projectile.velocity.RotatedBy(deflection) * 0.96f;

			if (!Main.dedServ)
			{
				ShiningVisuals.SpawnBladeImpact(target.Center, Projectile.velocity, Mode,
					hitCount, hitCount >= MaxHits);
				SoundEngine.PlaySound(SoundID.NPCHit53 with
				{
					Volume = 0.28f,
					Pitch = -0.08f + hitCount * 0.08f,
					MaxInstances = 5
				}, target.Center);
			}

			if (Projectile.owner == Main.myPlayer)
				Projectile.netUpdate = true;
			if (Mode == 1 && Projectile.owner == Main.myPlayer && damageDone > 0
				&& !target.friendly && target.lifeMax > 5 && target.type != NPCID.TargetDummy
				&& Main.player.IndexInRange(Projectile.owner))
				Main.player[Projectile.owner].GetModPlayer<ShiningStaffPlayer>()
					.TryHealFromAttack(target.Center);

			if (hitCount >= MaxHits)
				BeginDissolve();
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.velocity = -oldVelocity * 0.12f;
			BeginDissolve();
			return false;
		}

		private void BeginDissolve()
		{
			if (IsDissolving)
				return;
			dissolveAge = 0;
			TargetIndex = 0f;
			Projectile.tileCollide = false;
			Projectile.friendly = false;
			Projectile.timeLeft = DissolveDuration + 4;
			if (!Main.dedServ)
				ShiningVisuals.SpawnBladeDissolveStart(Projectile.Center, Projectile.velocity, Mode);
			if (Projectile.owner == Main.myPlayer)
				Projectile.netUpdate = true;
		}

		public override void OnKill(int timeLeft)
		{
			// 正常消散期间已经逐步释放碎屑；这里只兜底处理外部强制删除。
			if (!Main.dedServ && !IsDissolving)
				ShiningVisuals.SpawnBladeFragments(Projectile.Center, Projectile.velocity, Mode == 3 ? 8 : 5);
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(Projectile.timeLeft);
			writer.Write((byte)Math.Clamp(hitCount, 0, MaxHits));
			writer.Write((short)lastHitIndex);
			writer.Write((byte)Math.Clamp(retargetLock, 0, byte.MaxValue));
			writer.Write((short)dissolveAge);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			Projectile.timeLeft = Math.Clamp(reader.ReadInt32(), 1, 150);
			hitCount = Math.Clamp((int)reader.ReadByte(), 0, MaxHits);
			lastHitIndex = reader.ReadInt16();
			retargetLock = reader.ReadByte();
			dissolveAge = Math.Clamp((int)reader.ReadInt16(), -1, DissolveDuration);
			if (IsDissolving)
			{
				Projectile.friendly = false;
				Projectile.tileCollide = false;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			ShiningVisuals.DrawBladeProjectile(Projectile, Mode, Age, VisualOpacity);
			return false;
		}
	}

	/// <summary>
	/// 命中产生的治疗术体。它先从敌人伤口侧向逸出，再转向施术者；抵达后才实际结算治疗。
	/// </summary>
	public sealed class ShiningHealingWispProjectile : ModProjectile
	{
		private int HealAmount => Math.Max(1, (int)Projectile.ai[0]);
		private ref float Phase => ref Projectile.ai[1];
		private ref float Age => ref Projectile.ai[2];
		private const int AbsorbDuration = 18;
		private const int MissFadeDuration = 24;

		public override string Texture => ArknightsMod.noTexture;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 18;
			ProjectileID.Sets.TrailingMode[Type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 180;
			Projectile.extraUpdates = 1;
			Projectile.netImportant = true;
		}

		public override bool? CanDamage() => false;

		public override bool ShouldUpdatePosition() => Phase == 0f;

		public override void AI()
		{
			if (!Main.player.IndexInRange(Projectile.owner))
			{
				Projectile.Kill();
				return;
			}

			Player owner = Main.player[Projectile.owner];
			if (!owner.active || owner.dead)
			{
				Projectile.Kill();
				return;
			}

			Age++;
			Vector2 destination = owner.MountedCenter - Vector2.UnitY * 6f;
			if (Phase == 1f)
			{
				Projectile.Center = Vector2.Lerp(Projectile.Center, destination, 0.38f);
				Projectile.velocity = Vector2.Zero;
				Projectile.rotation += 0.16f;
				if (Age >= AbsorbDuration)
					Projectile.Kill();
				return;
			}

			if (Phase == 2f)
			{
				Projectile.velocity *= 0.88f;
				Projectile.rotation += 0.08f;
				if (Age >= MissFadeDuration)
					Projectile.Kill();
				return;
			}

			if (Projectile.timeLeft <= MissFadeDuration + 2)
			{
				Phase = 2f;
				Age = 0f;
				if (Projectile.owner == Main.myPlayer)
					Projectile.netUpdate = true;
				return;
			}

			Vector2 toOwner = destination - Projectile.Center;
			float distance = toOwner.Length();
			if (distance <= 22f)
			{
				BeginAbsorption(owner, destination);
				return;
			}

			Vector2 desiredDirection = toOwner.SafeNormalize(Vector2.UnitY);
			if (Age <= 8f)
			{
				float openingCurve = Projectile.identity % 2 == 0 ? 0.035f : -0.035f;
				Projectile.velocity = Projectile.velocity.RotatedBy(openingCurve) * 0.985f;
			}
			else
			{
				float closePressure = Utils.GetLerpValue(340f, 55f, distance, true);
				float desiredSpeed = MathHelper.Lerp(8.5f, 18f, closePressure);
				float turnRate = MathHelper.Lerp(0.065f, 0.22f, closePressure);
				float currentAngle = Projectile.velocity.SafeNormalize(desiredDirection).ToRotation();
				float newAngle = currentAngle.AngleTowards(desiredDirection.ToRotation(), turnRate);
				float newSpeed = MathHelper.Lerp(Projectile.velocity.Length(), desiredSpeed,
					MathHelper.Lerp(0.08f, 0.22f, closePressure));
				Projectile.velocity = newAngle.ToRotationVector2() * newSpeed;
			}

			Projectile.rotation = Projectile.velocity.ToRotation();
			Lighting.AddLight(Projectile.Center, 0.08f, 0.12f, 0.13f);
			if (!Main.dedServ)
				ShiningVisuals.SpawnHealingTrail(Projectile, (int)Age);
		}

		private void BeginAbsorption(Player owner, Vector2 destination)
		{
			Phase = 1f;
			Age = 0f;
			Projectile.Center = destination;
			Projectile.velocity = Vector2.Zero;
			if (Projectile.localAI[0] == 0f && !Main.dedServ)
			{
				Projectile.localAI[0] = 1f;
				ShiningVisuals.SpawnHealingArrival(destination);
			}
			if (Projectile.owner == Main.myPlayer)
			{
				owner.GetModPlayer<ShiningStaffPlayer>().CompletePendingHeal(HealAmount);
				Projectile.netUpdate = true;
			}
		}

		private float GetVisualOpacity()
		{
			if (Phase == 1f)
				return 1f - MathHelper.SmoothStep(0f, 1f,
					MathHelper.Clamp(Age / AbsorbDuration, 0f, 1f));
			if (Phase == 2f)
				return 1f - MathHelper.SmoothStep(0f, 1f,
					MathHelper.Clamp(Age / MissFadeDuration, 0f, 1f));
			return MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(Age / 8f, 0f, 1f));
		}

		public override bool PreDraw(ref Color lightColor)
		{
			ShiningVisuals.DrawHealingWisp(Projectile, Age, GetVisualOpacity(), Phase == 1f);
			return false;
		}
	}

	public sealed class ShiningBarrierProjectile : ModProjectile
	{
		private float lastReceivedHp = -1f;
		public override string Texture => ArknightsMod.noTexture;

		public override void Load() => ShiningBarrierRenderer.Load();
		public override void Unload() => ShiningBarrierRenderer.Unload();

		public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 360;

		public override void SetDefaults()
		{
			Projectile.width = 156;
			Projectile.height = 156;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 120;
			Projectile.netImportant = true;
		}

		public override bool ShouldUpdatePosition() => false;
		public override bool? CanDamage() => false;

		public override void AI()
		{
			if (!Main.player.IndexInRange(Projectile.owner))
			{
				Projectile.Kill();
				return;
			}

			Player owner = Main.player[Projectile.owner];
			if (!owner.active || owner.dead)
			{
				Projectile.Kill();
				return;
			}
			// 实际吸收伤害由持有者结算；远端只读取同步后的盾量，不读取未同步的 ModPlayer。
			if (Projectile.owner == Main.myPlayer)
			{
				ShiningStaffPlayer shield = owner.GetModPlayer<ShiningStaffPlayer>();
				if (shield.ShieldHp <= 0 || shield.ShieldTime <= 0)
				{
					Projectile.Kill();
					return;
				}
				Projectile.localAI[0]++;
				if (Projectile.ai[0] != shield.ShieldHp || Projectile.ai[1] != shield.ShieldMax
					|| Projectile.localAI[0] % 30f == 0f)
					Projectile.netUpdate = true;
				Projectile.ai[0] = shield.ShieldHp;
				Projectile.ai[1] = shield.ShieldMax;
				Projectile.ai[2] = shield.ShieldHitFlash;
				Projectile.timeLeft = 120;
			}
			else
				Projectile.ai[2] *= 0.86f;

			Projectile.Center = owner.MountedCenter;
			Projectile.rotation += 0.011f;
			Lighting.AddLight(Projectile.Center, 0.18f, 0.21f, 0.28f);
		}

		public override void SendExtraAI(BinaryWriter writer) => writer.Write(Projectile.timeLeft);

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			// 原版弹幕包不带 timeLeft；显式续期同时支持中途加入和断线后的超时清理。
			Projectile.timeLeft = Math.Clamp(reader.ReadInt32(), 1, 120);
			if (!Main.dedServ && Projectile.owner != Main.myPlayer)
			{
				if (lastReceivedHp < 0f)
					ShiningVisuals.SpawnBarrierParticles(Projectile.Center, true, 16);
				else if (Projectile.ai[0] < lastReceivedHp)
					ShiningVisuals.SpawnBarrierHit(Projectile.Center, false);
			}
			lastReceivedHp = Projectile.ai[0];
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,
			List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
			=> overPlayers.Add(index);

		public override bool PreDraw(ref Color lightColor)
		{
			float ratio = Projectile.ai[1] > 0f ? Projectile.ai[0] / Projectile.ai[1] : 0f;
			ShiningBarrierRenderer.Draw(Projectile.Center, ratio,
				Projectile.ai[2], Projectile.rotation, 158f);
			return false;
		}

		public override void OnKill(int timeLeft)
		{
			if (!Main.dedServ)
				ShiningVisuals.SpawnBarrierParticles(Projectile.Center, false, 18);
		}
	}

	public sealed class ShiningSanctuaryProjectile : ModProjectile
	{
		private ref float Lifetime => ref Projectile.ai[0];
		private ref float Age => ref Projectile.ai[1];

		public override string Texture => ArknightsMod.noTexture;

		public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 720;

		public override void SetDefaults()
		{
			Projectile.width = 420;
			Projectile.height = 220;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 120;
			Projectile.netImportant = true;
		}

		public override bool ShouldUpdatePosition() => false;
		public override bool? CanDamage() => false;

		public override void AI()
		{
			if (!Main.player.IndexInRange(Projectile.owner))
			{
				Projectile.Kill();
				return;
			}

			Player owner = Main.player[Projectile.owner];
			WeaponPlayer weaponPlayer = owner.GetModPlayer<WeaponPlayer>();
			bool isOwner = Projectile.owner == Main.myPlayer;
			if (!owner.active || owner.dead || Age >= Lifetime
				|| (isOwner && (owner.HeldItem.ModItem is not ShiningStaff
					|| weaponPlayer.Skill != 2 || !weaponPlayer.SkillActive)))
			{
				Projectile.Kill();
				return;
			}

			Age++;
			Projectile.Center = owner.MountedCenter + new Vector2(0f, 28f);
			if (isOwner)
			{
				Projectile.timeLeft = 120;
				if (Age % 30f == 0f)
					Projectile.netUpdate = true;
			}
			Lighting.AddLight(owner.Center, 0.24f, 0.25f, 0.32f);
			if (!Main.dedServ && Age % 12f == 0f)
				ShiningVisuals.SpawnSanctuaryMote(Projectile.Center, 196f);
		}

		public override void SendExtraAI(BinaryWriter writer) => writer.Write(Projectile.timeLeft);

		public override void ReceiveExtraAI(BinaryReader reader)
			=> Projectile.timeLeft = Math.Clamp(reader.ReadInt32(), 1, 120);

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,
			List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
			=> behindNPCsAndTiles.Add(index);

		public override bool PreDraw(ref Color lightColor)
		{
			float duration = Math.Max(1f, Lifetime);
			float fadeIn = MathHelper.Clamp(Age / 24f, 0f, 1f);
			float fadeOut = MathHelper.Clamp((duration - Age) / 30f, 0f, 1f);
			ShiningVisuals.DrawSanctuary(Projectile.Center, Age, Math.Min(fadeIn, fadeOut));
			return false;
		}
	}

	internal static class ShiningBarrierRenderer
	{
		private static Asset<Effect> effect;
		private static Asset<Texture2D> noise;
		private static bool shaderFaulted;

		internal static void Load()
		{
			if (Main.dedServ)
				return;

			const string effectPath = "ArknightsMod/Assets/Effects/ShiningBarrierSurface";
			const string noisePath = "ArknightsMod/Content/Projectiles/Medic/Shining/NoiseSoft";
			try
			{
				if (!ModContent.RequestIfExists(effectPath, out Asset<Effect> barrierEffect,
						AssetRequestMode.ImmediateLoad) ||
					!ModContent.RequestIfExists(noisePath, out Asset<Texture2D> barrierNoise,
						AssetRequestMode.ImmediateLoad))
				{
					shaderFaulted = true;
					return;
				}

				effect = barrierEffect;
				noise = barrierNoise;
			}
			catch
			{
				effect = null;
				noise = null;
				shaderFaulted = true;
			}
		}

		internal static void Unload()
		{
			effect = null;
			noise = null;
			shaderFaulted = false;
		}

		internal static void Draw(Vector2 center, float charge, float hitFlash, float rotation, float diameter)
		{
			charge = MathHelper.Clamp(charge, 0f, 1f);
			hitFlash = MathHelper.Clamp(hitFlash, 0f, 1f);
			bool drawn = false;
			if (!Main.dedServ && !shaderFaulted && effect?.Value != null && noise?.Value != null)
				drawn = TryDrawShader(center, charge, hitFlash, rotation, diameter);

			DrawOverlay(center, charge, hitFlash, rotation, diameter, drawn ? 0.76f : 1f);
		}

		private static bool TryDrawShader(Vector2 center, float charge, float hitFlash,
			float rotation, float diameter)
		{
			GraphicsDevice device = Main.instance.GraphicsDevice;
			Texture previousTexture1 = device.Textures[1];
			SamplerState previousSampler1 = device.SamplerStates[1];
			bool ended = false;
			bool started = false;
			try
			{
				Effect shader = effect.Value;
				Set(shader, "globalTime", Main.GlobalTimeWrappedHourly);
				Set(shader, "uShieldColor", new Vector3(0.025f, 0.035f, 0.065f));
				Set(shader, "uEdgeColor", Vector3.Lerp(new Vector3(0.48f, 0.62f, 0.78f),
					Vector3.One, hitFlash * 0.78f));
				Set(shader, "uCoreColor", new Vector3(0.92f, 0.97f, 1f));
				Set(shader, "uCharge", MathF.Sqrt(charge));
				Set(shader, "uHitFlash", hitFlash);
				Set(shader, "uOpacity", 0.70f + charge * 0.16f);
				Set(shader, "uRotation", rotation);
				device.Textures[1] = noise.Value;
				device.SamplerStates[1] = SamplerState.LinearWrap;

				Main.spriteBatch.End();
				ended = true;
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp,
					DepthStencilState.None, RasterizerState.CullNone, shader,
					Main.GameViewMatrix.TransformationMatrix);
				started = true;

				Texture2D canvas = noise.Value;
				Vector2 origin = canvas.Size() * 0.5f;
				Vector2 position = center - Main.screenPosition;
				float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 2.1f) * 0.018f;
				Vector2 scale = Vector2.One * (diameter * pulse) / canvas.Size();

				Set(shader, "uLayer", 0f);
				shader.CurrentTechnique.Passes[0].Apply();
				Main.spriteBatch.Draw(canvas, position, null, Color.White, 0f, origin, scale,
					SpriteEffects.None, 0f);
				Set(shader, "uLayer", 1f);
				shader.CurrentTechnique.Passes[0].Apply();
				Main.spriteBatch.Draw(canvas, position, null, Color.White, 0f, origin, scale * 1.025f,
					SpriteEffects.None, 0f);
				return true;
			}
			catch
			{
				shaderFaulted = true;
				return false;
			}
			finally
			{
				if (started)
					Main.spriteBatch.End();
				if (ended)
					Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
						Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone,
						null, Main.GameViewMatrix.TransformationMatrix);
				device.Textures[1] = previousTexture1;
				device.SamplerStates[1] = previousSampler1;
			}
		}

		private static void DrawOverlay(Vector2 center, float charge, float hitFlash,
			float rotation, float diameter, float opacity)
		{
			ShiningVisuals.BeginAdditive();
			float radius = diameter * 0.49f;
			ShiningVisuals.DrawEllipse(center, radius, radius, 2.4f,
				new Color(195, 222, 242), opacity * (0.34f + charge * 0.24f));
			ShiningVisuals.DrawEllipse(center, radius * (0.76f - hitFlash * 0.34f),
				radius * (0.76f - hitFlash * 0.34f), 3.4f, Color.White,
				opacity * hitFlash * 0.86f);
			for (int i = 0; i < 6; i++)
			{
				float angle = rotation + MathHelper.TwoPi * i / 6f;
				Vector2 direction = angle.ToRotationVector2();
				Vector2 tangent = new(-direction.Y, direction.X);
				Vector2 point = center + direction * radius * 0.88f;
				ShiningVisuals.DrawLine(point - tangent * 7f, point + tangent * 7f, 2f,
					Color.White * (opacity * 0.42f));
			}
			ShiningVisuals.EndAdditive();
		}

		private static void Set(Effect shader, string name, float value)
			=> shader.Parameters[name]?.SetValue(value);
		private static void Set(Effect shader, string name, Vector3 value)
			=> shader.Parameters[name]?.SetValue(value);
	}

	internal static class ShiningVisuals
	{
		private const string BladePetalTexturePath = "ArknightsMod/Content/Textures/trace_03";
		private const string BladeArcTexturePath = "ArknightsMod/Content/Textures/slash_04";
		private const string SoftParticleTexturePath = "ArknightsMod/Common/Particle/DefaultParticle";

		internal static void BeginAdditive()
		{
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
				DepthStencilState.None, RasterizerState.CullNone, null,
				Main.GameViewMatrix.TransformationMatrix);
		}

		internal static void EndAdditive()
		{
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
				DepthStencilState.None, RasterizerState.CullNone, null,
				Main.GameViewMatrix.TransformationMatrix);
		}

		internal static void DrawBladeProjectile(Projectile projectile, int mode, float age, float opacity)
		{
			if (opacity <= 0.001f)
				return;

			Vector2 direction = projectile.velocity.SafeNormalize(Vector2.UnitX);
			Vector2 center = projectile.Center;
			float length = mode == 3 ? 76f : 62f;
			float width = mode == 3 ? 9f : 7f;

			for (int i = projectile.oldPos.Length - 1; i >= 0; i--)
			{
				if (projectile.oldPos[i] == Vector2.Zero)
					continue;
				float fade = 1f - i / (float)projectile.oldPos.Length;
				Vector2 oldCenter = projectile.oldPos[i] + projectile.Size * 0.5f;
				Vector2 oldDirection = projectile.oldRot[i].ToRotationVector2();
				DrawSoftInkStreak(oldCenter - oldDirection * length * 0.10f, oldDirection,
					length * MathHelper.Lerp(0.22f, 0.62f, fade),
					Math.Max(1f, width * 0.72f * fade), 0.46f * fade * opacity);
			}

			DrawConvergingPetals(center, direction, age, opacity, mode == 3 ? 1.16f : 1f, false);
			DrawInkBladeShape(center, direction, length, width, opacity, false);

			BeginAdditive();
			for (int i = projectile.oldPos.Length - 1; i >= 0; i--)
			{
				if (projectile.oldPos[i] == Vector2.Zero)
					continue;
				float fade = 1f - i / (float)projectile.oldPos.Length;
				Vector2 oldCenter = projectile.oldPos[i] + projectile.Size * 0.5f;
				Vector2 oldDirection = projectile.oldRot[i].ToRotationVector2();
				DrawLine(oldCenter - oldDirection * length * 0.46f, oldCenter + oldDirection * length * 0.18f,
					Math.Max(1f, width * 0.34f * fade),
					new Color(135, 161, 190) * (0.30f * fade * opacity));
			}
			DrawLine(center - direction * length * 0.46f, center + direction * length * 0.50f,
				width * 0.56f, new Color(145, 169, 196) * (0.78f * opacity));
			DrawLine(center - direction * length * 0.34f, center + direction * length * 0.54f,
				Math.Max(1.4f, width * 0.20f), Color.White * (0.96f * opacity));
			EndAdditive();
		}

		private static void DrawConvergingPetals(Vector2 center, Vector2 direction, float age,
			float opacity, float sizeMultiplier, bool healing)
		{
			Texture2D petal = ModContent.Request<Texture2D>(BladePetalTexturePath).Value;
			float convergence = MathHelper.SmoothStep(0f, 1f,
				MathHelper.Clamp(age / (healing ? 55f : 92f), 0f, 1f));
			float radius = MathHelper.Lerp(healing ? 16f : 25f, healing ? 4.5f : 7f, convergence);
			// 四瓣以 45° 起始、彼此相隔 90°，飞行过程中缓慢旋转并向轴心收拢。
			float orbit = MathHelper.PiOver4 + age * (healing ? -0.020f : 0.012f);
			Vector2 convergencePoint = center + direction * (healing ? 26f : 43f);
			float petalLength = MathHelper.Lerp(healing ? 28f : 44f, healing ? 19f : 29f,
				convergence) * sizeMultiplier;
			float petalWidth = (healing ? 4.2f : 6.2f) * sizeMultiplier;

			for (int i = 0; i < 4; i++)
			{
				Vector2 radial = (orbit + MathHelper.PiOver2 * i).ToRotationVector2();
				Vector2 position = center + radial * radius;
				Vector2 aim = (convergencePoint - position).SafeNormalize(direction);
				Vector2 scale = new(petalWidth / petal.Width, petalLength / petal.Height);
				Main.spriteBatch.Draw(petal, position - Main.screenPosition, null,
					new Color(2, 3, 7) * (opacity * (healing ? 0.66f : 0.82f)),
					aim.ToRotation() + MathHelper.PiOver2, petal.Size() * 0.5f,
					scale, SpriteEffects.None, 0f);
			}
		}

		private static void DrawInkBladeShape(Vector2 center, Vector2 direction, float length,
			float width, float opacity, bool healing)
		{
			Texture2D arc = ModContent.Request<Texture2D>(BladeArcTexturePath).Value;
			float rotation = direction.ToRotation();
			Vector2 arcScale = new(length / 440f, width / 92f);
			Color shellColor = new Color(2, 3, 7) * (opacity * (healing ? 0.62f : 0.82f));
			// 同一条透明弧上下镜像，组成两端尖、中央留有呼吸空间的剑叶轮廓。
			Main.spriteBatch.Draw(arc, center - Main.screenPosition, null, shellColor,
				rotation, arc.Size() * 0.5f, arcScale, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(arc, center - Main.screenPosition, null, shellColor,
				rotation, arc.Size() * 0.5f, arcScale, SpriteEffects.FlipVertically, 0f);

			DrawSoftInkStreak(center - direction * length * 0.02f, direction,
				length * (healing ? 0.48f : 0.68f), width * (healing ? 0.82f : 1.04f),
				opacity * (healing ? 0.44f : 0.58f));
		}

		private static void DrawSoftInkStreak(Vector2 center, Vector2 direction, float length,
			float width, float opacity)
		{
			if (opacity <= 0.001f || length <= 0.1f || width <= 0.1f)
				return;
			Texture2D soft = ModContent.Request<Texture2D>(SoftParticleTexturePath).Value;
			Vector2 scale = new(length / 56f, width / 28f);
			Main.spriteBatch.Draw(soft, center - Main.screenPosition, null,
				new Color(3, 4, 9) * opacity, direction.ToRotation(), soft.Size() * 0.5f,
				scale, SpriteEffects.None, 0f);
		}

		internal static void SpawnBladeTrail(Projectile projectile, int age, float opacity)
		{
			if (opacity <= 0.08f)
				return;
			Vector2 forward = projectile.velocity.SafeNormalize(Vector2.UnitX);
			Vector2 normal = new(-forward.Y, forward.X);
			Vector2 tail = projectile.Center - forward * 16f;
			// 每个游戏帧留下一片非加法墨羽，密度接近连续采样；银屑只负责勾边。
			if (age % 2 == 0)
			{
				float side = age % 8 == 0 ? -1f : 1f;
				new ShiningInkParticle(tail + normal * side * Main.rand.NextFloat(1f, 4f),
					-forward * Main.rand.NextFloat(0.45f, 0.82f) + normal * side * 0.38f,
					Main.rand.Next(16, 24), Main.rand.NextFloat(0.34f, 0.52f), opacity).Spawn();
			}
			if (age % 6 == 0)
				new ShiningBarrierShardParticle(tail, -forward * 0.6f,
					new Color(145, 154, 170), 16, 0.22f).Spawn();
			if (age % 12 == 0)
			{
				Dust dust = Dust.NewDustPerfect(tail + normal * Main.rand.NextFloat(-3f, 3f),
					DustID.Smoke, -forward * 0.35f, 40, new Color(9, 10, 15), 0.65f);
				dust.noGravity = true;
				dust.noLight = true;
			}
		}

		internal static void SpawnBladeImpact(Vector2 center, Vector2 velocity, int mode,
			int hitCount, bool finalHit)
		{
			Vector2 forward = velocity.SafeNormalize(Vector2.UnitX);
			float strength = (mode == 3 ? 1.15f : 1f) * (finalHit ? 1.2f : 1f);
			new ShiningInkPulseParticle(center, forward.ToRotation(), 17,
				strength, finalHit ? 0.82f : 0.66f).Spawn();

			int inkCount = 5 + hitCount * 2;
			for (int i = 0; i < inkCount; i++)
			{
				Vector2 inkVelocity = (-forward).RotatedBy(Main.rand.NextFloat(-1.15f, 1.15f))
					* Main.rand.NextFloat(1.2f, finalHit ? 5.8f : 4.4f);
				new ShiningInkParticle(center + Main.rand.NextVector2Circular(4f, 4f), inkVelocity,
					Main.rand.Next(13, 22), Main.rand.NextFloat(0.28f, 0.56f) * strength,
					finalHit ? 1f : 0.86f).Spawn();
			}

			int shardCount = finalHit ? 8 : 4 + hitCount;
			for (int i = 0; i < shardCount; i++)
			{
				Vector2 shardVelocity = forward.RotatedBy(Main.rand.NextFloat(-1.2f, 1.2f))
					* Main.rand.NextFloat(1.8f, finalHit ? 7f : 4.8f);
				new ShiningBarrierShardParticle(center, shardVelocity,
					i % 3 == 0 ? Color.White : new Color(128, 151, 178),
					Main.rand.Next(10, 19), Main.rand.NextFloat(0.26f, 0.55f) * strength).Spawn();
			}
		}

		internal static void SpawnBladeDissolveStart(Vector2 center, Vector2 velocity, int mode)
		{
			Vector2 forward = velocity.SafeNormalize(Vector2.UnitX);
			new ShiningInkPulseParticle(center, forward.ToRotation(), 22,
				mode == 3 ? 1.25f : 1f, 0.58f).Spawn();
			for (int i = 0; i < 4; i++)
			{
				Vector2 outward = (MathHelper.PiOver4 + MathHelper.PiOver2 * i).ToRotationVector2();
				new ShiningInkParticle(center + outward * 5f, outward * Main.rand.NextFloat(0.8f, 2.2f),
					Main.rand.Next(18, 27), Main.rand.NextFloat(0.30f, 0.48f), 0.74f).Spawn();
			}
		}

		internal static void SpawnBladeDissolveMote(Vector2 center, Vector2 velocity, float opacity)
		{
			Vector2 direction = velocity.SafeNormalize(Main.rand.NextVector2Unit())
				.RotatedBy(Main.rand.NextFloat(-0.9f, 0.9f));
			new ShiningInkParticle(center + Main.rand.NextVector2Circular(5f, 5f),
				direction * Main.rand.NextFloat(0.5f, 1.8f), Main.rand.Next(12, 19),
				Main.rand.NextFloat(0.24f, 0.40f), opacity).Spawn();
		}

		internal static void SpawnHealingTrail(Projectile projectile, int age)
		{
			Vector2 forward = projectile.velocity.SafeNormalize(Vector2.UnitY);
			Vector2 normal = new(-forward.Y, forward.X);
			Vector2 tail = projectile.Center - forward * 7f;
			if (age % 3 == 0)
			{
				float side = age % 6 == 0 ? -1f : 1f;
				new ShiningInkParticle(tail + normal * side * 2f,
					-forward * 0.35f + normal * side * 0.24f, Main.rand.Next(13, 20),
					Main.rand.NextFloat(0.20f, 0.33f), 0.66f).Spawn();
			}
			if (age % 7 == 0)
				new ShiningBarrierShardParticle(tail, -forward * 0.28f,
					new Color(156, 198, 195), 14, 0.16f).Spawn();
		}

		internal static void DrawHealingWisp(Projectile projectile, float age,
			float opacity, bool absorbing)
		{
			if (opacity <= 0.001f)
				return;
			Vector2 direction = projectile.velocity.LengthSquared() > 0.01f
				? projectile.velocity.SafeNormalize(Vector2.UnitY)
				: projectile.rotation.ToRotationVector2();
			Vector2 center = projectile.Center;
			for (int i = projectile.oldPos.Length - 1; i >= 0; i--)
			{
				if (projectile.oldPos[i] == Vector2.Zero)
					continue;
				float fade = 1f - i / (float)projectile.oldPos.Length;
				Vector2 oldCenter = projectile.oldPos[i] + projectile.Size * 0.5f;
				DrawSoftInkStreak(oldCenter - direction * 1.5f, direction,
					MathHelper.Lerp(4f, 13f, fade), Math.Max(1f, 4.5f * fade),
					0.42f * fade * opacity);
			}

			DrawConvergingPetals(center, direction, absorbing ? 55f : age,
				opacity, absorbing ? 0.72f : 0.84f, true);
			DrawInkBladeShape(center, direction, 24f, 5.5f, opacity, true);

			BeginAdditive();
			Texture2D soft = ModContent.Request<Texture2D>(SoftParticleTexturePath).Value;
			float pulse = 1f + MathF.Sin(age * 0.18f) * 0.10f;
			float absorbScale = absorbing ? MathHelper.Lerp(1f, 0.45f,
				MathHelper.Clamp(age / 18f, 0f, 1f)) : 1f;
			Main.spriteBatch.Draw(soft, center - Main.screenPosition, null,
				new Color(150, 202, 198) * (0.72f * opacity), projectile.rotation,
				soft.Size() * 0.5f, 0.42f * pulse * absorbScale, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(soft, center - Main.screenPosition, null,
				Color.White * (0.82f * opacity), projectile.rotation,
				soft.Size() * 0.5f, 0.18f * absorbScale, SpriteEffects.None, 0f);
			EndAdditive();
		}

		internal static void SpawnHealingArrival(Vector2 center)
		{
			new ShiningInkPulseParticle(center, 0f, 20, 0.82f, 0.52f).Spawn();
			for (int i = 0; i < 8; i++)
			{
				Vector2 direction = (MathHelper.TwoPi * i / 8f).ToRotationVector2();
				Vector2 position = center + direction * Main.rand.NextFloat(24f, 42f);
				new ShiningInkParticle(position, -direction * Main.rand.NextFloat(1.8f, 3.8f),
					Main.rand.Next(13, 20), Main.rand.NextFloat(0.22f, 0.38f), 0.72f).Spawn();
				if (i % 2 == 0)
					new ShiningBarrierShardParticle(position, -direction * Main.rand.NextFloat(2f, 4.5f),
						new Color(170, 218, 208), 16, 0.28f).Spawn();
			}
		}

		internal static void DrawSanctuary(Vector2 center, float age, float opacity)
		{
			if (opacity <= 0f)
				return;
			BeginAdditive();
			float pulse = 1f + MathF.Sin(age * 0.045f) * 0.035f;
			Vector2 floor = center + new Vector2(0f, 58f);
			DrawEllipse(floor, 216f * pulse, 47f * pulse, 7f,
				new Color(42, 52, 76), opacity * 0.72f);
			DrawEllipse(floor, 174f * pulse, 34f * pulse, 2.5f,
				Color.White, opacity * 0.46f);
			DrawEllipse(center, 205f * pulse, 145f * pulse, 2f,
				new Color(128, 150, 180), opacity * 0.30f);
			for (int i = 0; i < 12; i++)
			{
				float angle = MathHelper.TwoPi * i / 12f + age * 0.006f;
				Vector2 point = floor + new Vector2(MathF.Cos(angle) * 184f, MathF.Sin(angle) * 38f);
				DrawLine(point - Vector2.UnitY * 8f, point - Vector2.UnitY * (38f + i % 3 * 12f),
					i % 3 == 0 ? 4f : 2f, (i % 2 == 0 ? Color.White : new Color(85, 104, 136))
					* (opacity * 0.38f));
			}
			EndAdditive();
		}

		internal static void DrawEllipse(Vector2 worldCenter, float radiusX, float radiusY,
			float width, Color color, float opacity, int segments = 64)
		{
			Vector2 previous = worldCenter + new Vector2(radiusX, 0f);
			for (int i = 1; i <= segments; i++)
			{
				float angle = MathHelper.TwoPi * i / segments;
				Vector2 current = worldCenter + new Vector2(MathF.Cos(angle) * radiusX,
					MathF.Sin(angle) * radiusY);
				DrawLine(previous, current, width, color * opacity);
				previous = current;
			}
		}

		internal static void DrawLine(Vector2 worldStart, Vector2 worldEnd, float width, Color color)
		{
			Vector2 start = worldStart - Main.screenPosition;
			Vector2 end = worldEnd - Main.screenPosition;
			Vector2 delta = end - start;
			if (delta.LengthSquared() < 0.01f)
				return;
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Main.spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), color,
				delta.ToRotation(), new Vector2(0f, 0.5f), new Vector2(delta.Length(), width),
				SpriteEffects.None, 0f);
		}

		internal static void SpawnBarrierParticles(Vector2 center, bool inward, int count)
		{
			if (Main.dedServ)
				return;
			for (int i = 0; i < count; i++)
			{
				Vector2 direction = Main.rand.NextVector2Unit();
				Vector2 position = inward ? center + direction * Main.rand.NextFloat(44f, 82f) : center;
				Vector2 velocity = direction * Main.rand.NextFloat(inward ? -4.4f : 2.6f,
					inward ? -1.5f : 7.2f);
				new ShiningBarrierShardParticle(position, velocity,
					i % 4 == 0 ? Color.White : new Color(98, 122, 158),
					Main.rand.Next(18, 30), Main.rand.NextFloat(0.55f, 1.15f)).Spawn();
			}
			for (int i = 0; i < Math.Max(2, count / 5); i++)
			{
				Dust dust = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(32f, 42f),
					DustID.SilverFlame, Main.rand.NextVector2Circular(2.2f, 2.2f), 60,
					Color.White, Main.rand.NextFloat(0.65f, 1.05f));
				dust.noGravity = true;
			}
		}

		internal static void SpawnBarrierHit(Vector2 center, bool broken)
			=> SpawnBarrierParticles(center, false, broken ? 22 : 10);

		internal static void SpawnBladeFragments(Vector2 center, Vector2 velocity, int count)
		{
			if (Main.dedServ)
				return;
			Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);
			for (int i = 0; i < count; i++)
			{
				Vector2 shardVelocity = direction.RotatedBy(Main.rand.NextFloat(-1.1f, 1.1f))
					* Main.rand.NextFloat(1.4f, 5.5f);
				new ShiningBarrierShardParticle(center, shardVelocity,
					i % 3 == 0 ? Color.White : new Color(90, 110, 142),
					Main.rand.Next(12, 22), Main.rand.NextFloat(0.35f, 0.72f)).Spawn();
			}
		}

		internal static void SpawnSanctuaryMote(Vector2 center, float radius)
		{
			if (Main.dedServ)
				return;
			float angle = Main.rand.NextFloat(MathHelper.TwoPi);
			Vector2 position = center + new Vector2(MathF.Cos(angle) * radius,
				MathF.Sin(angle) * radius * 0.24f + 48f);
			new ShiningBarrierShardParticle(position, new Vector2(0f, Main.rand.NextFloat(-1.8f, -0.7f)),
				Main.rand.NextBool(3) ? Color.White : new Color(85, 104, 136),
				Main.rand.Next(26, 42), Main.rand.NextFloat(0.32f, 0.62f)).Spawn();
		}
	}

	/// <summary>非加法混合的细墨羽：黑色不会在 Additive 下消失，复用现有单帧粒子纹理。</summary>
	public sealed class ShiningInkParticle : Particle
	{
		private readonly float strength;
		public override string TexturePath => "ArknightsMod/Common/Particle/DefaultParticle";
		public override BlendState DrawBlendState => BlendState.AlphaBlend;

		public ShiningInkParticle(Vector2 position, Vector2 velocity, int lifetime, float scale,
			float strength = 1f)
		{
			Position = position;
			Velocity = velocity;
			Lifetime = lifetime;
			Scale = scale;
			this.strength = MathHelper.Clamp(strength, 0f, 1f);
			Color = new Color(3, 4, 9);
			Rotation = velocity.ToRotation() + MathHelper.PiOver2;
		}

		public override void Update()
		{
			Velocity *= 0.945f;
			if (Velocity.LengthSquared() > 0.01f)
				Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
			float fadeIn = Utils.GetLerpValue(0f, 0.12f, LifetimeRatio, true);
			Opacity = fadeIn * MathF.Pow(1f - LifetimeRatio, 1.35f) * 0.86f * strength;
			Scale *= 0.982f;
		}

		public override void Draw()
		{
			Main.spriteBatch.Draw(Texture, Position - Main.screenPosition, null, Color * Opacity,
				Rotation, Texture.Size() * 0.5f, new Vector2(0.34f, 1.90f) * Scale,
				SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(Texture, Position - Main.screenPosition, null,
				Color.Black * (Opacity * 0.68f), Rotation, Texture.Size() * 0.5f,
				new Vector2(0.15f, 1.28f) * Scale,
				SpriteEffects.None, 0f);
		}
	}

	/// <summary>使用透明弧形贴图的黑色命中波；保持 AlphaBlend，避免黑色在加法层中消失。</summary>
	public sealed class ShiningInkPulseParticle : Particle
	{
		private readonly float peakOpacity;

		public override string TexturePath => "ArknightsMod/Content/Textures/slash_04";
		public override BlendState DrawBlendState => BlendState.AlphaBlend;

		public ShiningInkPulseParticle(Vector2 position, float rotation, int lifetime,
			float scale, float opacity)
		{
			Position = position;
			Rotation = rotation;
			Lifetime = lifetime;
			Scale = scale;
			peakOpacity = opacity;
			Color = new Color(2, 3, 7);
		}

		public override void Update()
		{
			float fade = 1f - MathHelper.SmoothStep(0f, 1f,
				MathHelper.Clamp(LifetimeRatio, 0f, 1f));
			Opacity = fade * peakOpacity;
			Scale *= 1.035f;
		}

		public override void Draw()
		{
			Main.spriteBatch.Draw(Texture, Position - Main.screenPosition, null, Color * Opacity,
				Rotation, Texture.Size() * 0.5f, new Vector2(0.115f, 0.075f) * Scale,
				SpriteEffects.None, 0f);
		}
	}

	public sealed class ShiningBarrierShardParticle : Particle
	{
		private readonly Color initialColor;

		// DefaultParticle 是本项目的一张独立单帧粒子贴图，不按精灵表切帧。
		public override string TexturePath => "ArknightsMod/Common/Particle/DefaultParticle";
		public override BlendState DrawBlendState => BlendState.Additive;

		public ShiningBarrierShardParticle(Vector2 position, Vector2 velocity, Color color,
			int lifetime, float scale)
		{
			Position = position;
			Velocity = velocity;
			Color = color;
			initialColor = color;
			Lifetime = lifetime;
			Scale = scale;
			Rotation = velocity.ToRotation();
		}

		public override void Update()
		{
			Velocity *= 0.955f;
			Rotation += 0.08f * Math.Sign(Velocity.X == 0f ? 1f : Velocity.X);
			Opacity = MathF.Pow(1f - LifetimeRatio, 1.25f);
			Color = Color.Lerp(initialColor, Color.Transparent, LifetimeRatio * LifetimeRatio);
			Scale *= 0.985f;
		}

		public override void Draw()
		{
			Vector2 drawScale = new(Scale * 0.22f, Scale * 1.65f);
			Main.spriteBatch.Draw(Texture, Position - Main.screenPosition, null, Color * Opacity,
				Rotation, Texture.Size() * 0.5f, drawScale, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(Texture, Position - Main.screenPosition, null, Color.White * (Opacity * 0.32f),
				Rotation, Texture.Size() * 0.5f, drawScale * new Vector2(0.28f, 0.76f),
				SpriteEffects.None, 0f);
		}
	}
}
