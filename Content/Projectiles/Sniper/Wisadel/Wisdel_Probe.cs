using ArknightsMod.Common;
using ArknightsMod.Common.Particle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WisdelItem = ArknightsMod.Content.Items.Weapons.Sniper.Wisadel.WisadelCannon;

namespace ArknightsMod.Content.Projectiles.Sniper.Wisadel
{
    public class Wisdel_Probe : ModProjectile
    {
		#region 音效
		/// <summary>
		/// 部署召唤
		/// </summary>
		public static SoundStyle Summon = new(ArknightsMod.PathProjectileExclusives + "Sniper/Wisadel/WisdelCannon/WisdelSummon");
		/// <summary>
		/// 普攻发射
		/// </summary>
		public static SoundStyle Shoot = new(ArknightsMod.PathProjectileExclusives + "Sniper/Wisadel/WisdelCannon/WisdelShoot");
		/// <summary>
		/// 普攻命中
		/// </summary>
		public static SoundStyle ShootBlast = new(ArknightsMod.PathProjectileExclusives + "Sniper/Wisadel/WisdelCannon/WisdelShootBlast");
		/// <summary>
		/// 普攻重新装填
		/// </summary>
		public static SoundStyle ShootReload = new(ArknightsMod.PathProjectileExclusives + "Sniper/Wisadel/WisdelCannon/WisdelReload");
		/// <summary>
		/// 技能开启
		/// </summary>
		public static SoundStyle SkillActivate = new(ArknightsMod.PathProjectileExclusives + "Sniper/Wisadel/WisdelCannon/SkillActivate");
		/// <summary>
		/// 三技能锁定
		/// </summary>
		public static SoundStyle Aim = new(ArknightsMod.PathProjectileExclusives + "Sniper/Wisadel/WisdelCannon/WisdelAim");
		/// <summary>
		/// 三技能爆破
		/// </summary>
		public static SoundStyle Explode = new(ArknightsMod.PathProjectileExclusives + "Sniper/Wisadel/WisdelCannon/WisdelBoom");
		#endregion
		public override string Texture => ArknightsMod.noTexture;
		public override void SetStaticDefaults() { }
        public override void SetDefaults()
        {
            Projectile.width = 44;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.hide = false;
            Projectile.tileCollide = false;
			Projectile.DamageType = DamageClass.Ranged;
        }
        public override bool? CanDamage() => false;

		/// <summary>
		/// Style：浮游炮的种类，决定贴图以及方位
		/// </summary>
		public ref float Style {
			get {
				return ref Projectile.ai[0];
			}
		}

		/// <summary>
		/// 旋转
		/// </summary>
		public float Rotation;

		/// <summary>
		/// 组合后相对位置
		/// </summary>
		public float RotationCombined;

		/// <summary>
		/// 位置
		/// </summary>
		public Vector2 Position;

		internal Player player => Main.player[Projectile.owner];
		public float AttackSpeed => player.GetTotalAttackSpeed(Projectile.DamageType);

		/// <summary>
		/// 应用攻速加成到时间值
		/// </summary>
		/// <param name="baseTime">基础时间</param>
		/// <returns>应用攻速后的时间</returns>
		protected int ApplyAttackSpeed(float baseTime) {
			return Math.Max(1, (int)(baseTime / AttackSpeed));
		}
		public Vector2 GetAssembledPosition()
		{
			Vector2 pos = Style switch {
				0 => new Vector2(0, 0),
				1 => new Vector2(-23, 9),
				2 => new Vector2(-47, -1),
				3 => new Vector2(-27, -6),
				_ => Vector2.Zero
			};
			return pos;
		}
		/// <summary>
		/// 三技能弹药数量，回来改成技能系统适配
		/// </summary>
		public static int MaxAmmoSkill3 = 6;

