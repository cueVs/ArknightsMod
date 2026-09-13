using ArknightsMod.Common.GlobalNPCs;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Supporter.Radian
{
	internal class RaidianSetPlayer : ArknightsArmorPlayer
	{
		/// <summary>套装效果给被标记敌人追加的伤害占召唤物攻击力的比例。</summary>
		public const float MarkedBonusDamageRatio = 0.08f;

		/// <summary>头盔效果「鼓舞」给召唤物加的生命上限占主人生命上限的比例。</summary>
		public const float InspireLifeRatio = 0.2f;

		/// <summary>召唤物"优先攻击被标记敌人"的搜索半径（像素）。</summary>
		private const float MarkedSearchRangePx = 1200f;

		public bool RaidianHelmetActive;
		public bool RaidianSetActive;

		public int MinionBonusHealth;

		public override void ResetEffects() {
			RaidianHelmetActive = false;
			RaidianSetActive = false;
			MinionBonusHealth = 0;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 RaidianHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			RaidianHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<RaidianHead>();
			RaidianSetActive = RaidianHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<RaidianBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<RaidianLegs>();

			int slotBonus = 0;
			if (RaidianHelmetActive)
				slotBonus++;
			if (RaidianSetActive)
				slotBonus++;

			Player.maxMinions += slotBonus;

			// 「鼓舞」的数值只在这里算，真正加到召唤物身上由 RaidianMinionGlobalProj 负责
			// （原版召唤物没有生命概念，只有本模组自己建模了生命的召唤物才吃得到）。
			if (RaidianHelmetActive)
				MinionBonusHealth = (int)(Player.statLifeMax2 * InspireLifeRatio);
		}

		public override void PostUpdate() {
			if (RaidianSetActive)
				RetargetMinionsToMarked();
		}

		// 套装效果的「召唤物优先攻击被标记的敌人」。
		//
		// 直接改每个召唤物的 AI 目标是不现实的（每种召唤物选敌逻辑都不一样，还有别的
		// 模组的），所以走原版统一的"指定目标"通道 Player.MinionAttackTargetNPC ——
		// 这是玩家右键点敌人手动指定目标时用的同一个字段，原版和绝大多数模组的召唤物
		// AI 都会优先认它。
		//
		// ⚠ 只在玩家自己没有手动指定目标时才接管，否则会把玩家的手动指定顶掉。
		private void RetargetMinionsToMarked() {
			if (Player.HasMinionAttackTargetNPC)
				return;

			NPC best = null;
			float bestDistSq = MarkedSearchRangePx * MarkedSearchRangePx;

			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc.friendly || npc.dontTakeDamage || npc.lifeMax <= 5)
					continue;
				if (!npc.GetGlobalNPC<RaidianMarkGlobalNPC>().RaidianMarked)
					continue;
				if (!npc.CanBeChasedBy(Player))
					continue;

				float distSq = Vector2.DistanceSquared(Player.Center, npc.Center);
				if (distSq >= bestDistSq)
					continue;

				best = npc;
				bestDistSq = distSq;
			}

			if (best != null)
				Player.MinionAttackTargetNPC = best.whoAmI;
		}

		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
			TryMark(item, target, damageDone);
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			TryMark(proj, target, damageDone);
		}

		private void TryMark(Item item, NPC target, int damageDone) {
			if (!RaidianSetActive || damageDone <= 0 || !item.DamageType.CountsAsClass(DamageClass.Summon))
				return;

			MarkTarget(target);
		}

		private void TryMark(Projectile proj, NPC target, int damageDone) {
			if (!RaidianSetActive || damageDone <= 0 || !proj.DamageType.CountsAsClass(DamageClass.Summon))
				return;

			MarkTarget(target);
		}

		private static void MarkTarget(NPC target) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			if (target.friendly || target.lifeMax <= 5)
				return;

			target.GetGlobalNPC<RaidianMarkGlobalNPC>().RaidianMarked = true;
		}
	}
}
