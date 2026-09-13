using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using ArknightsMod.Content.ElementalImpairment.Effect;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP2;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP1;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.EffectImage;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.NormalAttack;
using Terraria.Audio;

namespace ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP2
{
	public class InstinctCall : ModProjectile
	{

		private const int Duration = 600;
		private const float ExplosionRadius = 250f;
		private const int BindingDuration = 360;
		private const float SlowAmount = 0.8f;
		private const float DamageMultiplier = 0.75f;
		private const float ImpairmentRatio = 0.125f;
		private const float KnockbackStrength = 0.5f;

	
		private const float RingFadeInDuration = 45f;
		private const float RingMinScale = 0.1f;
		private const float RingMaxScale = 1.0f;

		// 纹理路径 
		private const string InstinctCallTexPath = "ArknightsMod/Content/Projectiles/Supporter/Tragodia/SP2/InstinctCall";
		private const string LightParticleTexPath = "ArknightsMod/Content/Projectiles/Supporter/Tragodia/EffectImage/Tail";

		//音频路径 
		private const string CatAttackSoundPath = "ArknightsMod/Assets/Sound/Tragodia/CatAttack";
		private const string CatSummonSoundPath = "ArknightsMod/Assets/Sound/Tragodia/CatSummon";
		private const string CatBoomSoundPath = "ArknightsMod/Assets/Sound/Tragodia/CatBoom";

		private bool hasExploded;
		private bool isExploding;
		private int explodeTimer;

		private int suctionEffectTimer = 0;
		private Projectile suctionEffectProj = null;

		private bool hasForceExploded = false;
		private const int FORCE_EXPLODE_TIME = 600;

		private Texture2D lightParticleTex;
		private float scrollOffset;
		private float ringFadeInTimer = 0f;
		private bool hasRingFadeStarted = false;

		private InstinctCallExplosion explosionEffect;


		private int animTimer = 0;
		private float catOffsetY = 0f;
		private float catRotation = 0f;
		private float catScale = 1f;
		private float floatOffset = 0f;

		private const float FloatAmplitude = 3f;
		private const float FloatSpeed = 0.025f;

		private const float JumpDuration = 45f;
		private const float JumpHeight = 28f;

		private const float RestDuration = 100f;

		private const float FlipDuration = 50f;
		private const float FlipHeight = 45f;
		private const float FlipRotations = 2f;


		private const float FlipLandDuration = 14f;

		// 冷却
		private const float CooldownDuration = 80f;

		// 总周期 
		private float TotalCycleDuration => JumpDuration + RestDuration + FlipDuration + FlipLandDuration + CooldownDuration;


		private int soundCooldown = 0;
		private const int SOUND_COOLDOWN_MIN = 5;
		private bool hasPlayedSummonSound = false;

		public override void Load() {

		}

		public override void Unload() {

		}

		public override void SetDefaults() {
			Projectile.width = 64;
			Projectile.height = 64;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = int.MaxValue;
			Projectile.alpha = 0;
			Projectile.velocity = Vector2.Zero;
		}

		public override void AI() {
			Projectile.velocity = Vector2.Zero;


			if (soundCooldown > 0)
				soundCooldown--;


			if (!hasPlayedSummonSound) {
				hasPlayedSummonSound = true;
				PlayCatSummonSound(Projectile.Center);
			}


			if (!hasRingFadeStarted) {
				hasRingFadeStarted = true;
				ringFadeInTimer = 0f;
			}
			if (ringFadeInTimer < RingFadeInDuration) {
				ringFadeInTimer++;
			}


			UpdateCatAnimation();


			if (!hasExploded && !isExploding) {
				Projectile.ai[0]++;


				if (Projectile.ai[0] >= FORCE_EXPLODE_TIME && !hasForceExploded) {
					hasForceExploded = true;
					Explode();
					return;
				}
			}


			if (isExploding) {
				explodeTimer++;
				if (explodeTimer >= InstinctCallExplosion.EffectDuration) {
					Projectile.Kill();
				}
				return;
			}

			// 生成牵引效果
			suctionEffectTimer++;
			if (suctionEffectTimer >= 0 && !hasExploded && suctionEffectProj == null) {
				suctionEffectProj = Projectile.NewProjectileDirect(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					Vector2.Zero,
					ModContent.ProjectileType<InstinctCallSuctionLines>(),
					0, 0f, Projectile.owner
				);
			}


			scrollOffset += 1.2f;
			if (scrollOffset >= 100000f)
				scrollOffset -= 100000f;

			// 牵引敌人逻辑
			if (!hasExploded) {
				Player player = Main.player[Projectile.owner];
				if (player != null && player.active) {
					int pullCount = 0;
					const int maxPullTargets = 4;
					const float pullStrength = 0.55f;
					const float maxPullSpeed = 7f;
					const float detonateRadius = 10f;

					foreach (NPC npc in Main.ActiveNPCs) {
						if (pullCount >= maxPullTargets)
							break;
						if (!npc.CanBeChasedBy(player) || npc.friendly)
							continue;

						float distance = Vector2.Distance(npc.Center, Projectile.Center);

						// 敌人碰到核心时引爆
						if (distance <= detonateRadius) {
							ForceExplode();
							return;
						}

						// 牵引效果
						if (distance <= ExplosionRadius && distance > 16f) {
							Vector2 pullVelocity = Vector2.Normalize(Projectile.Center - npc.Center) * pullStrength;
							npc.velocity += pullVelocity;
							if (npc.velocity.Length() > maxPullSpeed)
								npc.velocity = Vector2.Normalize(npc.velocity) * maxPullSpeed;
							pullCount++;
						}
					}
				}
			}
		}

