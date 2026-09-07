using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs;

public sealed class PassengerCurrentSlow : ModBuff
{
    public override string Texture => "Terraria/Images/Buff_" + BuffID.Electrified;
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
    }
}

// 原版 Slow 主要面向玩家；在 NPC 完成自身 AI 后缩减位移，才能可靠表现链术师的停顿。
public sealed class PassengerCurrentSlowNPC : GlobalNPC
{
    public override void PostAI(NPC npc)
    {
        if (!npc.boss && npc.realLife < 0 && npc.knockBackResist > 0f
            && npc.HasBuff(ModContent.BuffType<PassengerCurrentSlow>()))
            npc.velocity *= 0.2f;
    }
}
