using ArknightsMod.Content.Buffs;
using ArknightsMod.Content.Buffs.ArmorSets;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Systems.Gameplay.Damage;
using ArknightsMod.Systems.Gameplay.OperatorTags;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Sniper.Rosmontis
{
	public class RosmontisTacticalEquipment : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_191";

		public override void SetStaticDefaults() {
		}

		public override void SetDefaults() {
			Projectile.width = 48;
			Projectile.height = 64;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.tileCollide = true;
			Projectile.sentry = true;
			Projectile.timeLeft = 20 * 60;
			Projectile.penetrate = -1;
			Projectile.netImportant = true;
		}

		public override void AI() {
			Projectile.velocity = Vector2.Zero;

			if (Projectile.localAI[0] == 0f) {
				Projectile.localAI[0] = 1f;
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					if (!npc.active || !npc.CanBeChasedBy() || npc.friendly)
						continue;

					if (Vector2.Distance(npc.Center, Projectile.Center) <= 160f)
						OperatorStunNPC.TryApply(npc, 6 * 60);
				}
			}

			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (!npc.active || !npc.CanBeChasedBy() || npc.friendly)
					continue;

				if (Vector2.Distance(npc.Center, Projectile.Center) > 180f)
					continue;

				npc.defense = Math.Max(0, npc.defense - 40);
				DamageCategoryNPC cat = npc.GetGlobalNPC<DamageCategoryNPC>();
				// 法抗是 0~1 的比例（对照本项目其它 ~20 处 artsResistance 赋值），这里原来写的是 -20f，
				// 量级錯了两个数量级——法伤系统之前从未真正生效，这个 bug 一直没暴露出来。
				cat.artsResistance = Math.Max(0f, cat.artsResistance - 0.20f);
			}

			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile other = Main.projectile[i];
				if (!other.active || other.friendly || other.owner == Projectile.owner)
					continue;

				if (other.hostile && other.Hitbox.Intersects(Projectile.Hitbox))
					other.Kill();
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.velocity = Vector2.Zero;
			return false;
		}
	}
}
