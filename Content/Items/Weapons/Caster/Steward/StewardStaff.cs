using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Players;
using ArknightsMod.Systems.Gameplay.Skill;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Caster.Steward
{
	/// <summary>
	/// 史都华德的法杖（迁移自 ArknightsWeaponryExpansion 的 SDHD）。
	/// <br/>1 技能·强力击：攻击充能，每蓄满一次后下一击射出强化追踪弹（优先高防目标）。
	/// </summary>
	public class StewardStaff : ExpansionWeaponBase
	{
		protected override int[] EliteDamage => [43, 54];

		public override void SetDefaults() {
			Item.damage = EliteDamage[0];
			Item.DamageType = DamageClass.Magic;
			Item.width = 54;
			Item.height = 54;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.knockBack = 2f;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.autoReuse = true;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<StewardProj>();
			Item.mana = 7;
			Item.crit = 4;
			Item.shootSpeed = 11f;
			Item.useTurn = false;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.channel = true;
			Item.staff[Item.type] = true;
		}

		public override bool CanUseItem(Player player) {
			if (Main.myPlayer == player.whoAmI) {
				var mp = player.GetModPlayer<WeaponPlayer>();
				// 攻击充能：每次攻击积攒技力，蓄满即得到 1 层库存
				if (mp.CurrentSkill?.ChargeType == SkillChargeType.Attack && !mp.SkillActive)
					mp.OffensiveRecovery();
			}
			return base.CanUseItem(player);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			var mp = player.GetModPlayer<WeaponPlayer>();

			if (mp.StockCount > 0) {
				// 强力击：消耗 1 层库存，射出强化追踪弹
				mp.DelStockCount();
				Projectile.NewProjectile(source, position + velocity, velocity * 0.9f, type,
					(int)(damage * 1.90f), knockback, player.whoAmI, ai0: 1f);
				SoundEngine.PlaySound(SoundID.Item20, position);
				SoundEngine.PlaySound(SoundID.Item28, position);
			}
			else {
				Projectile.NewProjectile(source, position + velocity, velocity, type,
					damage, knockback, player.whoAmI);
				SoundEngine.PlaySound(SoundID.Item28, position);
			}
			return false;
		}
	}

	/// <summary>史都华德的追踪法术弹，优先追踪附近防御力最高的敌人。</summary>
	public class StewardProj : ModProjectile
	{
		public override string Texture => "ArknightsMod/Content/Items/Weapons/Caster/Steward/StewardProj";

		public override void SetDefaults() {
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.scale = 1f;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 60;
			Projectile.penetrate = 1;
			Projectile.alpha = 255;
			Projectile.light = 0.5f;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 8;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 60;
		}

		public override void AI() {
			Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
				DustID.Clentaminator_Cyan, 0f, 0f, 0, new Color(255, 255, 255, 255), 1.5f);
			dust.noGravity = true;
			dust.velocity = Projectile.velocity / 2f;
			dust.position = Projectile.Center;
			Dust dust2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
				DustID.BlueFairy, 0f, 0f, 0, new Color(255, 255, 255, 255), 0.7f);
			dust2.noGravity = true;
			dust2.position = Projectile.Center;

			if (Projectile.ai[0] == 1f) {
				Dust dust3 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
					DustID.Clentaminator_Cyan, 0f, 0f, 0, new Color(255, 255, 255, 255), 1.5f);
				dust3.noGravity = true;
				dust3.position = Projectile.Center;
			}

			// 优先追踪 200 像素内防御力最高的敌人
			NPC target = null;
			float distanceMax = 200f;
			int highestDefense = 0;
			foreach (NPC npc in Main.npc) {
				if (npc.active && !npc.friendly) {
					float currentDistance = Vector2.Distance(npc.Center, Projectile.Center) + Vector2.Distance(npc.Size, Vector2.Zero) / 2f;
					if (currentDistance < distanceMax) {
						highestDefense = npc.defense;
						distanceMax = currentDistance;
						target = npc;
					}
					if (npc.defense > highestDefense && currentDistance < 200) {
						highestDefense = npc.defense;
						target = npc;
					}
				}
			}

			if (target != null) {
				Vector2 targetVec = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 18f;
				Projectile.velocity = (Projectile.velocity * 10f + targetVec) / 11f;
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			if (Projectile.ai[0] == 1f)
				Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null,
					new Color(200, 200, 255, 200), 0f, tex.Size() / 2f, 0.75f, SpriteEffects.None, 0);
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null,
				Color.AliceBlue, 0f, tex.Size() / 2f, 0.64f, SpriteEffects.None, 0);
			return false;
		}
	}
}