		/// <summary>
		/// 已经消耗弹药数
		/// </summary>
		public int AmmoConsumed = 0;
		public override void OnSpawn(IEntitySource source)
		{
			// 重置所有状态，但是攻击冷却和攻击模式不重置，防止玩家刷新CD
			Player player = Main.player[Projectile.owner];
			player.wisdel().currentUse = 0;
			player.wisdel().channelTimer = 0;
			player.wisdel().modeSwitchCooldown = 0;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			Player player = Main.player[Projectile.owner];

			// 当玩家瞄准发射时，将浮游炮显示到玩家前面
			if (player.controlUseItem && player.wisdel().currentUse == Style
				|| player.wisdel().mode == 1)
			{
				overPlayers.Add(index);
			}
		}
		public override void AI()
        {
            Player player = Main.player[Projectile.owner];
			ref int Mode = ref player.wisdel().mode;

			// 有效性检测
			ValidCheck(player);

			// 常态下：浮游炮位置
			if (Mode == 0)
			{
				int style = Style switch {
					0 => 0,
					3 => 1,
					2 => 2,
					1 => 3,
					_ => 0
				};
				Position = player.RotatedRelativePoint(player.MountedCenter
					+ new Vector2(Style == 2 ? 30 : 40, 0).RotatedBy(MathHelper.PiOver4 + style * MathHelper.PiOver2)
					+ new Vector2(-10 * Projectile.spriteDirection, 0).RotatedBy(Rotation));
			}
			// 三技能情况下：组合
			else if (Mode == 1)
			{
				Vector2 MouseWorld = Main.MouseWorld;
				Vector2 playerPos = player.Center;
				Vector2 toMouse = MouseWorld - playerPos;
				float minDistance = 90f;

				// 限制最小距离
				if (toMouse.Length() < minDistance) {
					toMouse = toMouse.SafeNormalize(Vector2.UnitX) * minDistance;
					MouseWorld = playerPos + toMouse;
					toMouse = MouseWorld - playerPos;
				}

				Vector2 mouseDir = (MouseWorld - Projectile.Center)
								.SafeNormalize(default);

				Vector2 pos = GetAssembledPosition();

				int mouseDir2D = MouseWorld.X - player.Center.X > 0 ? 1 : -1;
				if (mouseDir2D == -1) {
					pos = new Vector2(pos.X, -pos.Y);
				}

				player.ChangeDir(mouseDir2D);

				// 玩家手臂
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full,
					Projectile.rotation - MathHelper.PiOver2);

				
				float offset = EaseFunction.QuadraticEase(player.wisdel().combineAnimationTimer, 0, 30, -0.4f, 0f, false, 0.9f);
				float totalRot = mouseDir.ToRotation() + offset * player.direction;

				Position = player.RotatedRelativePoint(player.MountedCenter
					+ (pos - new Vector2(-40, 2 * mouseDir2D)).RotatedBy(totalRot));

				Rotation = totalRot;
				Projectile.direction = 1;
				player.wisdel().currentUse = 0;
			}

			// 需要弹幕所有者操作的部分，确保只有弹幕所有者处理鼠标输入
			if (Main.myPlayer == Projectile.owner)
			{
				switch (Mode)
				{
					case 0:
						Mode1_ProbeMode(player);
						break;
					case 1:
						Mode3_CannonMode(player);
						break;
				}
				// 冷却时玩家状态
				if (player.wisdel().coolDown > 0)
				{
					player.itemTime = player.itemAnimation = 2;
					Projectile.netUpdate = true;
					player.wisdel().channelTimer = 0;
				}

				Projectile.netUpdate = true;
				Projectile.damage = player.GetWeaponDamage(player.HeldItem);
			}

			// 统一方向
			Projectile.spriteDirection = Projectile.direction;

