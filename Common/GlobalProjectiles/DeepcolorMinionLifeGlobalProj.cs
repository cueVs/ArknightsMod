using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.Supporter.Deepcolor;
using ArknightsMod.Content.Items.Weapons.Supporter.Deepcolor;
using ArknightsMod.Content.Projectiles.Supporter.Deepcolor;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;

namespace ArknightsMod.Common.GlobalProjectiles
{
	// 为触手添加生命值和防御
	public class DeepcolorMinionLifeGlobalProj : GlobalProjectile
	{
		public const int DefaultLifeMax = 191;
		public const int DefaultDefense = 10;
		// 触手受伤间隔
		public const int ReceiveDamageCooldownMax = 45;
		// 触手对敌人碰撞伤害间隔
		public const int DealContactDamageCooldownMax = 30;

		public override bool InstancePerEntity => true;

		public bool useLife;
		public bool drawHealthBar = true;
		public int life = -1;
		public int lifeMax = DefaultLifeMax;
		public int defense = DefaultDefense;
		public float damageReduction = 1f;
		public int hitCooldown = ReceiveDamageCooldownMax;
		// 召唤顺序，用于满员时替换最早的一只触手
		public int spawnOrder;

		private static readonly Dictionary<int, int> NextSpawnOrderByPlayer = new();

		private int receiveCooldown;
		private int dealContactCooldown;
		private int regenTimer;
		private int setRegenTimer;

		public override void PostAI(Projectile projectile) {
			if (!useLife || projectile.type != ModContent.ProjectileType<DeepcolorMinion>())
				return;

			if (projectile.Relocate().IsTransitioning)
				return;

			if (life < 0)
				life = lifeMax;

			Player owner = Main.player[projectile.owner];
			defense = DeepcolorSketchSkills.ShadowTentacleActive(owner)
				? (int)(DefaultDefense * DeepcolorSketchSkills.ShadowTentacleDefenseMult)
				: DefaultDefense;

			if (DeepcolorSketchSkills.ShadowTentacleActive(owner) && ++regenTimer >= 60) {
				regenTimer = 0;
				life = Math.Min(life + DeepcolorSketchSkills.ShadowTentacleRegenPerSecond, lifeMax);
			}

			// 深海色套装效果：所有深海色的召唤物每秒恢复25点生命值。
			// NeoArmor Reforge：深海色迁到新系统后，穿在盔甲栏里的是独立 ItemID 的套装件，
			// 比对时装类型 + hasUpgraded 会恒为 false（静默失效）。判定统一读
			// DeepcolorSetPlayer 的标记，它在 PostUpdateEquips 里用 GetSetType 算好了。
			if (owner.GetModPlayer<DeepcolorSetPlayer>().DeepcolorSetActive
				&& ++setRegenTimer >= 60) {
				setRegenTimer = 0;
				life = Math.Min(life + 25, lifeMax);
			}

			if (receiveCooldown > 0)
				receiveCooldown--;
			if (dealContactCooldown > 0)
				dealContactCooldown--;

			foreach (NPC npc in Main.ActiveNPCs) {
				if (!npc.active || npc.friendly || npc.dontTakeDamage)
					continue;
				if (!npc.CanBeChasedBy(owner))
					continue;
				if (!projectile.Hitbox.Intersects(npc.Hitbox) || receiveCooldown > 0)
					continue;

				TakeDamage(projectile, npc.damage);
				receiveCooldown = hitCooldown;
				break;
			}

			foreach (Projectile other in Main.ActiveProjectiles) {
				if (other.whoAmI == projectile.whoAmI || !other.active)
					continue;
				// 仅敌对弹幕可伤触手；排除主人自己的特效/辅助弹幕（常设 friendly=false）
				if (!other.hostile || other.owner == projectile.owner)
					continue;
				if (!projectile.Hitbox.Intersects(other.Hitbox) || receiveCooldown > 0)
					continue;

				TakeDamage(projectile, other.damage);
				receiveCooldown = hitCooldown;
				break;
			}

			if (life <= 0) {
				DeepcolorMinion.SpawnDeathParticles(projectile);
				projectile.Kill();
			}
		}

		public bool CanDealContactDamage() => dealContactCooldown <= 0;

		public void StartDealContactCooldown() {
			dealContactCooldown = DealContactDamageCooldownMax;
		}