		public override void OnKill(int timeLeft) {

			explosionEffect?.Dispose();
			explosionEffect = null;

			// 清理纹理引用
			lightParticleTex = null;
		}


		private void UpdateCatAnimation() {
			animTimer++;
			if (animTimer > 600)
				animTimer = 0;


			floatOffset = (float)Math.Sin(Main.timeForVisualEffects * FloatSpeed) * FloatAmplitude;

			float elapsed = animTimer % TotalCycleDuration;


			float jumpEnd = JumpDuration;
			float restEnd = jumpEnd + RestDuration;
			float flipEnd = restEnd + FlipDuration;
			float flipLandEnd = flipEnd + FlipLandDuration;


			catOffsetY = floatOffset;
			catRotation = 0f;
			catScale = 1f;

			// 跳跃
			if (elapsed < jumpEnd) {
				float t = elapsed / jumpEnd;
				float height = -4f * JumpHeight * t * (1f - t);
				catOffsetY = height + floatOffset;
				float angleT = (float)Math.Sin(Math.PI * t);
				catRotation = -0.08f * angleT;
				catScale = 1f + 0.03f * angleT;
			}
			// 休息
			else if (elapsed < restEnd) {
				float t = (elapsed - jumpEnd) / RestDuration;
				float breathe = (float)Math.Sin(t * MathHelper.TwoPi * 0.5f) * 1.5f;
				catOffsetY = breathe + floatOffset;
				catRotation = (float)Math.Sin(t * MathHelper.TwoPi * 0.5f) * 0.01f;
				catScale = 1f + (float)Math.Sin(t * MathHelper.TwoPi * 0.5f + MathHelper.Pi) * 0.01f;
			}
			//空翻
			else if (elapsed < flipEnd) {
				float t = (elapsed - restEnd) / FlipDuration;
				float heightProgress = 1f - (2f * t - 1f) * (2f * t - 1f);
				catOffsetY = -FlipHeight * heightProgress + floatOffset;
				catRotation = MathHelper.TwoPi * FlipRotations * t;
				float stretch = (float)Math.Sin(Math.PI * t) * 0.04f;
				catScale = 1f + stretch;
			}
			// 
			else if (elapsed < flipLandEnd) {
				float t = (elapsed - flipEnd) / FlipLandDuration;
				float eased = t * t * (3f - 2f * t);
				catOffsetY = 4f * (1f - eased) + floatOffset;
				catRotation = 0f;
				catScale = 1.02f - 0.05f * eased;
			}
			
			else {
				float t = (elapsed - flipLandEnd) / CooldownDuration;
				float breathe = (float)Math.Sin(t * MathHelper.TwoPi * 0.3f) * 1.2f;
				catOffsetY = breathe + floatOffset;
				catRotation = (float)Math.Sin(t * MathHelper.TwoPi * 0.3f) * 0.008f;
				catScale = 1f + (float)Math.Sin(t * MathHelper.TwoPi * 0.3f + MathHelper.Pi) * 0.008f;
			}
		}

		public void ForceExplode() {
			if (!hasExploded && !isExploding) {
				Explode();
			}
		}

