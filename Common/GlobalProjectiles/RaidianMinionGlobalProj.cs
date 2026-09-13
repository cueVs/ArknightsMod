using ArknightsMod.Common.GlobalNPCs;
using ArknightsMod.Content.Items.Armor.Supporter.Radian;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Common.GlobalProjectiles
{
	// 电弧(Radian) 套装效果里"作用在召唤物身上"的那两条。
	//
	// 迁移补记：这两条效果的文案在 ArmorSets.hjson 里一直写着，RaidianSetPlayer 也一直
	// 在写 MinionBonusHealth / RaidianMarked 两个字段，但**从来没有任何地方读取它们**，
	// 也就是说除了"召唤栏+1"之外，电弧的头盔/套装效果实际上从未生效过。这个类就是
	// 把那两个字段真正接上去。
	public class RaidianMinionGlobalProj : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		// 「鼓舞」只加一次，加过之后记住加了多少，卸下套装时好原样扣回去。
		private int appliedBonusLife;

		public override void PostAI(Projectile projectile) {
			if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
				return;

			Player owner = Main.player[projectile.owner];
			if (!owner.active)
				return;

			ApplyInspireBonus(projectile, owner);
		}

		// ── 头盔效果：召唤物获得相当于（主人）自身生命上限 20% 的鼓舞 ──────────
		//
		// 本模组里只有"带生命值建模"的召唤物（目前是深海色的触手，见
		// DeepcolorMinionLifeGlobalProj）才有生命上限可加，普通召唤物在原版里根本没有
		// 生命这个概念，所以这条只对前者生效。
		private void ApplyInspireBonus(Projectile projectile, Player owner) {
			DeepcolorMinionLifeGlobalProj life = projectile.MinionLife();
			if (!life.useLife)
				return;

			int target = owner.GetModPlayer<RaidianSetPlayer>().MinionBonusHealth;

			if (target == appliedBonusLife)
				return;

			// 差量更新：生命上限跟着主人当前的加成值走，同时把当前生命也补上同样的差，
			// 这样"穿上套装"不会出现血条瞬间变空的观感，"脱下套装"也不会残留超额血量。
			int delta = target - appliedBonusLife;
			life.lifeMax += delta;
			if (life.life >= 0)
				life.life = System.Math.Clamp(life.life + delta, 1, life.lifeMax);

			appliedBonusLife = target;
		}

		// ── 套装效果：召唤伤害打出的标记，让召唤物额外附带 8% 攻击力的法术伤害 ──
		//
		// "优先攻击被标记的敌人"在 RaidianSetPlayer 里通过原版的
		// Player.MinionAttackTargetNPC 实现（那是原版召唤物统一认的"指定目标"字段），
		// 这里只负责追加伤害那一半。
		public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) {
			if (!projectile.DamageType.CountsAsClass(DamageClass.Summon))
				return;
			if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
				return;

			Player owner = Main.player[projectile.owner];
			if (!owner.active || !owner.GetModPlayer<RaidianSetPlayer>().RaidianSetActive)
				return;

			if (!target.GetGlobalNPC<RaidianMarkGlobalNPC>().RaidianMarked)
				return;

			// 额外 8% 攻击力的法术伤害。做成平砍加值而不是独立弹幕：独立弹幕会多触发一次
			// 命中判定，和吸血/护甲穿透之类的效果叠起来容易出问题。
			modifiers.FlatBonusDamage += projectile.damage * RaidianSetPlayer.MarkedBonusDamageRatio;
		}
	}
}