		public void TakeDamage(Projectile projectile, int damage) {
			// 深海色头盔效果：召唤物获得7%闪避（判定改读 SetPlayer 标记，原因同上）
			Player owner = Main.player[projectile.owner];
			if (owner.GetModPlayer<DeepcolorSetPlayer>().DeepcolorHelmetActive
				&& Main.rand.NextFloat() < 0.07f) {
				CombatText.NewText(projectile.getRect(), Color.Yellow, "Miss");
				return;
			}

			damage = (int)((damage - defense) * damageReduction);
			if (damage < 1)
				damage = 1;

			life -= damage;
			CombatText.NewText(projectile.getRect(), Color.Red, damage);
			SoundEngine.PlaySound(SoundID.NPCHit1, projectile.Center);

			if (life <= 0)
				SoundEngine.PlaySound(SoundID.NPCDeath4, projectile.Center);
		}

		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			if (projectile.type != ModContent.ProjectileType<DeepcolorMinion>())
				return;

			useLife = true;
			lifeMax = DefaultLifeMax;
			life = lifeMax;
			defense = DefaultDefense;
			drawHealthBar = true;

			if (!NextSpawnOrderByPlayer.TryGetValue(projectile.owner, out int nextOrder))
				nextOrder = 0;
			spawnOrder = nextOrder;
			NextSpawnOrderByPlayer[projectile.owner] = nextOrder + 1;
		}

		public override void PostDraw(Projectile projectile, Color lightColor) {
			if (!useLife || !drawHealthBar || projectile.type != ModContent.ProjectileType<DeepcolorMinion>())
				return;

			if (projectile.Relocate().IsTransitioning)
				return;

			if (Main.netMode == NetmodeID.Server)
				return;

			DrawHealthBar(projectile);
			ShowHealthOnHover(projectile);
		}

		private void DrawHealthBar(Projectile projectile) {
			// 原版的血条样式
			float footY = projectile.Bottom.Y + DeepcolorMinion.GroundOverlapPixels;
			float barY = footY + 10f + projectile.gfxOffY;
			float alpha = Lighting.Brightness(
				(int)(projectile.Center.X / 16f),
				(int)((projectile.Center.Y + projectile.gfxOffY) / 16f));

			Main.instance.DrawHealthBar(projectile.Center.X, barY, life, lifeMax, alpha, 1f);
		}

		private void ShowHealthOnHover(Projectile projectile) {
			Vector2 mouse = Main.MouseWorld;
			if (!new Rectangle((int)mouse.X - 10, (int)mouse.Y - 10, 20, 20).Intersects(projectile.getRect()))
				return;

			float textWorldY = projectile.Bottom.Y + DeepcolorMinion.GroundOverlapPixels + 26f + projectile.gfxOffY;
			Vector2 textPos = new Vector2(projectile.Center.X, textWorldY) - Main.screenPosition;
			string text = $"{life}/{lifeMax}";
			Vector2 origin = FontAssets.MouseText.Value.MeasureString(text) * 0.5f;
			ChatManager.DrawColorCodedStringWithShadow(
				Main.spriteBatch,
				FontAssets.MouseText.Value,
				text,
				textPos,
				Color.White,
				0f,
				origin,
				Vector2.One * 0.85f);
		}

		public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter) {
			binaryWriter.Write(useLife);
			if (!useLife)
				return;
			binaryWriter.Write(drawHealthBar);
			binaryWriter.Write(lifeMax);
			binaryWriter.Write(life);
			binaryWriter.Write(defense);
			binaryWriter.Write(damageReduction);
			binaryWriter.Write(hitCooldown);
			binaryWriter.Write(receiveCooldown);
			binaryWriter.Write(dealContactCooldown);
			binaryWriter.Write(spawnOrder);
		}

		public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader) {
			useLife = binaryReader.ReadBoolean();
			if (!useLife)
				return;
			drawHealthBar = binaryReader.ReadBoolean();
			lifeMax = binaryReader.ReadInt32();
			life = binaryReader.ReadInt32();
			defense = binaryReader.ReadInt32();
			damageReduction = binaryReader.ReadSingle();
			hitCooldown = binaryReader.ReadInt32();
			receiveCooldown = binaryReader.ReadInt32();
			dealContactCooldown = binaryReader.ReadInt32();
			spawnOrder = binaryReader.ReadInt32();
		}
	}

	public static class DeepcolorMinionLifeExtensions
	{
		public static DeepcolorMinionLifeGlobalProj MinionLife(this Projectile projectile)
			=> projectile.GetGlobalProjectile<DeepcolorMinionLifeGlobalProj>();

		public static bool CanDealContactDamage(this Projectile projectile) {
			var life = projectile.MinionLife();
			return life.useLife && life.CanDealContactDamage();
		}

		public static void SetDealContactCooldown(this Projectile projectile) {
			projectile.MinionLife().StartDealContactCooldown();
		}
	}
}
