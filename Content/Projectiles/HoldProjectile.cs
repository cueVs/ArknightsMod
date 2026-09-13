using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles
{
	/// <summary>
	/// 蓄力武器弹幕基类
	/// <para/>用于实现需要持续按住使用的武器
	/// <para/>支持普通攻击和三个技能的不同攻击逻辑
	/// <para/>提供攻速加成处理：传入的时间参数为基础值，使用 ApplyAttackSpeed() 方法应用攻速
	/// <para/>记得在InitializeProjectile()设置弹幕伤害属性
	/// </summary>
	public abstract class HoldProjectile : ModProjectile
	{
		/// <summary>
		/// 绑定的物品类型
		/// </summary>
		/// <returns></returns>
		public abstract int BindingItemType();

		/// <summary>
		/// 决定物品距离玩家中心的位置。十分复杂的一个参数，谨慎修改
		/// </summary>
		/// <returns></returns>
		public virtual float Speed() => 24f;
		/// <summary>
		/// 当前技能
		/// </summary>
		internal float CurrentSkill {
			get => Projectile.ai[2];
			set => Projectile.ai[2] = value;
		}
		/// <summary>
		/// 射击次数
		/// </summary>
		internal float ShootTimes {
			get => Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}
		/// <summary>
		/// 蓄力时间，每帧+1
		/// </summary>
		internal float Timer {
			get => Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		internal Player player => Main.player[Projectile.owner];
		internal WeaponPlayer modPlayer => player.GetModPlayer<WeaponPlayer>();

		/// <summary>
		/// 应用攻速加成到时间值
		/// </summary>
		/// <param name="baseTime">基础时间</param>
		/// <returns>应用攻速后的时间</returns>
		protected int ApplyAttackSpeed(float baseTime) {
			float attackSpeed = player.GetTotalAttackSpeed(Projectile.DamageType);
			return Math.Max(1, (int)(baseTime / attackSpeed));
		}

		/// <summary>
		/// 获取玩家的攻速倍率
		/// </summary>
		protected float GetAttackSpeedMultiplier() {
			return player.GetTotalAttackSpeed(Projectile.DamageType);
		}

		public sealed override void SetDefaults() {
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.ignoreWater = true;
			Projectile.hide = true;
			InitializeProjectile();
		}
		/// <summary>
		/// 在SetDefaults()后调用
		/// </summary>
		public virtual void InitializeProjectile() { }
		public sealed override bool? CanDamage() => false;

		public sealed override void AI() {
			OnAI_Beginning();

			if (player.dead || !player.active ||
				player.HeldItem.type != BindingItemType() || !player.channel) {
				Projectile.Kill();
				return;
			}

			OnAI_PreChannel();
			// 攻击设置
			if (player.channel) {
				Projectile.timeLeft = 2;
				Timer++;

				int baseUseTime = player.HeldItem.useTime;
				int baseReuseDelay = player.HeldItem.reuseDelay;

				if (modPlayer.SkillActive) {
					switch (modPlayer.Skill) {
						case 0:
							UpdateSkill_1(baseUseTime, baseReuseDelay);
							break;
						case 1:
							UpdateSkill_2(baseUseTime, baseReuseDelay);
							break;
						case 2:
							UpdateSkill_3(baseUseTime, baseReuseDelay);
							break;
					}
				}
				else {
					UpdateNormalAttack(baseUseTime, baseReuseDelay);
				}
			}
			OnAI_PostChannel();
			// 其他设置
			var vel = Vector2.Normalize(Main.MouseWorld - player.Center);
			Projectile.velocity = vel * Speed();
			Projectile.position = player.RotatedRelativePoint(player.MountedCenter, true) - Projectile.Size / 2f;
			Projectile.direction = Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
			Projectile.rotation = Projectile.velocity.ToRotation();
			if (Projectile.spriteDirection == -1)
				Projectile.rotation += MathHelper.Pi;
			player.ChangeDir(Projectile.direction);
			player.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * Projectile.direction, Projectile.velocity.X * Projectile.direction);
			player.heldProj = Projectile.whoAmI;
			player.itemTime = 2;
			player.itemAnimation = 2;

			OnAI_End();
		}

		/// <summary>
		/// 在AI()头部调用
		/// </summary>
		public virtual void OnAI_Beginning() {

		}
		/// <summary>
		/// 在AI()尾部调用
		/// </summary>
		public virtual void OnAI_End() {

		}
		/// <summary>
		/// 在AI()的蓄力逻辑前调用
		/// </summary>
		public virtual void OnAI_PreChannel() {

		}
		/// <summary>
		/// 在AI()的蓄力逻辑后调用
		/// </summary>
		public virtual void OnAI_PostChannel() {

		}
		/// <summary>
		/// 普攻时的逻辑
		/// <para/>使用ApplyAttackSpeed()方法来应用攻速加成
		/// <para/>示例：技能期间攻速为2倍，获取使用时间：
		/// <para/>int useTime = ApplyAttackSpeed(baseUseTime * 0.5f);
		/// </summary>
		/// <param name="baseUseTime">基础useTime（未应用攻速）</param>
		/// <param name="baseReuseDelay">基础reuseDelay（未应用攻速）</param>
		public virtual void UpdateNormalAttack(int baseUseTime, int baseReuseDelay) {
		}
		/// <summary>
		/// 一技能发动时的逻辑
		/// <para/>使用ApplyAttackSpeed()方法来应用攻速加成
		/// <para/>示例：技能期间攻速为2倍，获取使用时间：
		/// <para/>int useTime = ApplyAttackSpeed(baseUseTime * 0.5f);
		/// </summary>
		/// <param name="baseUseTime">基础useTime（未应用攻速）</param>
		/// <param name="baseReuseDelay">基础reuseDelay（未应用攻速）</param>
		public virtual void UpdateSkill_1(int baseUseTime, int baseReuseDelay) {
		}
		/// <summary>
		/// 二技能发动时的逻辑
		/// <para/>使用ApplyAttackSpeed()方法来应用攻速加成
		/// <para/>示例：技能期间攻速为2倍，获取使用时间：
		/// <para/>int useTime = ApplyAttackSpeed(baseUseTime * 0.5f);
		/// </summary>
		/// <param name="baseUseTime">基础useTime（未应用攻速）</param>
		/// <param name="baseReuseDelay">基础reuseDelay（未应用攻速）</param>
		public virtual void UpdateSkill_2(int baseUseTime, int baseReuseDelay) {
		}
		/// <summary>
		/// 三技能发动时的逻辑
		/// <para/>使用ApplyAttackSpeed()方法来应用攻速加成
		/// <para/>示例：技能期间攻速为2倍，获取使用时间：
		/// <para/>int useTime = ApplyAttackSpeed(baseUseTime * 0.5f);
		/// </summary>
		/// <param name="baseUseTime">基础useTime（未应用攻速）</param>
		/// <param name="baseReuseDelay">基础reuseDelay（未应用攻速）</param>
		public virtual void UpdateSkill_3(int baseUseTime, int baseReuseDelay) {
		}
	}
}