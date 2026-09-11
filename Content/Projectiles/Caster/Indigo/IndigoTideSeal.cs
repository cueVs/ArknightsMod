using System;
using System.IO;
using ArknightsMod.Content.Items.Weapons.Caster.Indigo;
using ArknightsMod.Content.Projectiles.Caster.Ebenholz;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ArknightsMod.Content.Projectiles.Caster.Indigo;

public sealed class IndigoTideSeal : ModProjectile
{
    public override string Texture => "Terraria/Images/Projectile_0";
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 180;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 29;
        Projectile.netImportant = true;
    }

    internal static void TryApply(Projectile source, NPC hit)
    {
        if (source.owner != Main.myPlayer || !hit.active || hit.life <= 0 || hit.friendly) return;
        Player owner = Main.player[source.owner];
        if (owner.GetModPlayer<IndigoStaffPlayer>().Mode != 2 || !Main.rand.NextBool(3, 5)) return;
        int identity = EbenholzCombat.Identity(hit);
        NPC target = Main.npc[identity];
        if (!target.active || target.life <= 0) return;
        if (EbenholzCombat.CanPull(target)) target.AddBuff(ModContent.BuffType<IndigoTideBind>(), 120);
        int damage = Math.Max(1, (int)MathF.Round(owner.GetWeaponDamage(owner.HeldItem) * .25f));
        int type = ModContent.ProjectileType<IndigoTideSeal>();
        foreach (Projectile p in Main.ActiveProjectiles)
        {
            if (p.type != type || p.owner != source.owner || (int)p.ai[0] != identity + 1) continue;
            // 延长潮印，不重置30帧伤害节拍；连续命中不能把伤害一直推迟或叠出多个潮印。
            p.ai[2] = 120;
            p.damage = damage;
            p.netUpdate = true;
            return;
        }
        if (owner.ownedProjectileCounts[type] >= 12) return;
        int index = Projectile.NewProjectile(source.GetSource_OnHit(hit), target.Center, Vector2.Zero,
            type, damage, 0f, source.owner, identity + 1, target.type, 120);
        if (Main.projectile.IndexInRange(index)) Main.projectile[index].CritChance = 0;
    }

    public override void AI()
    {
        int index = (int)Projectile.ai[0] - 1;
        Player owner = Main.player[Projectile.owner];
        if (index < 0 || index >= Main.maxNPCs || !owner.active || owner.dead || owner.HeldItem.ModItem is not IndigoStaff
            || Projectile.ai[2]-- <= 0 || (Projectile.owner == Main.myPlayer && owner.GetModPlayer<IndigoStaffPlayer>().Mode != 2))
        { Projectile.Kill(); return; }
        NPC target = Main.npc[index];
        if (!target.active || target.life <= 0 || target.type != (int)Projectile.ai[1]
            || target.DistanceSQ(owner.Center) > 900f * 900f)
        { Projectile.Kill(); return; }
        Projectile.timeLeft = 2;
        Projectile.Center = target.Center;
        Projectile.localAI[0]++;
        if ((int)Projectile.localAI[0] % 30 == 0) IndigoVisuals.Scatter(Projectile.Center, 4, 1.5f);
    }
    public override bool? CanDamage() => (int)Projectile.localAI[0] % 30 == 0 && Projectile.localAI[0] > 0 ? null : false;
    public override bool? CanHitNPC(NPC target) => target.whoAmI == (int)Projectile.ai[0] - 1
        && EbenholzCombat.Clear(Main.player[Projectile.owner].Center, target.Center) ? null : false;
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => true;
    public override bool PreDraw(ref Color lightColor)
    {
        int index = (int)Projectile.ai[0] - 1;
        if (index < 0 || index >= Main.maxNPCs) return false;
        NPC npc = Main.npc[index];
        float radius = Math.Clamp(Math.Max(npc.width, npc.height) * .6f, 22f, 70f);
        IndigoVisuals.Seal(Projectile.Center, radius, Projectile.localAI[0], Math.Min(1f, Projectile.ai[2] / 20f));
        return false;
    }
}

public sealed class IndigoTideBind : ModBuff
{
    public override string Texture => "ArknightsMod/Content/Buffs/ArmorSets/IndigoBindDebuff";
    public override void SetStaticDefaults()
        => Main.debuff[Type] = Main.buffNoSave[Type] = Main.buffNoTimeDisplay[Type] = true;
}

public sealed class IndigoBindingNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;
    private int remaining;
    private Vector2 anchor;
    public override void PostAI(NPC npc)
    {
        if (!EbenholzCombat.CanPull(npc)) { remaining = 0; return; }
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            int buff = npc.FindBuffIndex(ModContent.BuffType<IndigoTideBind>());
            if (buff >= 0)
            {
                if (remaining <= 0) anchor = npc.position;
                remaining = Math.Max(remaining, Math.Clamp(npc.buffTime[buff], 1, 120));
                npc.DelBuff(buff);
                npc.netUpdate = true;
            }
        }
        if (remaining <= 0) return;
        npc.position = anchor;
        npc.velocity = Vector2.Zero;
        if (--remaining == 0 && Main.netMode != NetmodeID.MultiplayerClient) npc.netUpdate = true;
    }
    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (remaining > 0)
            IndigoVisuals.Ripple(npc.Bottom, Math.Clamp(npc.width * .6f, 16f, 65f), remaining * .025f, .3f,
                IndigoVisuals.Light(IndigoVisuals.Pearl, Math.Min(1f, remaining / 15f) * .7f), 1.2f);
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
