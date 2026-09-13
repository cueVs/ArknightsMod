using System;
using ArknightsMod.Content.Buffs.Supporter.Pramanix;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Supporter.Pramanix
{
	public static class FrostDomainLogic
	{
		public const int TierMax = 3;

		public static readonly int[] HitBudgetMax = [0, 15, 30, 50];
		public static readonly float[] RadiusByTier = [0f, 280f, 380f, 480f];
		public static readonly float[] DamageMultByTier = [0f, 0.15f, 0.25f, 0.40f];
		public static readonly int[] TickIntervalByTier = [0, 90, 75, 60];
		public const int MinAttackInterval = 60;

		public static readonly int[] PassiveTierInterval = [0, 900, 1200, 1500];
		public const float Skill3PassiveGrowthMult = 0.55f;
		public const float Skill3DomainTickMult = 0.72f;

		public static int GetMagicDamage(Player player) {
			Item item = player.HeldItem;
			if (item.ModItem is not SaintBell)
				return 0;
			return (int)player.GetDamage(DamageClass.Magic).ApplyTo(item.damage);
		}

		public static float GetRadius(int tier, bool skill3Active) {
			if (tier <= 0)
				return 0f;
			float radius = RadiusByTier[tier];
			if (skill3Active)
				radius *= 1.5f;
			return radius;
		}

		public static void SetTier(SaintBellPlayer state, int tier) {
			int oldTier = state.FrostTier;
			state.FrostTier = (int)MathHelper.Clamp(tier, 0, TierMax);
			state.FrostHitBudget = HitBudgetMax[state.FrostTier];
			if (state.FrostTier == 0)
				state.PassiveGrowthTimer = 0;
			else if (state.FrostTier > oldTier)
				state.PassiveGrowthTimer = 0;
		}

		public static void ConsumeHit(SaintBellPlayer state, int amount = 1) {
			if (state.FrostTier <= 0 || amount <= 0)
				return;

			int remaining = amount;
			while (remaining > 0 && state.FrostTier > 0) {
				state.FrostHitBudget -= remaining;
				remaining = 0;
				if (state.FrostHitBudget > 0)
					break;

				remaining = -state.FrostHitBudget;
				SetTier(state, state.FrostTier - 1);
			}
		}

		public static void TickPassiveGrowth(Player player, SaintBellPlayer state) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			state.PassiveGrowthTimer++;
			int targetTier = state.FrostTier + 1;
			if (targetTier > TierMax)
				return;

			int interval = PassiveTierInterval[targetTier];
			if (state.Skill3Active)
				interval = Math.Max(1, (int)(interval * Skill3PassiveGrowthMult));
			if (state.PassiveGrowthTimer < interval)
				return;

			state.PassiveGrowthTimer = 0;
			SetTier(state, targetTier);
		}

		public static void TickDomain(Player player, SaintBellPlayer state) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;
			if (state.FrostTier <= 0)
				return;

			state.FrostDomainTimer++;
			int interval = TickIntervalByTier[state.FrostTier];
			if (state.Skill3Active)
				interval = Math.Max(1, (int)(interval * Skill3DomainTickMult));

			if (state.FrostDomainTimer < interval)
				return;
			state.FrostDomainTimer = 0;

			float radius = GetRadius(state.FrostTier, state.Skill3Active);
			int baseDamage = GetMagicDamage(player);
			if (baseDamage <= 0)
				return;

			int damage = Math.Max(1, (int)(baseDamage * DamageMultByTier[state.FrostTier]));

			int hits = 0;
			foreach (NPC npc in Main.ActiveNPCs) {
				if (!npc.CanBeChasedBy(player) || npc.Distance(player.Center) > radius)
					continue;

				if (state.Skill3Active && player.GetModPlayer<SaintBellPlayer>() is { } sb)
					sb.StrikeSkill3Magic(npc, damage);
				else
					StrikeMagic(player, npc, damage);
				npc.AddBuff(ModContent.BuffType<PramanixSlowDebuff>(), 30);
				PramanixHitVfx.SpawnSlowSmoke(npc);
				PramanixColdAttachment.ApplyDomain(npc, player);
				if (player.whoAmI == Main.myPlayer)
					PramanixSoundHelper.PlayDomainHit(npc.Center);
				hits++;
			}

			if (hits > 0)
				ConsumeHit(state, hits);
		}

		public static void StrikeMagic(Player player, NPC npc, int damage) {
			NPC.HitInfo hit = new() {
				Damage = damage,
				DamageType = DamageClass.Magic,
				HitDirection = player.Center.X <= npc.Center.X ? 1 : -1,
				Knockback = 0f,
			};
			npc.StrikeNPC(hit);
		}
	}
}