			// 缓动更新位置
			float posLerp = Mode == 1 ? 0.5f : 0.2f;
			Projectile.Center = new Vector2(MathHelper.Lerp(Projectile.Center.X, Position.X, posLerp),
											MathHelper.Lerp(Projectile.Center.Y, Position.Y, posLerp));
			// 缓动更新旋转
			float leftMouseLerp = player.controlUseItem ? Math.Min(1f, 0.12f * AttackSpeed) : 0.1f;
			float rotLerp = Mode == 1 ? 1f : leftMouseLerp;
			Projectile.rotation = MathHelper.Lerp(Projectile.rotation, Rotation, rotLerp);
		}
		/// <summary>
		/// 弹幕是否有效
		/// </summary>
		/// <param name="player"></param>
		public void ValidCheck(Player player)
		{
			// 玩家非法的时候直接销毁
			if (!player.active || player.dead || player.ghost) {
				Projectile.Kill();
			}

			if (player.HeldItem.ModItem is WisdelItem) {
				Projectile.timeLeft = 2;
			}
		}
		public void Mode1_ProbeMode(Player player)
		{
			// 左键状态
			if (player.controlUseItem) {
				if (player.wisdel().currentUse == Style) {
					if (player.wisdel().coolDown == 0) {
						// 瞄准和发射
						UpdateNormalAiming(player);

						// 玩家手臂挥舞
						player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full,
							Projectile.rotation - MathHelper.PiOver2 * player.direction);
					}
					else {
						player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full,
							MathHelper.PiOver4 * player.direction);
					}
				}
				else {
					ResetToDefaultState(player);
				}
			}
			// 右键状态
			else if (player.controlUseTile) {
				// 模式切换
				ModeSwitch(player);
			}

			// 待机状态
			else {
				player.wisdel().channelTimer = 0;
				// 重置状态
				ResetToDefaultState(player);
			}
		}
		public void Mode3_CannonMode(Player player) {
			if (AmmoConsumed >= MaxAmmoSkill3) {
				AmmoConsumed = 0;
				ModeSwitch(player);
			}
			// 左键状态
			if (player.controlUseItem)
			{
				if (player.wisdel().currentUse == Style)
				{
					if (player.wisdel().coolDown == 0)
					{
						// 瞄准和发射
						UpdateCannonAiming(player);
					}
				}
				else
				{
				}
			}
			// 右键状态
			else if (player.controlUseTile) {
				// 模式切换
				ModeSwitch(player);
			}
			// 待机状态
			else
			{
				UpdateCannonShoot(player);
				player.wisdel().channelTimer = 0;
			}
		}
		public void ModeSwitch(Player player)
		{
			if (player.wisdel().modeSwitchCooldown == 0)
			{
				Projectile.spriteDirection = 1;
				Projectile.direction = 1;
				player.wisdel().mode++;
				if (player.wisdel().mode == 1) {
					SoundEngine.PlaySound(SkillActivate, player.Center);
				}
				else {
					SoundEngine.PlaySound(SoundID.MenuTick, player.Center);
				}
				if (player.wisdel().mode > 1)
				{
					player.wisdel().mode = 0;
				}
				
				player.wisdel().modeSwitchCooldown = 30;
			}
		}
		/// <summary>
		/// 重置到待机状态
		/// </summary>
		public void ResetToDefaultState(Player player)
		{
			// 固定角度
			float rot = Style switch {
				0 => MathHelper.PiOver4,
				3 => MathHelper.PiOver4 * 3,
				2 => MathHelper.PiOver4,
				1 => -MathHelper.PiOver4,
				_ => MathHelper.PiOver4
			};
			Rotation = rot + (Projectile.direction == -1 ? MathHelper.Pi : 0);

			// 设置速度
			Projectile.velocity = (Projectile.velocity * 20 + player.velocity.SafeNormalize(default)) / 21f;

			// 待机状态下会随机出现位置扰动
			Vector2 disturbDirection = Style switch {
				0 => new Vector2(1, -1),
				3 => new Vector2(-1, 1),
				2 => new Vector2(1, 1),
				1 => new Vector2(-1, -1),
				_ => new Vector2(1, -1)
			};

			float random = Main.rand.NextFloat(12, 36);
			float disturbStrength = EaseFunction.SineEase(
				x: (float)Main.timeForVisualEffects + Style * MathHelper.PiOver4,
				yMin: -20f,
				yMax: 20f,
				frequency: 1f) / random;

			Position += disturbDirection * disturbStrength;
		}

		/// <summary>
		/// 普攻瞄准
		/// </summary>
		/// <param name="player"></param>
		public void UpdateNormalAiming(Player player) {

			// 瞄准时浮游炮强制转向鼠标位置
			Vector2 mouseDir = (Main.MouseWorld - Projectile.Center)
								.SafeNormalize(default);
			Projectile.velocity = mouseDir;
			Rotation = MathF.Atan2(mouseDir.Y * Projectile.direction, mouseDir.X * Projectile.direction);
			player.ChangeDir(Main.MouseWorld.X - player.Center.X > 0 ? 1 : -1);
			UpdateNormalShoot(player);

		}

		/// <summary>
		/// 普攻发射
		/// </summary>
		/// <param name="player"></param>
		public void UpdateNormalShoot(Player player)
		{
			// 增加蓄力时间
			player.wisdel().channelTimer++;

			// 最大蓄力时间
			int channelTimeMax = ApplyAttackSpeed(player.HeldItem.useTime * 0.67f);

			if (player.wisdel().channelTimer == (int)(channelTimeMax * 0.5f)) {
				if (Main.netMode != NetmodeID.Server) {
					SoundEngine.PlaySound(ShootReload, Projectile.position);
				}
			}
			// 当超过蓄力时间，并且冷却结束：发射
			if (player.wisdel().channelTimer > channelTimeMax && player.wisdel().coolDown <= 0)
			{
				Projectile.netUpdate = true;

				// 客户端播放音效
				if (Main.netMode != NetmodeID.Server)
				{
					SoundEngine.PlaySound(Shoot, Projectile.position);
				}

				Vector2 playerPos = player.Center;
				Vector2 mouseWorld = Main.MouseWorld;
				Vector2 toMouse = mouseWorld - playerPos;
				Vector2 direction = toMouse.SafeNormalize(Vector2.UnitX);
				float checkStep = 4f;
				float checkLength = toMouse.Length();
				bool blocked = false;
				Vector2 blockedPos = mouseWorld;

				for (float i = 0f; i < checkLength; i += checkStep) {
					Vector2 checkPos = playerPos + direction * i;
					Point tile = checkPos.ToTileCoordinates();

					if (WorldGen.SolidTile(tile.X, tile.Y)) {
						blocked = true;
						blockedPos = playerPos + direction * (i - checkStep); // 上一个没被挡的位置
						break;
					}
				}

				if (blocked) {
					mouseWorld = blockedPos;
				}
				// 弹幕
				int p = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, (mouseWorld - Projectile.Center)
						.SafeNormalize(default) * 16,
					ModContent.ProjectileType<WisdelShotNormal>(), Projectile.damage, Projectile.knockBack,
					Projectile.owner);
				var shot = Main.projectile[p].ModProjectile as WisdelShotNormal;
				shot.aimPos = mouseWorld;
				// 统一冷却
				player.wisdel().coolDown = ApplyAttackSpeed(player.HeldItem.useTime);

				// 切换下一个浮游炮发射
				player.wisdel().currentUse++;
				if (player.wisdel().currentUse > 3)
					player.wisdel().currentUse = 0;
				return;
			}
		}

		/// <summary>
		/// 组合状态瞄准
		/// </summary>
		/// <param name="player"></param>
		public void UpdateCannonAiming(Player player)
		{
			if (player.wisdel().channelTimer == 1) {
				if (Main.netMode != NetmodeID.Server) {
					SoundEngine.PlaySound(Aim, Projectile.Center);
				}
			}
			player.wisdel().channelTimer++;
		}
		public void UpdateCannonShoot(Player player)
		{
			// 最大蓄力时间
			int channelTimeMax = ApplyAttackSpeed(player.HeldItem.useTime * 0.7f);

			// 当超过蓄力时间，并且冷却结束：发射
			if (player.wisdel().channelTimer > channelTimeMax && player.wisdel().coolDown <= 0) {
				Projectile.netUpdate = true;
				player.wisdel().combineAnimationTimer = 30;
				if (Main.netMode != NetmodeID.Server) {
					SoundEngine.PlaySound(Shoot.WithPitchOffset(-0.1f), Projectile.position);
				}
				player.velocity.X -= player.direction * 3;

				Vector2 playerPos = player.Center;
				Vector2 mouseWorld = Main.MouseWorld;
				Vector2 toMouse = mouseWorld - playerPos;
				Vector2 direction = toMouse.SafeNormalize(Vector2.UnitX);
				float checkStep = 4f;
				float checkLength = toMouse.Length();
				bool blocked = false;
				Vector2 blockedPos = mouseWorld;

				for (float i = 0f; i < checkLength; i += checkStep) {
					Vector2 checkPos = playerPos + direction * i;
					Point tile = checkPos.ToTileCoordinates();

					if (WorldGen.SolidTile(tile.X, tile.Y)) {
						blocked = true;
						blockedPos = playerPos + direction * (i - checkStep); // 上一个没被挡的位置
						break;
					}
				}

				if (blocked) {
					mouseWorld = blockedPos;
				}
				// 弹幕
				int p = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, (mouseWorld - Projectile.Center)
						.SafeNormalize(default) * 16,
					ModContent.ProjectileType<WisdelShotLarge>(), (int)(Projectile.damage * 6.558f), Projectile.knockBack,
					Projectile.owner);

				var shot = Main.projectile[p].ModProjectile as WisdelShotLarge;
				shot.aimPos = mouseWorld;

				Vector2 velocity = Main.MouseWorld - Projectile.Center;
				velocity.Normalize();
				Vector2 c = new Vector2(60 * Projectile.direction, 0).RotatedBy(Projectile.rotation);
				var circle = Projectile.NewProjectileDirect(null, Projectile.Center + c, velocity,
										 ModContent.ProjectileType<WisdelShotBigCircle>(), 0,
										 Projectile.knockBack, Projectile.owner,
										 30, 10);

				AmmoConsumed++;

				UpdateCannonParticle(velocity);

				// 统一冷却
				player.wisdel().coolDown = ApplyAttackSpeed(player.HeldItem.useTime * 3f);
			}
		}

		public void UpdateCannonParticle(Vector2 velocity)
		{
			for (int i = 0; i < 20; i++) {
				Vector2 pos = new Vector2(50 * Projectile.direction, 0).RotatedBy(Projectile.rotation);
				Vector2 vec = velocity;
				if (float.IsNaN(vec.X) || float.IsNaN(vec.Y)) {
					vec = -Vector2.UnitY;
				}
				float speedX = vec.X * 6 + Main.rand.NextFloat(-4f, 4.1f);
				float speedY = vec.Y * 6 + Main.rand.NextFloat(-4f, 4.1f);

				var particle = new DefaultParticle(Projectile.Center + pos,
				new Vector2(speedX, speedY), 120, Main.rand.NextFloat(0.2f, 0.5f) * 4f, new Color(249, 90, 100), true);
				particle.Deformation = new Vector2(0.25f, 1f);
				if (Main.rand.NextBool(2)) {
					particle.Scale = Main.rand.NextFloat(0.2f, 0.5f) * Main.rand.NextFloat(0.5f, 2f);
					if (particle.Scale < 1f) {
						particle.Velocity *= 2f;
						//particle.fadeIn = true;
						if (Main.rand.NextBool(5)) {
							float size = Math.Min(particle.Deformation.X, particle.Deformation.Y);
							particle.Deformation = new Vector2(size, size * 3);
						}
						else if (Main.rand.NextBool(5))
							particle.Deformation.Y /= 2;
					}
				}
				particle.Spawn();
			}
			for (int i = 0; i < 5; i++) {
				Vector2 pos = new Vector2(50 * Projectile.direction, 0).RotatedBy(Projectile.rotation);
				Vector2 vec = velocity;
				if (float.IsNaN(vec.X) || float.IsNaN(vec.Y)) {
					vec = -Vector2.UnitY;
				}
				float speedX = vec.X * 6 + Main.rand.NextFloat(-4f, 4.1f);
				float speedY = vec.Y * 6 + Main.rand.NextFloat(-4f, 4.1f);

				var particle = new DefaultParticleNonPre(Projectile.Center + pos,
				new Vector2(speedX, speedY), 120, Main.rand.NextFloat(0.2f, 0.5f) * 4f, Color.Black, true);
				particle.Deformation = new Vector2(0.25f, 1f);
				if (Main.rand.NextBool(2)) {
					particle.Scale = Main.rand.NextFloat(0.2f, 0.5f) * Main.rand.NextFloat(0.5f, 2f);
					if (particle.Scale < 1f) {
						particle.Velocity *= 2f;
						//particle.fadeIn = true;
						if (Main.rand.NextBool(5)) {
							float size = Math.Min(particle.Deformation.X, particle.Deformation.Y);
							particle.Deformation = new Vector2(size, size * 3);
						}
						else if (Main.rand.NextBool(5))
							particle.Deformation.Y /= 2;
					}
				}
				particle.Spawn();
			}
		}

		/// <summary>
		/// 范围内寻敌
		/// </summary>
		/// <param name="position">寻敌起点</param>
		/// <param name="maxRange">最大距离</param>
		/// <param name="checkCanHit">检查是否有效</param>
		/// <returns></returns>
		public NPC FindTargetWithinRange(Vector2 position, float maxRange, bool checkCanHit = false)
        {
            NPC result = null;
            float num = maxRange;
            for (int i = 0; i < 200; i++)
            {
                NPC nPC = Main.npc[i];
                if (nPC.CanBeChasedBy(this) && Projectile.localNPCImmunity[i] == 0 && (!checkCanHit || Collision.CanHitLine(position, Projectile.width, Projectile.height, nPC.position, nPC.width, nPC.height)))
                {
                    float num2 = Vector2.Distance(position, nPC.Center);
                    if (!(num <= num2))
                    {
                        num = num2;
                        result = nPC;
                    }
                }
            }

            return result;
        }

        public override bool PreDraw(ref Color lightColor) {

			Player player = Main.player[Projectile.owner];
			Texture2D texture = ModContent.Request<Texture2D>($"ArknightsMod/Content/Projectiles/Sniper/Wisadel/Wisdel_Probe{Style}").Value;
			Texture2D glowTex = ModContent.Request<Texture2D>($"ArknightsMod/Content/Projectiles/Sniper/Wisadel/Wisdel_Probe_Glow{Style}").Value;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			int mouseDir2D = Main.MouseWorld.X - player.Center.X > 0 ? 1 : -1;

			if (player.wisdel().mode == 1) {
				if (Main.myPlayer == Projectile.owner) {
					if (mouseDir2D == -1) {
						effects = SpriteEffects.FlipVertically;
					}
				}
			}

			Main.EntitySpriteDraw(texture, drawPos, null, lightColor, Projectile.rotation, texture.Size() * 0.5f,
				1f, effects);

			Main.EntitySpriteDraw(glowTex, drawPos, null, Color.White, Projectile.rotation, texture.Size() * 0.5f,
			   1f, effects);

			return false;
        }
    }
}
