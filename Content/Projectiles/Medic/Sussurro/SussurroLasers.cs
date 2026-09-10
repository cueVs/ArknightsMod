using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Medic.Sussurro;

/// <summary>高速针束：15 次更新、双层拉长光粒，共四次命中额度，撞墙绽放蝶群并结束。</summary>
public sealed class SussurroNeedle : ModProjectile
{
    private readonly HashSet<int> hitBodies = new();
    private int impactCount;
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 18;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 4;
        Projectile.extraUpdates = 14;
        Projectile.timeLeft = 20 * 15;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }
    public override bool CanHitPvp(Player target) => false;
    public override bool? CanCutTiles() => false;
    public override bool? CanHitNPC(NPC target) => target.CanBeChasedBy(Projectile)
        && !hitBodies.Contains(target.realLife >= 0 ? target.realLife : target.whoAmI) ? null : false;
    public override void AI()
    {
        if (!Main.player[Projectile.owner].active || Main.player[Projectile.owner].dead)
        {
            Projectile.Kill();
            return;
        }
        Projectile.rotation = Projectile.velocity.ToRotation();
        Projectile.localAI[0]++;
        if (Main.dedServ)
            return;
        if (Projectile.localAI[0] == 5f)
            SussurroVisuals.Muzzle(Projectile.Center, Projectile.rotation, Projectile.ai[0] > 0f ? 1.2f : .85f);
        if (Projectile.localAI[0] > 6f && Projectile.numUpdates % 3 == 0)
        {
            float boost = MathHelper.Clamp(Projectile.localAI[0] * .005f, 0f, 1.5f);
            // 沿用 Halley 的 7 帧光粒残留，额外更新期间节流，不每次更新都喷粒子。
            SussurroVisuals.Needle(Projectile.Center, Projectile.velocity, 1f + boost * .2f, 7);
        }
        if (Projectile.numUpdates == 0)
        {
            Lighting.AddLight(Projectile.Center, .12f, .55f, .22f);
            SussurroVisuals.Firefly(Projectile.Center, -Projectile.velocity * .06f, .6f);
        }
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        hitBodies.Add(target.realLife >= 0 ? target.realLife : target.whoAmI);
        // 穿透不衰减伤害，只有重型视觉按每束数量限流；不产生每个目标一份回血。
        if (impactCount++ < 3)
            SussurroVisuals.Impact(target.Center, Projectile.rotation, Projectile.ai[0] > 0f ? .85f : .6f);
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        SussurroVisuals.ButterflyBurst(Projectile.Center, oldVelocity.ToRotation(), Projectile.ai[0] > 0f ? 1.1f : .85f);
        return true;
    }
    public override bool PreDraw(ref Color lightColor) => false;
}

/// <summary>借鉴 ExoPrism 的短暂压缩束：束长锁定、先聚后散，每次仅伤害同一敌人一次。</summary>
public sealed class SussurroCompressionRay : ModProjectile
{
    internal const float BeamLength = 1500f;
    private readonly HashSet<int> hitBodies = new();
    private int impacts;
    private bool traced, hitWall;
    private float beamLength;
    private Vector2 Direction => Projectile.velocity.SafeNormalize(Vector2.UnitX);
    private float Age => 14 - Projectile.timeLeft;
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;
    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1600;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 14;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }
    public override bool ShouldUpdatePosition() => false;
    public override bool? CanDamage() => Age >= 2f && impacts < 4 && beamLength > 0f ? null : false;
    public override bool CanHitPvp(Player target) => false;
    public override bool? CanCutTiles() => false;
    public override bool? CanHitNPC(NPC target) => impacts < 4 && target.CanBeChasedBy(Projectile)
        && !hitBodies.Contains(target.realLife >= 0 ? target.realLife : target.whoAmI) ? null : false;
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float collision = 0f;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
            Projectile.Center + Direction * beamLength, 14f, ref collision);
    }
    // 绘制与伤害共用到第一面实心墙为止的线段。
    private static float Trace(Vector2 start, Vector2 direction, float length, out bool hit)
    {
        hit = false;
        int x = (int)MathF.Floor(start.X / 16f), y = (int)MathF.Floor(start.Y / 16f);
        int stepX = Math.Sign(direction.X), stepY = Math.Sign(direction.Y);
        float deltaX = stepX == 0 ? float.PositiveInfinity : Math.Abs(16f / direction.X);
        float deltaY = stepY == 0 ? float.PositiveInfinity : Math.Abs(16f / direction.Y);
        float nextX = stepX == 0 ? float.PositiveInfinity : ((x + (stepX > 0 ? 1 : 0)) * 16f - start.X) / direction.X;
        float nextY = stepY == 0 ? float.PositiveInfinity : ((y + (stepY > 0 ? 1 : 0)) * 16f - start.Y) / direction.Y;
        float distance = 0f;
        while (distance < length)
        {
            if (x < 0 || y < 0 || x >= Main.maxTilesX || y >= Main.maxTilesY)
                return distance;
            Tile tile = Main.tile[x, y];
            if (tile.HasTile && !tile.IsActuated && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
            {
                hit = true;
                return Math.Max(0f, distance - .1f);
            }
            distance = Math.Min(nextX, nextY);
            bool crossedX = nextX <= nextY;
            bool crossedY = nextY <= nextX;
            if (crossedX) { x += stepX; nextX += deltaX; }
            if (crossedY) { y += stepY; nextY += deltaY; }
        }
        return length;
    }
    public override void AI()
    {
        if (!traced)
        {
            traced = true;
            beamLength = Trace(Projectile.Center, Direction, BeamLength, out hitWall);
        }
        if (!Main.player[Projectile.owner].active || Main.player[Projectile.owner].dead)
        {
            Projectile.Kill();
            return;
        }
        if (Main.dedServ)
            return;
        if (Age == 2)
        {
            SussurroVisuals.Muzzle(Projectile.Center, Direction.ToRotation(), 1.65f);
            if (hitWall)
                SussurroVisuals.ButterflyBurst(Projectile.Center + Direction * beamLength, Direction.ToRotation(), 1.25f);
            for (int i = 0; i < 12; i++)
            {
                float distance = 45f + i * 118f;
                if (distance > beamLength) break;
                Vector2 position = Projectile.Center + Direction * distance;
                SussurroVisuals.Firefly(position, Direction.RotatedByRandom(.5f) * Main.rand.NextFloat(1f, 3f), 1f);
            }
        }
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        hitBodies.Add(target.realLife >= 0 ? target.realLife : target.whoAmI);
        if (impacts++ < 3)
            SussurroVisuals.Impact(target.Center, Direction.ToRotation(), Projectile.ai[0] > 0f ? 1.45f : 1.1f);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        float fade = Age < 2f ? .14f : MathF.Pow(MathHelper.Clamp(Projectile.timeLeft / 12f, 0f, 1f), 1.4f);
        if (beamLength > 0f)
            SussurroVisuals.DrawCompression(Projectile.Center, Direction, beamLength, fade, Projectile.ai[0] > 0f);
        return false;
    }
}
