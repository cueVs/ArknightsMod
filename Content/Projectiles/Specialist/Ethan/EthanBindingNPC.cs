using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ArknightsMod.Content.Projectiles.Specialist.Ethan;

public sealed class EthanBind : ModBuff
{
    public override string Texture => "Terraria/Images/Buff_" + BuffID.Webbed;
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = Main.buffNoSave[Type] = Main.buffNoTimeDisplay[Type] = true;
    }
}
public sealed class EthanBindingNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;
    private ulong[] nextRoll;
    private int remaining;
    private Vector2 anchor;
    internal bool CanRoll(int owner)
    {
        nextRoll ??= new ulong[Main.maxPlayers];
        if (Main.GameUpdateCount < nextRoll[owner]) return false;
        nextRoll[owner] = Main.GameUpdateCount + 42;
        return true;
    }
    public override void PostAI(NPC npc)
    {
        if (!EthanCombat.CanBind(npc)) { remaining = 0; return; }
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            int index = npc.FindBuffIndex(ModContent.BuffType<EthanBind>());
            if (index >= 0)
            {
                if (remaining <= 0) anchor = npc.position;
                remaining = Math.Max(remaining, Math.Clamp(npc.buffTime[index], 1, 180));
                npc.DelBuff(index);
                npc.netUpdate = true;
            }
        }
        if (remaining <= 0) return;
        // Bind movement while allowing the NPC's attack AI and timers to continue.
        npc.position = anchor;
        npc.velocity = Vector2.Zero;
        if (--remaining == 0 && Main.netMode != NetmodeID.MultiplayerClient) npc.netUpdate = true;
        if (!Main.dedServ && Main.rand.NextBool(4))
            EthanVisuals.Scatter(npc.Center + Main.rand.NextVector2Circular(npc.width * .4f, npc.height * .4f), 1, .6f);
    }
    public override void PostDraw(NPC npc, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (remaining > 0) EthanVisuals.Binding(npc.Center, Math.Min(65f, Math.Max(npc.width, npc.height) * .6f), remaining);
    }
    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter writer)
    {
        writer.Write((short)remaining);
        if (remaining > 0) { writer.Write(anchor.X); writer.Write(anchor.Y); }
    }
    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader reader)
    {
        remaining = reader.ReadInt16();
        if (remaining > 0) anchor = new Vector2(reader.ReadSingle(), reader.ReadSingle());
    }
}
