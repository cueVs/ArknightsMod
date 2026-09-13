using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace ArknightsMod.Content.ElementalImpairment.Effect
{
	// ========== 抽象基类 ==========
	public abstract class BindingEffect
	{
		// 持续时间（可被子类重写）
		public virtual int Duration => 300;

		// 是否对Boss有效
		public virtual bool AffectBoss => false;

		// 减速比例
		public virtual float SlowAmount => 0.98f;

		// 检查是否对该NPC有效
		public static bool IsAffected(NPC npc, bool affectBoss) {
			if (npc == null || !npc.active)
				return false;
			if (npc.boss && !affectBoss)
				return false;
			return true;
		}

		// 应用效果
		public static void Apply(NPC npc, int duration, float slowAmount, bool freezeAI) {
			if (npc == null || !npc.active)
				return;

			var globalNPC = npc.GetGlobalNPC<BindingGlobalNPC>();
			if (duration > globalNPC.bindingTimer)
				globalNPC.bindingTimer = duration;
			globalNPC.slowAmount = slowAmount;
			globalNPC.freezeAI = freezeAI;
		}
	}

	// ========== 全局NPC处理 ==========
	public class BindingGlobalNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public int bindingTimer;
		public float slowAmount = 0.98f;
		public bool freezeAI = true;

		public override void ResetEffects(NPC npc) {
			if (bindingTimer > 0)
				bindingTimer--;
		}

		public override bool PreAI(NPC npc) {
			if (bindingTimer > 0 && freezeAI) {
				npc.velocity = Vector2.Zero;
				return false;
			}
			return base.PreAI(npc);
		}

		public override void PostAI(NPC npc) {
			if (bindingTimer > 0 && slowAmount > 0 && !freezeAI) {
				npc.velocity *= (1f - slowAmount);
			}
		}

		public override void DrawEffects(NPC npc, ref Color drawColor) {
			if (bindingTimer > 0) {
				drawColor = Color.Lerp(drawColor, new Color(100, 50, 150), 0.3f);
			}
		}

		public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers) {
			if (bindingTimer > 0)
				modifiers.FinalDamage *= 1.1f;
		}

		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) {
			if (bindingTimer > 0)
				modifiers.FinalDamage *= 1.1f;
		}
	}
}