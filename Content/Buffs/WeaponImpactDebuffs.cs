using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs
{
	// 原版 Bleeding/Electrified 不提供 NPC 扣血逻辑；独立状态只服务于指定武器，
	// 不修改原版状态或其他干员的命中规则。AddBuff 使用原有 NPC 状态同步，重复命中仅刷新时间。
	public sealed class WeaponBleedingDebuff : ModBuff
	{
		public override string Texture => "Terraria/Images/Buff_" + BuffID.Bleeding;

		public override void SetStaticDefaults()
		{
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
		}
	}

	public sealed class WeaponImpactDebuffNPC : GlobalNPC
	{
		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			int loss = 0;
			if (npc.HasBuff(ModContent.BuffType<WeaponBleedingDebuff>()))
				loss += 16; // lifeRegen 每两点对应每秒一点伤害，即流血 8 HP/s。
			if (loss == 0)
				return;
			npc.lifeRegen = Math.Min(0, npc.lifeRegen) - loss;
			damage = Math.Max(damage, 2);
		}

		public override void DrawEffects(NPC npc, ref Color drawColor)
		{
			if (Main.dedServ)
				return;
			if (npc.HasBuff(ModContent.BuffType<WeaponBleedingDebuff>()) && Main.rand.NextBool(12))
				Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood, 0f, 0.5f, Scale: 0.7f);
		}
	}
}
