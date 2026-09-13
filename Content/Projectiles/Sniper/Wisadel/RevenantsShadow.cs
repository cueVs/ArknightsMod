using ArknightsMod.Content.Items.Armor.Sniper.Wisadel;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Sniper.Wisadel
{
	public class RevenantsShadow : ModProjectile
	{
		public override void SetDefaults() {
			Projectile.width = 36;
			Projectile.height = 32;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 2;
			Projectile.ignoreWater = true;
		}

		public override bool? CanDamage() => false;

		public const int MaxBlockLife = 5;
		private const float OrbitRadius = 80f;
		private const int RefreshInterval = 60 * 60; // 60 秒重置

		/// <summary>剩余格挡次数</summary>
		public int BlockLife {
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		/// <summary>槽位（0/1/2）</summary>
		private int Slot => (int)Projectile.ai[2];

		/// <summary>独立刷新计时器（帧），存在 localAI 中</summary>
		private int RefreshTimer {
			get => (int)Projectile.localAI[0];
			set => Projectile.localAI[0] = value;
		}

		private Player Owner => Main.player[Projectile.owner];

		public override void AI() {
			Player player = Owner;
			var setPlayer = player.GetModPlayer<WisadelSetPlayer>();
			if (player.dead || !setPlayer.WisadelHelmetActive) {
				Projectile.Kill();
				return;
			}

			Projectile.timeLeft = 2;

			// 独立刷新：每60秒重置格挡次数
			RefreshTimer++;
			if (RefreshTimer >= RefreshInterval) {
				RefreshTimer = 0;
				BlockLife = MaxBlockLife;
				if (Main.netMode != NetmodeID.Server) {
					for (int i = 0; i < 8; i++) {
						Dust.NewDust(Projectile.Center, 0, 0, DustID.Granite, 0f, 0f, 100, default, 1.5f);
					}
				}
			}

			// 从 SetPlayer 读取公共旋转角 + 槽位偏移，确保三等分点始终均匀
			float angle = setPlayer.globalOrbitAngle + Slot * MathHelper.TwoPi / 3;
			Projectile.Center = player.MountedCenter + new Vector2(OrbitRadius, 0).RotatedBy(angle);


			// 格挡
			for (int i = 0; i < Main.maxProjectiles; ++i) {
				if (Main.projectile[i].active && i != Projectile.whoAmI) {
					if (Main.projectile[i].active && i != Projectile.whoAmI
						&& Main.projectile[i].Hitbox.Intersects(Projectile.Hitbox)
						&& !Main.projectile[i].minion
						&& Main.projectile[i].minionSlots <= 0
						&& Main.projectile[i].hostile) {
						Projectile hostile = Main.projectile[i];

						hostile.Kill();
						BlockLife--;

						if (Main.netMode != NetmodeID.Server) {
							for (int j = 0; j < 4; j++) {
								Dust.NewDust(Projectile.Center, 0, 0, DustID.Granite, 0f, 0f, 100, default, 1f);
							}
						}

						if (BlockLife <= 0) {
							Projectile.Kill();
							return;
						}
					}
				}
			}
		}

		public override void PostDraw(Color lightColor)
		{
			if (Main.netMode == NetmodeID.Server || BlockLife == MaxBlockLife)
				return;

			float footY = Projectile.Bottom.Y;
			float barY = footY + 10f + Projectile.gfxOffY;
			float alpha = Lighting.Brightness(
				(int)(Projectile.Center.X / 16f),
				(int)((Projectile.Center.Y + Projectile.gfxOffY) / 16f));

			Main.instance.DrawHealthBar(Projectile.Center.X, barY, BlockLife, MaxBlockLife, alpha, 1f);
		}
	}
}
