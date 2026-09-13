using ArknightsMod.Content.Projectiles.Sniper.Typhon;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs
{
	public class TyphonSkillArrowStunBuff : ModBuff
	{
		public override string Texture => "ArknightsMod/Content/Buffs/StunDebuff";

		public override void SetStaticDefaults()
		{
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
		}
	}

	public class TyphonSkillArrowStunGlobalNPC : GlobalNPC
	{
		public override bool PreAI(NPC npc)
		{
			if (npc.HasBuff<TyphonSkillArrowStunBuff>())
			{
				npc.velocity = Vector2.Zero;
				return false;
			}

			return true;
		}
	}

	public static class TyphonSkillArrowStun
	{
		public const int TicksPerHit = 15;

		public const int MaxDurationTicks = 60 * 5;

		private const float AiS3RainHoming = 3f;
		private const float AiS3RainColumn = 3.25f;

		public static void TryApply(NPC target, Player owner, Projectile proj)
		{
			if (!target.active || !owner.active)
				return;
			if (!owner.GetModPlayer<WeaponPlayer>().SkillActive)
				return;
			if (!IsTyphonSkillDamageProjectile(proj))
				return;
			if (!CanStunNpc(target, owner))
				return;
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			int buffType = ModContent.BuffType<TyphonSkillArrowStunBuff>();
			int idx = target.FindBuffIndex(buffType);
			if (idx >= 0)
				target.buffTime[idx] = System.Math.Min(target.buffTime[idx] + TicksPerHit, MaxDurationTicks);
			else
				target.AddBuff(buffType, TicksPerHit);

			target.netUpdate = true;
		}

		private static bool IsTyphonSkillDamageProjectile(Projectile proj)
		{
			if (proj.ModProjectile is TyphonS2Arrow)
				return true;

			if (proj.ModProjectile is not TyphonArrow)
				return false;

			float m = proj.ai[2];
			return m == 2f
				|| m == 4f
				|| m == AiS3RainHoming
				|| m == AiS3RainColumn;
		}

		private static bool CanStunNpc(NPC npc, Player owner)
		{
			return !npc.friendly
				&& !npc.boss
				&& !npc.dontTakeDamage
				&& npc.life > 0
				&& npc.CanBeChasedBy(owner);
		}
	}
}
