using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Defender.Horn;

public sealed class HornGrenade : ModProjectile
{
    internal byte VisualState;
    public override void SendExtraAI(BinaryWriter writer) => writer.Write(VisualState);
    public override void ReceiveExtraAI(BinaryReader reader) => VisualState = reader.ReadByte();
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GrenadeI;
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 10;
        ProjectileID.Sets.TrailingMode[Type] = 0;
    }
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 12;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 150;
    }
    public override bool? CanDamage() => false; // All damage belongs to the single explosion, never impact + explosion.
    public override void AI()
    {
        Projectile.velocity.Y = MathHelper.Clamp(Projectile.velocity.Y + .16f, -20f, 20f);
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, .6f, .35f, .08f);
        if (!Main.dedServ && Main.rand.NextBool(2))
        {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, -Projectile.velocity * .15f, 100, default, 1.2f);
            dust.noGravity = true;
        }
        HornVisuals.SkillTrail(Projectile, VisualState);
        if (Projectile.owner != Main.myPlayer) return;
        foreach (NPC npc in Main.ActiveNPCs)
            if (npc.CanBeChasedBy(Projectile) && npc.Hitbox.Intersects(Projectile.Hitbox))
            { Projectile.Kill(); return; }
    }
    public override bool OnTileCollide(Vector2 oldVelocity) => true;
    public override void OnKill(int timeLeft)
    {
        if (Projectile.owner == Main.myPlayer)
            Explode(Projectile, Projectile.Center, Projectile.ai[0], (int)Projectile.ai[1], (int)Projectile.ai[2]);
        SoundEngine.PlaySound(SoundID.Item14 with { Volume = .55f, MaxInstances = 5 }, Projectile.Center);
    }
    internal static void Explode(Projectile source, Vector2 center, float radius, int flare, int arts)
    {
        Spawn(source.damage, 0);
        if (arts > 0) Spawn(arts, 1);
        if (flare > 0)
            Projectile.NewProjectile(source.GetSource_FromThis(), center, Vector2.Zero, ModContent.ProjectileType<HornFlare>(),
                0, 0f, source.owner, radius, flare);
        void Spawn(int damage, int kind)
        {
            int index = Projectile.NewProjectile(source.GetSource_FromThis(), center, Vector2.Zero,
                ModContent.ProjectileType<HornBurst>(), damage, source.knockBack, source.owner, radius, kind, (source.ModProjectile as HornGrenade)?.VisualState ?? 0);
            if (Main.projectile.IndexInRange(index)) Main.projectile[index].CritChance = source.CritChance;
        }
    }
}
