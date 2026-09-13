using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Astesia
{
	/// <summary>
	/// 星极（Astesia）套装效果玩家。
	/// 共同机制：附近有敌人时，每 20 秒累积 1 层（每层 5%，最多 5 层 = 25%）；
	///           附近无敌人持续 5 秒后全部层数失效清零。
	///   · 头盔效果（只戴头盔即生效）：近战攻击速度 +5% × 层数（最高 +25%）
	///   · 套装效果（三件齐全生效）：防御力 +5% × 层数（最高 +25%）
	/// 两个效果的触发条件与层数完全相同，因此共用同一套层数与计时。
	/// </summary>
	internal class AstesiaSetPlayer : ArknightsArmorPlayer
	{
		public bool AstesiaHelmetActive;
		public bool AstesiaSetActive;

		private const int StackIntervalTicks = 20 * 60; // 每 20 秒叠 1 层
		private const int LoseDelayTicks = 5 * 60;     // 无敌人 5 秒后失效
		private const int MaxStacks = 5;                // 每层 5% → 最高 25%
		private const float BonusPerStack = 0.05f;
		private const float EnemyScanRange = 900f;      // 与 Mizuki 等干员一致的敌人检测范围

		/// <summary>当前层数（0~5）。</summary>
		public int Stacks;

		private int enemyNearbyTimer;   // 有敌人时累加，满 20 秒叠一层
		private int noEnemyTimer;       // 无敌人时累加，满 5 秒清空层数

		public override void ResetEffects() {
			AstesiaHelmetActive = false;
			AstesiaSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 AstesiaHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			AstesiaHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<AstesiaHead>();
			AstesiaSetActive = AstesiaHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<AstesiaBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<AstesiaLegs>();

			if (Stacks <= 0)
				return;

			// 头盔效果：近战攻击速度 +5% / 层
			if (AstesiaHelmetActive)
				Player.GetAttackSpeed(DamageClass.Melee) += BonusPerStack * Stacks;

			// 套装效果：防御力 +5% / 层（在装备防御累加完成后乘算，最可靠）
			if (AstesiaSetActive)
				Player.statDefense += (int)(Player.statDefense * BonusPerStack * Stacks);
		}

		public override void PostUpdate() {
			// 一件都没穿（或已不再是套装件）→ 层数与计时全部清空
			if (!AstesiaHelmetActive && !AstesiaSetActive) {
				Stacks = 0;
				enemyNearbyTimer = 0;
				noEnemyTimer = 0;
				return;
			}

			if (HasEnemyNearby()) {
				noEnemyTimer = 0;
				enemyNearbyTimer++;
				if (enemyNearbyTimer >= StackIntervalTicks) {
					enemyNearbyTimer = 0;
					if (Stacks < MaxStacks)
						Stacks++;
				}
			}
			else {
				enemyNearbyTimer = 0;
				noEnemyTimer++;
				if (noEnemyTimer >= LoseDelayTicks) {
					noEnemyTimer = 0;
					Stacks = 0;
				}
			}
		}

		/// <summary>附近（900 像素内）是否存在可攻击的敌人。</summary>
		private bool HasEnemyNearby() {
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (!npc.active || npc.friendly || npc.lifeMax <= 5 || npc.dontTakeDamage)
					continue;

				if (npc.Distance(Player.Center) > EnemyScanRange)
					continue;

				return true;
			}

			return false;
		}
	}
}
