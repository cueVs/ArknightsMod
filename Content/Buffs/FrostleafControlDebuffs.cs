using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs
{
	public sealed class FrostleafSlowDebuff : ModBuff
	{
		public override string Texture => $"Terraria/Images/Buff_{BuffID.Slow}";

		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
		}
	}

	public sealed class FrostleafBindDebuff : ModBuff
	{
		public override string Texture => $"Terraria/Images/Buff_{BuffID.Frozen}";

		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
		}
	}

	/// <summary>
	/// 原版 Slow/Frozen 主要是玩家控制效果，NPC 仅持有那些 Buff 并不会自动减速。
	/// 这里把霜叶技能的 NPC 控场语义落实为实际速度约束。
	/// </summary>
	public sealed class FrostleafControlGlobalNPC : GlobalNPC
	{
		public override bool PreAI(NPC npc) {
			if (!npc.boss && npc.HasBuff<FrostleafBindDebuff>()) {
				npc.velocity = Vector2.Zero;
				return false;
			}

			return true;
		}

		public override void PostAI(NPC npc) {
			if (!npc.boss && npc.HasBuff<FrostleafBindDebuff>())
				npc.velocity = Vector2.Zero;
			else if (npc.HasBuff<FrostleafSlowDebuff>())
				npc.velocity *= 0.5f;
		}
	}
}
