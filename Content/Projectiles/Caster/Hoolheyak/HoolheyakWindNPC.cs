using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ArknightsMod.Content.Projectiles.Caster.Hoolheyak;

/// <summary>使用原生 NPC Buff 同步命中产生的浮空请求；客户端不直接推动服务端敌人。</summary>
public sealed class HoolheyakUpdraft : ModBuff
{
    public override string Texture => "Terraria/Images/Buff_" + BuffID.Featherfall;
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
    }
}

public sealed class HoolheyakWindNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;
    private int liftRemaining;
    private int recovery;
    private float hoverY;
    internal bool IsLifted => liftRemaining > 0;

    public override bool PreAI(NPC npc) => !IsLifted || !HoolheyakWindCombat.CanLift(npc);

    public override void PostAI(NPC npc)
    {
        if (!HoolheyakWindCombat.CanLift(npc))
        {
            liftRemaining = 0;
            return;
        }
        if (recovery > 0)
            recovery--;

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            int buffIndex = npc.FindBuffIndex(ModContent.BuffType<HoolheyakUpdraft>());
            if (buffIndex >= 0)
            {
                if (recovery == 0 && !IsLifted)
                    BeginLift(npc, npc.buffTime[buffIndex]);
                npc.DelBuff(buffIndex);
            }
        }
        if (IsLifted)
        {
            // 高度锚定第一次浮空的位置，连续命中不能无限向天上搬运 NPC。
            npc.velocity.X *= .85f;
            npc.velocity.Y = MathHelper.Clamp((hoverY - npc.Center.Y) * .15f, -3.2f, 2f);
            if (--liftRemaining == 0)
            {
                recovery = 120;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    npc.netUpdate = true;
            }
            return;
        }
        if (Main.netMode == NetmodeID.MultiplayerClient || recovery > 0)
            return;

        Projectile nearest = null;
        float best = float.MaxValue;
        foreach (Projectile projectile in Main.ActiveProjectiles)
        {
            if (projectile.ModProjectile is not HoolheyakSmallVortex vortex)
                continue;
            Player owner = Main.player[projectile.owner];
            if (!owner.active || owner.dead || owner.HeldItem.ModItem is not Items.Weapons.Caster.Hoolheyak.HoolheyakWindArts)
                continue;
            float distance = npc.DistanceSQ(projectile.Center);
            if (distance < vortex.PullRadius * vortex.PullRadius && distance < best
                && HoolheyakWindCombat.ClearPath(npc.Center, projectile.Center))
            {
                best = distance;
                nearest = projectile;
            }
        }
        if (nearest == null)
            return;
        float radius = ((HoolheyakSmallVortex)nearest.ModProjectile).PullRadius;
        float closeness = 1f - MathF.Sqrt(best) / radius;
        Vector2 toward = (nearest.Center - npc.Center).SafeNormalize(Vector2.Zero);
        // 参照 DarkPlasma 的径向渐进吸附，近风眼再托起；多个风场取最近一个，不叠加拉力。
        Vector2 desiredVelocity = toward * MathHelper.Lerp(1.8f, 5f, closeness);
        npc.velocity = Vector2.Lerp(npc.velocity, desiredVelocity, .06f + .12f * closeness);
        if ((Main.GameUpdateCount + (ulong)npc.whoAmI) % 12 == 0)
            npc.netUpdate = true;
        if (best < 64f * 64f)
            BeginLift(npc, 24);
    }

    private void BeginLift(NPC npc, int duration)
    {
        liftRemaining = Math.Clamp(duration, 1, 240);
        hoverY = npc.Center.Y - 56f;
        npc.netUpdate = true;
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        bitWriter.WriteBit(liftRemaining > 0 || recovery > 0);
        if (liftRemaining > 0 || recovery > 0)
        {
            binaryWriter.Write((short)liftRemaining);
            binaryWriter.Write((short)recovery);
            binaryWriter.Write(hoverY);
        }
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        if (bitReader.ReadBit())
        {
            liftRemaining = binaryReader.ReadInt16();
            recovery = binaryReader.ReadInt16();
            hoverY = binaryReader.ReadSingle();
        }
        else
            liftRemaining = recovery = 0;
    }
}