		private void Explode() {
			if (hasExploded || isExploding)
				return;


			PlayCatBoomSound(Projectile.Center);


			if (suctionEffectProj != null && suctionEffectProj.active) {
				suctionEffectProj.Kill();
				suctionEffectProj = null;
			}

			hasExploded = true;
			isExploding = true;
			explodeTimer = 0;


			explosionEffect = new InstinctCallExplosion();
			explosionEffect.Initialize(Projectile.Center, Projectile.owner);



			Projectile noteDust = Projectile.NewProjectileDirect(
				Projectile.GetSource_FromThis(),
				Projectile.Center,
				Vector2.Zero,
				ModContent.ProjectileType<InstinctCallNoteDust>(),
				0, 0f, Projectile.owner);

			if (noteDust.ModProjectile is InstinctCallNoteDust noteProj) {
				noteProj.OrbitRadius = 250f * 1.05f;
				noteProj.AngularSpeed = 0.08f;
				noteProj.NoteAlpha = 1f;
			}



			Player player = Main.player[Projectile.owner];
			if (player == null || !player.active)
				return;

			foreach (NPC npc in Main.ActiveNPCs) {
				if (!npc.CanBeChasedBy(player) || npc.friendly)
					continue;
				if (Vector2.Distance(Projectile.Center, npc.Center) > ExplosionRadius)
					continue;

				BindingEffect.Apply(npc, BindingDuration, SlowAmount, false);
				npc.GetGlobalNPC<InstinctBoundNPC>().StartTick(npc, Projectile.owner, DamageMultiplier, ImpairmentRatio, KnockbackStrength);
			}
		}




		private void PlayCatSummonSound(Vector2 position) {
			SoundStyle catSummonSound = new SoundStyle($"{CatSummonSoundPath}") {
				Volume = 0.7f,
				PitchVariance = 0.1f,
				MaxInstances = 3,
			};
			SoundEngine.PlaySound(catSummonSound, position);
		}


		private void PlayCatBoomSound(Vector2 position) {
			SoundStyle catBoomSound = new SoundStyle($"{CatBoomSoundPath}") {
				Volume = 0.8f,
				PitchVariance = 0.05f,
				MaxInstances = 3,
			};
			SoundEngine.PlaySound(catBoomSound, position);
		}


		private void PlayCatAttackSound(Vector2 position) {
			if (soundCooldown > 0)
				return;

			SoundStyle catAttackSound = new SoundStyle($"{CatAttackSoundPath}") {
				Volume = 0.6f,
				PitchVariance = 0.1f,
				MaxInstances = 5,
			};

			SoundEngine.PlaySound(catAttackSound, position);
			soundCooldown = SOUND_COOLDOWN_MIN;
		}

		

		private float GetRingAlpha() {
			if (ringFadeInTimer >= RingFadeInDuration)
				return 1f;
			float t = ringFadeInTimer / RingFadeInDuration;
			return t * t * t * (t * (6f * t - 15f) + 10f);
		}

		private float GetRingScale() {
			if (ringFadeInTimer >= RingFadeInDuration)
				return RingMaxScale;
			float t = ringFadeInTimer / RingFadeInDuration;
			float eased = t * t * (3f - 2f * t);
			return RingMinScale + (RingMaxScale - RingMinScale) * eased;
		}

		public override bool PreDraw(ref Color lightColor) {
			if (!isExploding) {
				if (lightParticleTex == null)
					lightParticleTex = ModContent.Request<Texture2D>(LightParticleTexPath).Value;

				DrawLightRing();
				DrawCat();
			}
			return false;
		}

		private void DrawCat() {
			Texture2D tex = ModContent.Request<Texture2D>(InstinctCallTexPath).Value;
			if (tex == null || tex.IsDisposed)
				return;

			float zoom = Main.GameViewMatrix.Zoom.X;
			Matrix transform = Main.GameViewMatrix.TransformationMatrix;

			Vector2 drawPos = Vector2.Transform(Projectile.Center - Main.screenPosition, transform);
			drawPos += new Vector2(0, catOffsetY * zoom);

			SpriteEffects effects = SpriteEffects.None;

			float normalizedRot = catRotation % MathHelper.TwoPi;
			if (normalizedRot < 0)
				normalizedRot += MathHelper.TwoPi;
			if (normalizedRot > MathHelper.PiOver2 && normalizedRot < MathHelper.Pi * 1.5f) {
				effects = SpriteEffects.FlipVertically;
			}

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				SamplerState.PointClamp, DepthStencilState.None,
				RasterizerState.CullNone, null, Matrix.Identity);

