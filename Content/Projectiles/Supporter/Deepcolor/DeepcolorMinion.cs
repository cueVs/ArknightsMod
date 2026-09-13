using System;
using System.Collections.Generic;
using ArknightsMod.Common.GlobalProjectiles;
using ArknightsMod.Content.Buffs.Supporter.Deepcolor;
using ArknightsMod.Content.Items.Weapons.Supporter.Deepcolor;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Supporter.Deepcolor
{
	// 塔防式固定召唤物
	public class DeepcolorMinion : ModProjectile
	{
		public const int MaxTentacles = 4;
		public const int FrameWidth = 40;
		public const int FrameHeight = 40;
		public const int GroundOverlapPixels = 2;
		// 仅补偿贴图帧底透明边，0 = 脚底对齐方块顶面
		public const int VisualFootSinkPixels = 0;
		public const int TotalFrameCount = 21;
		public const int AttackFrameStart = 0;
		public const int AttackFrameCount = 11;
		public const int IdleFrameStart = 11;
		public const int IdleFrameCount = 10;

		private const int AttackTicksPerFrame = 3;
		private const int IdleTicksPerFrame = 6;
		// 半圆弧索敌基础半径（4 格）；二技能再 +5 格
		public const float BaseAttackRangeRadiusPx = 64f;
		private const int AttackCooldownMax = 45;
		private const int AttackHitFrameStart = AttackFrameStart + 4;
		private const int AttackDrawExtendRight = 12;
		// 攻击帧触手根部 X（贴图内）；待机用帧几何中心
		private const float AttackFramePivotX = 10f;
		private const float IdleFramePivotX = FrameWidth * 0.5f;
		private const int AttackDrawCullingPadding = 192;

		private ref float IdleAnimTimer => ref Projectile.ai[0];
		private ref float AttackAnimTimer => ref Projectile.ai[1];
		private ref float AttackCooldownTimer => ref Projectile.ai[2];
		private bool IsAttacking => AttackAnimTimer > 0f;
		private bool dealtAttackHitThisSwing;
		private int attackFacingDirection = 1;

		public override void SetStaticDefaults() {
			Main.projFrames[Type] = TotalFrameCount;
			ProjectileID.Sets.MinionSacrificable[Type] = true;
			// 不启用 MinionTargetingFeature：右键需留给速写 LOGO 攻击，否则原版会拦截物品使用且 Shoot 不会触发。
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = AttackDrawCullingPadding;
		}

		public override void SetDefaults() {
			Projectile.width = FrameWidth;
			Projectile.height = FrameHeight;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.minionSlots = 0f;
			Projectile.penetrate = -1;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = false;
			Projectile.aiStyle = -1;
		}

		public override void OnSpawn(IEntitySource source) {
			Projectile.velocity = Vector2.Zero;
			SnapToGround();
			Projectile.tileCollide = false;

			// 每只触手独立动画相位，避免同时同帧
			IdleAnimTimer = Main.rand.Next(IdleFrameCount * IdleTicksPerFrame);
			AttackAnimTimer = 0f;
			AttackCooldownTimer = Main.rand.Next(AttackCooldownMax / 2);
			Projectile.frame = IdleFrameStart + ((int)IdleAnimTimer / IdleTicksPerFrame) % IdleFrameCount;

			var life = Projectile.MinionLife();
			life.useLife = true;
			life.lifeMax = DeepcolorMinionLifeGlobalProj.DefaultLifeMax;
			life.life = life.lifeMax;
			life.defense = DeepcolorMinionLifeGlobalProj.DefaultDefense;
			life.drawHealthBar = true;
			Projectile.Relocate().BeginStable(Projectile);
		}

		public static int CountActiveForPlayer(Player player) {
			int type = ModContent.ProjectileType<DeepcolorMinion>();
			int count = 0;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile proj = Main.projectile[i];
				if (proj.active && proj.owner == player.whoAmI && proj.type == type)
					count++;
			}
			return count;
		}

		// 满员时移除该玩家最早召唤的一只触手
		public static bool TryDespawnOldestForPlayer(Player player) {
			int type = ModContent.ProjectileType<DeepcolorMinion>();
			Projectile oldest = null;
			int oldestOrder = int.MaxValue;

			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile proj = Main.projectile[i];
				if (!proj.active || proj.owner != player.whoAmI || proj.type != type)
					continue;

				int order = proj.MinionLife().spawnOrder;
				if (order >= oldestOrder)
					continue;

				oldestOrder = order;
				oldest = proj;
			}

			if (oldest == null)
				return false;

			DespawnWithDeathEffect(oldest);
			return true;
		}

		public static void DespawnWithDeathEffect(Projectile projectile) {
			if (!projectile.active || projectile.type != ModContent.ProjectileType<DeepcolorMinion>())
				return;

			SpawnDeathParticles(projectile);
			SoundEngine.PlaySound(SoundID.NPCDeath4, projectile.Center);
			projectile.Kill();
		}

		public override bool? CanCutTiles() => false;

		public override bool MinionContactDamage() => false;

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.SetDealContactCooldown();
			// 与召唤主共用免疫槽，避免贴脸时同帧多次结算
			target.immune[Projectile.owner] = DeepcolorMinionLifeGlobalProj.DealContactDamageCooldownMax;
		}

		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			if (!CheckActive(owner)) {
				Projectile.Kill();
				return;
			}

			Projectile.velocity = Vector2.Zero;
			var relocate = Projectile.Relocate();

			if (!relocate.IsTransitioning)
				SnapToGround();

			DeepcolorMinionRelocate.UpdateRelocate(Projectile, owner, IsAttacking);

			if (relocate.IsTransitioning) {
				if (!IsAttacking)
					UpdateIdleAnimation();
				return;
			}

			bool targetInRange = TryGetClosestTarget(owner, out Vector2 targetCenter);

			if (targetInRange)
				UpdateSpriteDirection(targetCenter);

			if (targetInRange && AttackCooldownTimer <= 0f && !IsAttacking)
				StartAttack(targetCenter);

			if (IsAttacking) {
				if (!targetInRange)
					Projectile.spriteDirection = attackFacingDirection;
				UpdateAttackAnimation();
			}
			else
				UpdateIdleAnimation();

			if (AttackCooldownTimer > 0f)
				AttackCooldownTimer--;
		}

		private static bool CheckActive(Player owner) {
			if (owner.dead || !owner.active)
				return false;
			return owner.HasBuff<DeepcolorMinionBuff>();
		}

		// 左—上—右半圆弧内索敌，不攻击正下方
		public bool IsInAttackRange(NPC npc) {
			if (!npc.CanBeChasedBy(Projectile))
				return false;

			Player owner = Main.player[Projectile.owner];
			return DeepcolorSketchSkills.IsInAttackRangeAt(Projectile, npc.Center, owner);
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			Player owner = Main.player[Projectile.owner];
			if (DeepcolorSketchSkills.ShadowTentacleActive(owner))
				modifiers.SourceDamage.Base *= DeepcolorSketchSkills.ShadowTentacleDamageMult;
		}

		private bool TryGetClosestTarget(Player owner, out Vector2 targetCenter) {
			targetCenter = Projectile.Center;
			bool found = false;
			float bestDist = 0f;

			if (owner.HasMinionAttackTargetNPC && owner.MinionAttackTargetNPC >= 0 && owner.MinionAttackTargetNPC < Main.maxNPCs)
				ConsiderTarget(Main.npc[owner.MinionAttackTargetNPC], ref found, ref bestDist, ref targetCenter);

			for (int i = 0; i < Main.maxNPCs; i++)
				ConsiderTarget(Main.npc[i], ref found, ref bestDist, ref targetCenter);

			return found;
		}

		private void ConsiderTarget(NPC npc, ref bool found, ref float bestDist, ref Vector2 targetCenter) {
			if (!npc.active || !IsInAttackRange(npc))
				return;

			float dist = Vector2.Distance(npc.Center, Projectile.Center);
			if (!found || dist < bestDist) {
				found = true;
				bestDist = dist;
				targetCenter = npc.Center;
			}
		}

		private void StartAttack(Vector2 targetCenter) {
			UpdateSpriteDirection(targetCenter);
			attackFacingDirection = Projectile.spriteDirection;
			AttackAnimTimer = 1f;
			Projectile.frame = AttackFrameStart;
			dealtAttackHitThisSwing = false;
		}

		private void UpdateAttackAnimation() {
			AttackAnimTimer++;
			if (AttackAnimTimer % AttackTicksPerFrame != 0)
				return;

			Projectile.frame++;
			if (!dealtAttackHitThisSwing && Projectile.frame == AttackHitFrameStart)
				StrikeTargetsInRange();

			if (Projectile.frame >= AttackFrameStart + AttackFrameCount) {
				AttackAnimTimer = 0f;
				AttackCooldownTimer = AttackCooldownMax;
				Projectile.frame = IdleFrameStart + ((int)IdleAnimTimer / IdleTicksPerFrame) % IdleFrameCount;
			}
		}

		private void StrikeTargetsInRange() {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;
			if (!Projectile.CanDealContactDamage())
				return;

			Player owner = Main.player[Projectile.owner];
			bool hitAny = false;

			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (!npc.active || !npc.CanBeChasedBy(Projectile) || !IsInAttackRange(npc))
					continue;
				if (npc.immune[Projectile.owner] > 0)
					continue;

				NPC.HitModifiers modifiers = new NPC.HitModifiers {
					DamageType = Projectile.DamageType,
				};
				ModifyHitNPC(npc, ref modifiers);

				bool crit = Main.rand.Next(100) < owner.GetTotalCritChance(Projectile.DamageType);
				NPC.HitInfo hit = modifiers.ToHitInfo(Projectile.damage, crit, Projectile.knockBack, damageVariation: true);
				hit.HitDirection = npc.Center.X >= Projectile.Center.X ? 1 : -1;

				npc.StrikeNPC(hit);
				OnHitNPC(npc, hit, hit.Damage);
				hitAny = true;
			}

			if (hitAny)
				Projectile.SetDealContactCooldown();
		}

		private void UpdateIdleAnimation() {
			IdleAnimTimer++;
			if (IdleAnimTimer % IdleTicksPerFrame != 0)
				return;

			int idleIndex = ((int)IdleAnimTimer / IdleTicksPerFrame) % IdleFrameCount;
			Projectile.frame = IdleFrameStart + idleIndex;
		}

		private void UpdateSpriteDirection(Vector2 lookAt) {
			int face = lookAt.X >= Projectile.Center.X ? 1 : -1;
			Projectile.spriteDirection = face;
			Projectile.direction = face;
		}

		private static bool IsAttackFrame(int frame) => frame >= AttackFrameStart && frame < AttackFrameStart + AttackFrameCount;

		public float VisualFootWorldY => Projectile.Bottom.Y + VisualFootSinkPixels;

		public static float GetGroundSurfaceY(float worldX, float searchFromWorldY) {
			int tileX = (int)(worldX / 16f);
			int startTileY = (int)((searchFromWorldY + 4f) / 16f);
			tileX = Utils.Clamp(tileX, 1, Main.maxTilesX - 2);
			startTileY = Utils.Clamp(startTileY, 1, Main.maxTilesY - 2);

			for (int tileY = startTileY; tileY < Main.maxTilesY; tileY++) {
				if (!WorldGen.SolidTile(tileX, tileY))
					continue;
				return tileY * 16f;
			}

			return searchFromWorldY;
		}

		public static bool TryGetStandTile(float worldX, float groundSurfaceY, out int tileX, out int tileY, out Tile tile) {
			tileX = (int)(worldX / 16f);
			tileY = (int)(groundSurfaceY / 16f);
			tile = default;

			if (!WorldGen.InWorld(tileX, tileY, 2))
				return false;

			tile = Main.tile[tileX, tileY];
			if (tile.HasTile)
				return true;

			tileY++;
			if (!WorldGen.InWorld(tileX, tileY, 2))
				return false;

			tile = Main.tile[tileX, tileY];
			return tile.HasTile;
		}

		public static void SpawnTransitionDustSurface(float worldX, float groundSurfaceY) {
			if (Main.netMode == NetmodeID.Server)
				return;

			if (!TryGetStandTile(worldX, groundSurfaceY, out int tileX, out int tileY, out Tile tile))
				return;

			int amount = WorldGen.KillTile_GetTileDustAmount(fail: true, tile, tileX, tileY);
			amount = Math.Clamp(amount, 2, 5);

			for (int i = 0; i < amount; i++)
				WorldGen.KillTile_MakeTileDust(tileX, tileY, tile);
		}

		public static void SpawnGroundTileDust(float worldX, float groundSurfaceY) {
			SpawnTransitionDustSurface(worldX, groundSurfaceY);
		}

		// 统一绘制：世界锚点始终在碰撞箱中心脚底；仅按状态切换贴图内枢轴与翻转
		private void GetSpriteDrawState(
			Texture2D texture,
			out Rectangle source,
			out Vector2 origin,
			out SpriteEffects effects) {
			source = new Rectangle(0, Projectile.frame * FrameHeight, FrameWidth, FrameHeight);
			effects = SpriteEffects.None;

			if (!IsAttacking) {
				origin = new Vector2(IdleFramePivotX, FrameHeight);
				return;
			}

			origin = new Vector2(AttackFramePivotX, FrameHeight);
			if (Projectile.spriteDirection < 0) {
				effects = SpriteEffects.FlipHorizontally;
				return;
			}

			int extraWidth = Math.Min(AttackDrawExtendRight, Math.Max(0, texture.Width - FrameWidth));
			if (extraWidth > 0)
				source.Width = FrameWidth + extraWidth;
		}

		public override bool PreDraw(ref Color lightColor) {
			var relocate = Projectile.Relocate();
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			float relocateOffset = relocate.RelocateVisualOffsetY;
			float footY = VisualFootWorldY + relocateOffset + Projectile.gfxOffY;
			Player owner = Main.player[Projectile.owner];
			float drawScale = DeepcolorSketchSkills.VisualTrapActive(owner) ? DeepcolorSketchSkills.VisualTrapDrawScale : 1f;
			float maskGroundY = relocate.AnchorGroundY;

			GetSpriteDrawState(texture, out Rectangle source, out Vector2 origin, out SpriteEffects effects);
			Vector2 drawPos = new Vector2(Projectile.Center.X, footY) - Main.screenPosition;

			if (relocate.IsTransitioning) {
				if (!ApplyGroundMaskCrop(ref source, ref origin, ref drawPos, footY, maskGroundY, drawScale))
					return false;
			}

			Main.EntitySpriteDraw(texture, drawPos, source, lightColor, Projectile.rotation, origin, drawScale, effects, 0);
			return false;
		}

		// 整体平移 + 固定地面线裁切底部：没入时先消失根部，而非压缩贴图
		private static bool ApplyGroundMaskCrop(
			ref Rectangle source,
			ref Vector2 origin,
			ref Vector2 drawPos,
			float footWorldY,
			float maskGroundY,
			float drawScale) {
			float fullDrawHeight = source.Height * drawScale;
			float hiddenBelowGround = Math.Max(0f, footWorldY - maskGroundY);

			if (hiddenBelowGround <= 0.5f)
				return true;

			float visibleWorldHeight = fullDrawHeight - hiddenBelowGround;
			if (visibleWorldHeight <= 0.5f)
				return false;

			int visibleSrcHeight = Math.Max(1, (int)MathF.Ceiling(visibleWorldHeight / drawScale));
			visibleSrcHeight = Math.Min(visibleSrcHeight, source.Height);

			// 保留帧顶部，裁掉没入地下的部分
			source.Height = visibleSrcHeight;
			origin.Y = visibleSrcHeight;

			float visibleFootY = footWorldY - hiddenBelowGround;
			drawPos.Y += visibleFootY - footWorldY;

			return true;
		}

		private void SnapToGround() {
			int tileX = (int)(Projectile.Center.X / 16f);
			int startTileY = (int)((Projectile.Bottom.Y + 2f) / 16f);
			tileX = Utils.Clamp(tileX, 1, Main.maxTilesX - 2);
			startTileY = Utils.Clamp(startTileY, 1, Main.maxTilesY - 2);

			for (int tileY = startTileY; tileY < Main.maxTilesY; tileY++) {
				if (!WorldGen.SolidTile(tileX, tileY))
					continue;

				Projectile.position.Y = tileY * 16f - Projectile.height;
				return;
			}
		}

		public static Vector2 FindGroundSpawnPosition(Vector2 worldPosition, int width, int height) {
			int tileX = (int)(worldPosition.X / 16f);
			int startTileY = (int)(worldPosition.Y / 16f);
			tileX = Utils.Clamp(tileX, 1, Main.maxTilesX - 2);
			startTileY = Utils.Clamp(startTileY, 1, Main.maxTilesY - 2);

			for (int tileY = startTileY; tileY < Main.maxTilesY - 1; tileY++) {
				Tile tile = Main.tile[tileX, tileY];
				Tile tileAbove = tileY > 0 ? Main.tile[tileX, tileY - 1] : tile;

				if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && (!tileAbove.HasUnactuatedTile || !Main.tileSolid[tileAbove.TileType])) {
					float spawnX = tileX * 16f + 8f;
					float spawnY = tileY * 16f - height;
					return new Vector2(spawnX, spawnY);
				}
			}

			for (int tileY = startTileY; tileY < Main.maxTilesY; tileY++) {
				if (WorldGen.SolidTile(tileX, tileY)) {
					float spawnX = tileX * 16f + 8f;
					float spawnY = tileY * 16f - height;
					return new Vector2(spawnX, spawnY);
				}
			}

			return worldPosition;
		}

		public static void SpawnDeathParticles(Projectile projectile) {
			if (Main.netMode == NetmodeID.Server)
				return;

			Texture2D texture;
			try {
				texture = ModContent.Request<Texture2D>(projectile.ModProjectile.Texture, AssetRequestMode.ImmediateLoad).Value;
			}
			catch {
				return;
			}

			int frame = projectile.frame;
			Rectangle frameRect = new(0, frame * FrameHeight, FrameWidth, FrameHeight);
			Color[] pixels = new Color[FrameWidth * FrameHeight];
			try {
				texture.GetData(0, frameRect, pixels, 0, pixels.Length);
			}
			catch {
				return;
			}

			List<Color> palette = [];
			for (int attempt = 0; attempt < 120 && palette.Count < 16; attempt++) {
				int x = Main.rand.Next(FrameWidth);
				int y = Main.rand.Next(FrameHeight);
				Color sample = pixels[y * FrameWidth + x];
				if (sample.A <= 40)
					continue;

				bool exists = false;
				foreach (Color c in palette) {
					if (c == sample) {
						exists = true;
						break;
					}
				}

				if (!exists)
					palette.Add(sample);
			}

			if (palette.Count == 0)
				return;

			Vector2 center = projectile.Center;
			for (int i = 0; i < 24; i++) {
				Color dustColor = palette[Main.rand.Next(palette.Count)];
				Vector2 offset = Main.rand.NextVector2Circular(10f, 10f);
				Dust dust = Dust.NewDustPerfect(center + offset, DustID.Smoke, Scale: Main.rand.NextFloat(0.9f, 1.5f));
				dust.color = dustColor;
				dust.noGravity = true;
				dust.fadeIn = 1.1f;
				dust.velocity = offset.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(1.2f, 3.2f);
				if (dust.velocity == Vector2.Zero)
					dust.velocity = Main.rand.NextVector2Circular(2.5f, 2.5f);
			}

			for (int i = 0; i < 8; i++) {
				Color dustColor = palette[Main.rand.Next(palette.Count)];
				Dust dust = Dust.NewDustPerfect(center, DustID.Cloud, Scale: Main.rand.NextFloat(0.7f, 1.1f));
				dust.color = dustColor;
				dust.noGravity = true;
				dust.velocity = Main.rand.NextVector2Circular(2f, 2f);
			}
		}
	}
}
