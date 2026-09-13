using System;
using ArknightsMod.Content.Items.Weapons.Medic.Closure;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Medic.Closure
{
	// 可露希尔跟随宠物「指挥中心」：手持可露希尔扫描枪时在玩家身后飞行跟随。
	//   贴图为竖排精灵图（8 帧，每帧 62×44），循环播放帧动画。
	//   暂不做任何具体功能，仅作为跟随视觉存在。
	public class CommandCenterPet : ModProjectile
	{
		private const int FrameCount = 8;
		private const int FrameTicks = 6; // 每帧持续的刻数

		public override string Texture => "ArknightsMod/Content/Projectiles/Medic/Closure/CommandCenterPet";

		public override void SetStaticDefaults() {
			Main.projFrames[Type] = FrameCount;
			Main.projPet[Type] = true;
			ProjectileID.Sets.MinionSacrificable[Type] = false;
			ProjectileID.Sets.CharacterPreviewAnimations[Type] = ProjectileID.Sets.SimpleLoop(0, FrameCount - 1, FrameTicks);
		}

		public override void SetDefaults() {
			Projectile.width = 62;
			Projectile.height = 44;
			Projectile.aiStyle = -1;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.netImportant = true;
			Projectile.timeLeft = 2; // 每帧由持械玩家续命；停止手持后自动消失
		}

		/// <summary>玩家手持可露希尔武器时，若尚无跟随宠物则生成一个（仅本地玩家）。</summary>
		public static void EnsureFor(Player player) {
			if (player.whoAmI != Main.myPlayer)
				return;
			int type = ModContent.ProjectileType<CommandCenterPet>();
			if (player.ownedProjectileCounts[type] > 0)
				return;
			Vector2 spawn = player.Center + new Vector2(-player.direction * 44f, -34f);
			Projectile.NewProjectile(player.GetSource_Misc("ClosureCommandCenter"), spawn, Vector2.Zero,
				type, 0, 0f, player.whoAmI);
		}

		public override bool? CanCutTiles() => false;
		public override bool MinionContactDamage() => false;

		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			bool valid = owner != null && owner.active && !owner.dead
				&& owner.HeldItem?.ModItem is ClosureScanGun;
			if (!valid) {
				// 不手持武器时自然消失（不强制 Kill，留出淡出余量）
				return;
			}

			Projectile.timeLeft = 2; // 续命，保持存在

			// 目标：玩家身后（面朝反方向）偏上一点，并加轻微上下浮动
			float bob = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 5f;
			Vector2 target = owner.Center + new Vector2(-owner.direction * 46f, -36f + bob);
			Vector2 toTarget = target - Projectile.Center;
			float dist = toTarget.Length();

			if (dist > 1600f) {
				Projectile.Center = target; // 距离过远直接瞬移跟上
				Projectile.velocity = Vector2.Zero;
			}
			else {
				// 平滑跟随：距离越远越快，接近时减速
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 0.14f, 0.2f);
			}

			// 朝向与玩家一致
			Projectile.spriteDirection = owner.direction;
			Projectile.rotation = Projectile.velocity.X * 0.02f; // 飞行时轻微侧倾

			// 帧动画循环
			if (++Projectile.frameCounter >= FrameTicks) {
				Projectile.frameCounter = 0;
				Projectile.frame = (Projectile.frame + 1) % FrameCount;
			}

			Lighting.AddLight(Projectile.Center, 0.15f, 0.25f, 0.35f);
		}
	}
}