			Main.spriteBatch.Draw(
				tex,
				drawPos,
				null,
				Color.White,
				catRotation,
				tex.Size() / 2f,
				catScale * zoom,
				effects,
				0f
			);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				SamplerState.PointClamp, DepthStencilState.None,
				RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private void DrawLightRing() {
			if (lightParticleTex == null || lightParticleTex.IsDisposed)
				return;

			float ringAlpha = GetRingAlpha();
			if (ringAlpha < 0.01f)
				return;

			float ringScale = GetRingScale();
			float currentRadius = ExplosionRadius * ringScale;

			const int LightRingSegments = 120;
			const float LightRingThickness = 12f;
			const float LightRingPatternLength = 160f;
			const float LightRingBrightSpeed = 0.03f;
			const int LightRingBrightBands = 3;
			const float LightRingBrightIntensity = 0.4f;
			Color LightRingBaseColor = new Color(180, 100, 255, 220);

			float zoom = Main.GameViewMatrix.Zoom.X;
			Matrix transform = Main.GameViewMatrix.TransformationMatrix;
			Vector2 center = Vector2.Transform(Projectile.Center - Main.screenPosition, transform);
			float scaledRadius = currentRadius * zoom;

			SpriteBatch sb = Main.spriteBatch;
			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

			float angleStep = MathHelper.TwoPi / LightRingSegments;
			float texScale = lightParticleTex.Height / LightRingPatternLength;

			for (int i = 0; i < LightRingSegments; i++) {
				float angle = i * angleStep;
				Vector2 pos = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * scaledRadius;
				float rotation = angle + MathHelper.Pi;
				float arcLen = scaledRadius * angleStep;
				int srcHeight = (int)(arcLen * texScale + 0.5f);
				if (srcHeight < 1)
					srcHeight = 1;
				int srcY = (int)((scrollOffset + i * arcLen) * texScale) % lightParticleTex.Height;
				Rectangle sourceRect = new Rectangle(0, srcY, lightParticleTex.Width, srcHeight);
				float scaleX = LightRingThickness * zoom / lightParticleTex.Width;
				float scaleY = arcLen / srcHeight;

				float brightFactor = 0f;
				for (int b = 0; b < LightRingBrightBands; b++) {
					float offset = b * MathHelper.TwoPi / LightRingBrightBands;
					float phase = angle + offset + scrollOffset * LightRingBrightSpeed;
					brightFactor += (MathF.Sin(phase * 2f) * 0.5f + 0.5f) * (1f / LightRingBrightBands);
				}
				brightFactor = MathHelper.Lerp(1f, 1f + LightRingBrightIntensity, brightFactor);

				Color finalColor = new Color(
					(int)(LightRingBaseColor.R * brightFactor),
					(int)(LightRingBaseColor.G * brightFactor),
					(int)(LightRingBaseColor.B * brightFactor),
					(int)(LightRingBaseColor.A * ringAlpha)
				);

				sb.Draw(lightParticleTex, pos, sourceRect, finalColor,
					rotation, new Vector2(0, 0.5f), new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
			}

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
		}


		public override void PostDraw(Color lightColor) {
			if (!isExploding)
				return;

			if (explosionEffect != null) {
				Main.spriteBatch.End();
				explosionEffect.Draw(Projectile.Center, explodeTimer);
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
					SamplerState.PointClamp, DepthStencilState.None,
					RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}
		}
	}


	public class InstinctBoundNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		private int boundTimer;
		private int tickTimer;
		private float damageMult;
		private float impairRatio;
		private float knockback;
		private int ownerIndex;

		// 音效路径
		private const string CatAttackSoundPath = "ArknightsMod/Assets/Sound/Tragodia/CatAttack";

		public void StartTick(NPC npc, int owner, float dmgMult, float impRatio, float kb) {
			boundTimer = 360;
			tickTimer = 14;
			ownerIndex = owner;
			damageMult = dmgMult;
			impairRatio = impRatio;
			knockback = kb;
		}

		public override void PostAI(NPC npc) {
			if (boundTimer <= 0)
				return;

			boundTimer--;
			tickTimer++;

			if (tickTimer >= 15) {
				tickTimer = 0;

				Player player = Main.player[ownerIndex];
				if (player == null || !player.active)
					return;

				int weaponDamage = player.HeldItem?.damage ?? 1;
				int damage = (int)(weaponDamage * damageMult);

				npc.StrikeNPC(new NPC.HitInfo {
					Damage = damage,
					Knockback = knockback,
					HitDirection = npc.Center.X > player.Center.X ? 1 : -1,
					DamageType = DamageClass.Magic
				});

				Vector2 spawnPos = npc.Center + new Vector2(0, npc.height * 0.3f);

				Projectile.NewProjectile(
					npc.GetSource_FromThis(),
					spawnPos,
					Vector2.Zero,
					ModContent.ProjectileType<Light>(),
					0, 0f, ownerIndex);

				int nerveValue = (int)(weaponDamage * impairRatio);
				if (nerveValue > 0)
					npc.GetGlobalNPC<AfflictionGlobalNPC>().Container
						.AddAfflictionValue<NervousImpairment>(nerveValue);

				// 播放DOT伤害音效
				PlayCatAttackSound(npc.Center);
			}
		}

		private void PlayCatAttackSound(Vector2 position) {
			SoundStyle catAttackSound = new SoundStyle($"{CatAttackSoundPath}") {
				Volume = 0.5f,
				PitchVariance = 0.15f,
				MaxInstances = 8,
			};
			SoundEngine.PlaySound(catAttackSound, position);
		}
	}
}