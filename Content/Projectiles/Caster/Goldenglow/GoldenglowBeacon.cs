using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Goldenglow
{
	// 澄闪浮游信标：漂浮在空中，当玩家攻击时同步向光标发射魔法弹
	public class GoldenglowBeacon : ModProjectile
	{
		public override string Texture => "ArknightsMod/Content/Items/Weapons/Caster/Goldenglow/GoldenglowBeacon";

		public const int BaseMaxBeacons = 3;

		// 当前玩家的浮游单元数量上限：基础 4 个，技能 1/2/3 激活时分别 +1/+1/+2
		public static int GetMaxBeacons(Player player) {
			var mp = player.GetModPlayer<WeaponPlayer>();
			int bonus = 0;
			if (mp.SkillActive) {
				bonus = mp.Skill switch {
					0 => 1,
					1 => 1,
					2 => 2,
					_ => 0
				};
			}
			return BaseMaxBeacons + bonus;
		}

		// ai[0] = 发射冷却计时器
		// ai[1] = 生成时的 Y 坐标（浮动参考基点）
		// localAI[0] = 生成时刻（用于在数量超限时找到最早召唤的浮游单元）
		private ref float FireCooldown => ref Projectile.ai[0];
		private ref float BaseY => ref Projectile.ai[1];
		public ref float SpawnTick => ref Projectile.localAI[0];

		public override void SetDefaults() {
			Projectile.width = 14;
			Projectile.height = 24;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = int.MaxValue;
			Projectile.minionSlots = 0f;
			Projectile.light = 0.3f;
		}

		public override void OnSpawn(IEntitySource source) {
			BaseY = Projectile.Center.Y;
			SpawnTick = Main.GameUpdateCount;
		}

		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			if (!owner.active || owner.dead) { Projectile.Kill(); return; }

			// 如果玩家不再持有法杖，信标消失
			if (!(owner.HeldItem.ModItem is Items.Weapons.Caster.Goldenglow.GoldenglowWand)) {
				// 保持存在（只要换回法杖就能继续使用）
			}

			// 轻微上下浮动
			float bob = (float)Math.Sin(Main.GameUpdateCount * 0.05f + Projectile.whoAmI * 1.3f) * 4f;
			Projectile.Center = new Vector2(Projectile.Center.X, BaseY + bob);

			// 金色光效粒子
			if (Main.rand.NextBool(8)) {
				Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 10f),
					DustID.GoldFlame, Vector2.Zero, 150, default, 0.7f);
				d.noGravity = true;
			}

			// 发射冷却计时
			if (FireCooldown > 0) { FireCooldown--; return; }

			// 当玩家正在攻击法杖时，同步向光标发射弹幕
			bool playerAttacking = owner.itemAnimation > 0
				&& owner.HeldItem.ModItem is Items.Weapons.Caster.Goldenglow.GoldenglowWand;

			// 每个浮游单元独立按冷却发射，冷却时长等于攻击间隔，保证每次攻击最多发射一枚，不计入玩家左键弹幕的堆叠上限
			if (playerAttacking && Projectile.owner == Main.myPlayer) {
				Vector2 dir = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX);
				// 浮游单元弹幕伤害为武器面板伤害(GetWeaponDamage，含加成后的展示伤害)的 40%
				int boltDamage = (int)(owner.GetWeaponDamage(owner.HeldItem) * 0.4f);
				int boltIndex = Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					Projectile.Center,
					dir * owner.HeldItem.shootSpeed,
					ProjectileID.MagicMissile,
					boltDamage,
					owner.HeldItem.knockBack,
					owner.whoAmI);
				if (boltIndex >= 0 && boltIndex < Main.maxProjectiles)
					Main.projectile[boltIndex].DamageType = DamageClass.Magic;

				FireCooldown = Math.Max(1, owner.HeldItem.useTime);
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Vector2 origin = tex.Size() / 2f;
			Vector2 pos = Projectile.Center - Main.screenPosition;

			// 轻微发光叠层
			Color glowCol = new Color(255, 220, 80, 80);
			Main.spriteBatch.Draw(tex, pos, null, glowCol, 0f, origin, Projectile.scale * 1.3f, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(tex, pos, null, lightColor, 0f, origin, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}

		public override bool ShouldUpdatePosition() => false;
	}
}
