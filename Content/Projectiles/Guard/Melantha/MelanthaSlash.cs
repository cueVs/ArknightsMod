using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Melantha
{
	// 玫兰莎的剑挥砍弹幕。ai[0]==0 为向下挥砍(白剑)，ai[0]==1 为上挑挥砍(黑剑)。
	// 贴图是一把完整手持剑（剑尖朝右、剑柄在左），以剑柄为支点绕玩家旋转扫过弧线，而非沿路径拉伸的拖尾条带。
	public class MelanthaSlash : ModProjectile
	{
		public override string Texture => "ArknightsMod/Content/Items/Weapons/Guard/Melantha/MelanthasSword_white";

		// 剑身命中点离玩家中心的距离
		private const float Radius = 30f;
		// 单侧挥砍弧度（总挥砍 ≈ 2.7 rad）
		private const float HalfArc = 1.35f;

		// 挥砍中心朝向（指向鼠标，整段不变）
		private float aim;
		// 当前帧剑的指向
		private float currentAng;
		// true=向下挥砍
		private bool down;

		// 0→1 挥砍进度
		private ref float Progress => ref Projectile.localAI[0];
		private bool Black => Projectile.ai[0] == 1f;

		public override void SetDefaults() {
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			// 防止隔墙命中
			Projectile.ownerHitCheck = true;
			Projectile.usesLocalNPCImmunity = true;
			// 一次挥砍对同一敌人只判定一次
			Projectile.localNPCHitCooldown = 60;
			Projectile.timeLeft = 60;
			Projectile.scale = 1.2f;
			Projectile.alpha = 0;
		}

		public override void OnSpawn(IEntitySource source) {
			Player player = Main.player[Projectile.owner];
			aim = (Main.MouseWorld - player.Center).ToRotation();
			// 白=向下，黑=上挑
			down = !Black;
			currentAng = down ? aim - HalfArc : aim + HalfArc;
			SoundEngine.PlaySound(Black ? SoundID.Item19 : SoundID.Item1, player.Center);
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (player.dead || !player.active) {
				Projectile.Kill();
				return;
			}

			// 攻速修正：itemAnimationMax 越小，挥得越快（与近战攻速挂钩）
			float step = 0.075f * (40f / Math.Max(1, player.itemAnimationMax));
			Progress += step;
			float p = MathHelper.Clamp(Progress, 0f, 1f);

			// 向下：从 aim-HalfArc 扫到 aim+HalfArc；上挑：反向扫
			currentAng = down
				? aim - HalfArc + 2f * HalfArc * p
				: aim + HalfArc - 2f * HalfArc * p;

			Projectile.Center = player.Center + currentAng.ToRotationVector2() * Radius;

			// 手臂跟随剑的指向，动作连贯
			player.ChangeDir(Math.Cos(aim) >= 0 ? 1 : -1);
			player.heldProj = Projectile.whoAmI;
			player.itemTime = 2;
			player.itemAnimation = 2;
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full,
				currentAng - MathHelper.PiOver2);

			if (Progress >= 1f) {
				Projectile.alpha += 40;
				if (Projectile.alpha >= 255)
					Projectile.Kill();
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];
			Texture2D tex = ModContent.Request<Texture2D>(Black
				? "ArknightsMod/Content/Items/Weapons/Guard/Melantha/MelanthasSword_black"
				: "ArknightsMod/Content/Items/Weapons/Guard/Melantha/MelanthasSword_white").Value;

			// 朝左挥砍时垂直翻转贴图，使剑刃朝向正确（整段挥砍朝向一致，不会中途突变）
			bool faceLeft = Math.Cos(aim) < 0;
			SpriteEffects fx = faceLeft ? SpriteEffects.FlipVertically : SpriteEffects.None;

			// origin 取贴图左侧中点 = 剑柄，玩家手持此处
			Vector2 origin = new Vector2(0f, tex.Height / 2f);
			Vector2 drawPos = player.Center - Main.screenPosition;
			float fade = 1f - Projectile.alpha / 255f;

			Main.spriteBatch.Draw(tex, drawPos, null, Color.White * fade,
				currentAng, origin, Projectile.scale, fx, 0f);

			return false;
		}

		public override bool ShouldUpdatePosition() => false;
	}
}
