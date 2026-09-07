using System;
using System.Collections.Generic;
using System.IO;
using ArknightsMod.Content.Buffs;
using ArknightsMod.Content.Items.Weapons.Caster.Passenger;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Passenger;

/// <summary>实体闪电撞敌后的传导，以及三技能天降雷暴；按确定顺序在敌人间跳跃。</summary>
public class PassengerChainDischarge : ModProjectile
{
    private readonly HashSet<int> hitEnemies = new();
    private readonly int[] targets = new int[5];
    private readonly int[] targetTypes = new int[5];
    private readonly Vector2[] endpoints = new Vector2[5];
    private int count;
    private bool ready;
    private bool impactTriggered;
    private int Age => (int)Projectile.localAI[0];
    private int TargetLimit => Math.Clamp(Math.Abs((int)Projectile.ai[2]), 1, 5);
    private bool IsStorm => Projectile.ai[2] < 0f;
    internal virtual LightningPalette Palette => LightningPalette.Passenger;
    protected virtual float VisualSize => 1f;
    private float Intensity => MathHelper.Clamp(Projectile.ai[0], 1f, 1.5f) * VisualSize;
    private float Range => IsStorm ? 1280f : TargetLimit == 5 ? PassengerConductor.FocusRange : PassengerConductor.NormalRange;
    // velocity 只存固定雷暴中心相对于起点的偏移，不参与移动。
    private Vector2 StormCenter => Projectile.Center + Projectile.velocity;
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;
    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1400;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 8;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 38;
        Projectile.netImportant = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    internal void InitializeRoute(Player owner, Vector2 aim, NPC firstHit = null)
    {
        HashSet<int> selected = new();
        Vector2 previous = Projectile.Center;
        impactTriggered = firstHit != null && !IsStorm;
        if (impactTriggered)
        {
            targets[0] = firstHit.whoAmI;
            targetTypes[0] = firstHit.type;
            endpoints[0] = firstHit.Center;
            selected.Add(PassengerTargeting.EnemyIdentity(firstHit));
            previous = firstHit.Center;
            count = 1;
        }
        for (int hop = count; hop < TargetLimit; hop++)
        {
            NPC best = null;
            float nearest = hop == 0 ? 240f * 240f : PassengerConductor.JumpRange * PassengerConductor.JumpRange;
            Vector2 reference = hop == 0 ? aim : previous;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy(Projectile) || selected.Contains(PassengerTargeting.EnemyIdentity(npc))
                    || !impactTriggered && npc.Distance(owner.MountedCenter) > Range
                    || !impactTriggered && !PassengerTargeting.ClearPath(hop == 0 && IsStorm ? StormCenter : previous, npc.Center)
                    || IsStorm && (npc.Distance(StormCenter) > PassengerThunderstorm.Radius
                        || !PassengerTargeting.ClearPath(StormCenter, npc.Center)))
                    continue;
                float distance = Vector2.DistanceSquared(reference, npc.Center);
                if (distance >= nearest)
                    continue;
                nearest = distance;
                best = npc;
            }
            if (best == null)
                break;
            targets[count] = best.whoAmI;
            targetTypes[count] = best.type;
            endpoints[count++] = best.Center;
            selected.Add(PassengerTargeting.EnemyIdentity(best));
            previous = best.Center;
        }
        // 空挥只做装饰放电，不在鼠标位置生成隐形范围伤害。
        if (count == 0)
        {
            targets[0] = -1;
            endpoints[0] = aim;
            if (!IsStorm)
            {
                Vector2 delta = aim - Projectile.Center;
                Vector2 end = Projectile.Center;
                int steps = Math.Max(1, (int)MathF.Ceiling(delta.Length() / 16f));
                for (int i = 1; i <= steps; i++)
                {
                    Vector2 next = Projectile.Center + delta * (i / (float)steps);
                    if (!PassengerTargeting.ClearPath(end, next))
                        break;
                    end = next;
                }
                endpoints[0] = end;
            }
            count = 1;
        }
        ready = true;
        Projectile.netUpdate = true;
    }

    // 路径由持有者选择一次并同步；其他客户端不读取自己的鼠标或另选一组目标。
    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(ready);
        writer.Write(impactTriggered);
        writer.Write((byte)count);
        writer.Write((byte)Math.Clamp(Age, 0, 255));
        for (int i = 0; i < count; i++)
        {
            writer.Write((short)targets[i]);
            writer.Write(targetTypes[i]);
            writer.Write(endpoints[i].X);
            writer.Write(endpoints[i].Y);
        }
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        ready = reader.ReadBoolean();
        impactTriggered = reader.ReadBoolean();
        int incomingCount = reader.ReadByte();
        Projectile.localAI[0] = reader.ReadByte();
        count = Math.Min(incomingCount, targets.Length);
        for (int i = 0; i < incomingCount; i++)
        {
            int target = reader.ReadInt16();
            int type = reader.ReadInt32();
            Vector2 point = new(reader.ReadSingle(), reader.ReadSingle());
            if (i >= count)
                continue;
            targets[i] = target;
            targetTypes[i] = type;
            endpoints[i] = point;
            if (!float.IsFinite(point.X) || !float.IsFinite(point.Y))
                ready = false;
        }
    }

    private Vector2 SegmentStart(int hop) => hop > 0 ? endpoints[hop - 1]
        : IsStorm ? endpoints[0] - Vector2.UnitY * 720f : Projectile.Center;

    public override bool ShouldUpdatePosition() => false;
    public override bool? CanDamage() => ready && Age >= 3 && Age <= 5 + (count - 1) * 3 ? null : false;
    public override bool CanHitPvp(Player target) => false;
    public override bool? CanCutTiles() => false;

    public override bool? CanHitNPC(NPC target)
    {
        int hop = Array.IndexOf(targets, target.whoAmI, 0, count);
        if (!ready || hop < 0 || impactTriggered && hop == 0
            || target.type != targetTypes[hop] || !target.CanBeChasedBy(Projectile)
            || hitEnemies.Contains(PassengerTargeting.EnemyIdentity(target)))
            return false;
        int localAge = Age - hop * 3;
        Vector2 source = hop == 0 && IsStorm ? StormCenter : SegmentStart(hop);
        if (localAge < 3 || localAge > 5
            || !impactTriggered && target.Distance(Main.player[Projectile.owner].MountedCenter) > Range
            || !impactTriggered && !PassengerTargeting.ClearPath(source, target.Center)
            || IsStorm && target.Distance(StormCenter) > PassengerThunderstorm.Radius)
            return false;
        // 每个节点只结算已锁定的敌人，不让装饰分叉与重叠光效重复造成伤害。
        return null;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => true;

    public override void AI()
    {
        if (!ready)
            return;
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead)
        {
            Projectile.Kill();
            return;
        }
        Projectile.localAI[0]++;
        for (int hop = 0; hop < count; hop++)
        {
            int localAge = Age - hop * 3;
            if (localAge < 1 || localAge > 5)
                continue;
            Vector2 start = SegmentStart(hop);
            Vector2 end = endpoints[hop];
            int seed = Projectile.identity * 397 ^ Projectile.owner ^ hop * 7919;
            float nodeIntensity = Intensity * (hop == 0 ? 1f : 0.7f);
            bool targetNode = targets[hop] >= 0 || IsStorm;
            if (localAge == 1)
            {
                PassengerLightningVisuals.Fracture(start, end, Intensity, seed, 3f, 24, Palette);
                if (targetNode)
                    PassengerLightningVisuals.PrepareImpact(end, nodeIntensity, seed, Palette);
            }
            if (localAge <= 3)
            {
                float previous = MathHelper.SmoothStep(0f, 1f, (localAge - 1f) / 3f);
                float progress = MathHelper.SmoothStep(0f, 1f, localAge / 3f);
                PassengerLightningVisuals.EmitSpan(Vector2.Lerp(start, end, previous),
                    Vector2.Lerp(start, end, progress), Intensity, localAge * 22 + hop * 13, Palette);
            }
            if (localAge == 3 && targetNode)
            {
                PassengerLightningVisuals.Burst(end, nodeIntensity * 0.65f, seed, Palette);
                PassengerLightningVisuals.Impact(end, nodeIntensity, seed, hop == 0, Palette);
                if (impactTriggered)
                    PassengerLightningVisuals.ElectricImpactDust(end, nodeIntensity, hop == 0, Palette);
            }
            if (!Main.dedServ)
                Lighting.AddLight(end, new Vector3(0.65f, 0.30f, 0.045f) * nodeIntensity);
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        int hop = Array.IndexOf(targets, target.whoAmI, 0, count);
        modifiers.SourceDamage *= MathF.Pow(0.85f, Math.Max(0, hop));
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        hitEnemies.Add(PassengerTargeting.EnemyIdentity(target));
        if (!target.boss && target.knockBackResist > 0f && target.realLife < 0)
            target.AddBuff(ModContent.BuffType<PassengerCurrentSlow>(), (int)Projectile.ai[1]);
    }

    public override bool PreDraw(ref Color lightColor) => false;
}

/// <summary>没有敌人时的短放电以及雷暴装饰电弧。永不产生伤害。</summary>
public sealed class PassengerArcVisual : ModProjectile
{
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 24;
        Projectile.penetrate = -1;
    }
    public override bool ShouldUpdatePosition() => false;
    public override bool? CanDamage() => false;
    public override bool PreDraw(ref Color lightColor) => false;
    public override void AI()
    {
        if (Projectile.localAI[0]++ != 0)
            return;
        Vector2 end = Projectile.Center + Projectile.velocity;
        PassengerLightningVisuals.Fracture(Projectile.Center, end, Projectile.ai[0], Projectile.identity * 397, 3f, 22);
        PassengerLightningVisuals.EmitSpan(Projectile.Center, end, Projectile.ai[0], Projectile.identity * 13);
    }
}
