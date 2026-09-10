using System;
using ArknightsMod.Content.Items.Weapons.Medic.Sussurro;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Medic.Sussurro;

public sealed class SussurroHealingButterfly : ModProjectile
{
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 12;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 180;
        Projectile.netImportant = true;
    }
    public override bool? CanDamage() => false;
    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        int targetIndex = (int)Projectile.ai[0];
        if (!Main.player.IndexInRange(targetIndex) || owner.HeldItem.ModItem is not SussurroStaff
            || !owner.active || owner.dead || !SussurroStaffPlayer.CanTreat(owner, Main.player[targetIndex]))
        {
            Projectile.Kill();
            return;
        }
        Player target = Main.player[targetIndex];
        Projectile.localAI[0]++;
        float age = Projectile.localAI[0];
        Vector2 toTarget = target.MountedCenter - Projectile.Center;
        // LivingShard 蝴蝶先舒展开翅膀，再以柔和惯性靠近；近身治疗不必等命中敌人。
        if (age < 10f)
            Projectile.velocity *= .93f;
        else
        {
            float speed = MathHelper.Clamp(8f + toTarget.Length() * .035f + (age - 10f) * .08f, 8f, 28f);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget.SafeNormalize(Vector2.UnitY) * speed, .16f);
        }
        if (!Main.dedServ && (int)age % 3 == 0)
            SussurroVisuals.Firefly(Projectile.Center, -Projectile.velocity * .12f, .65f);
        if (age >= 10f && toTarget.Length() < 24f && Projectile.owner == Main.myPlayer)
        {
            int amount = Math.Min(Math.Clamp((int)Projectile.ai[1], 0, 12), Math.Max(0, target.statLifeMax2 - target.statLife));
            if (amount > 0)
            {
                if (target.whoAmI == Main.myPlayer)
                {
                    target.Heal(amount);
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        NetMessage.SendData(MessageID.PlayerLifeMana, -1, -1, null, target.whoAmI);
                }
                else if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    // 队友治疗只发一次原版治疗消息；不先在本地叠加再重复广播同一回血。
                    NetMessage.SendData(MessageID.SpiritHeal, -1, -1, null, target.whoAmI, amount);
                }
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.MountedCenter, Vector2.Zero,
                    ModContent.ProjectileType<SussurroHealingBloom>(), 0, 0f, Projectile.owner,
                    Projectile.ai[2] > 0f ? 1f : .65f, target.whoAmI);
            }
            Projectile.Kill();
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        SussurroVisuals.DrawButterfly(Projectile.Center, Projectile.velocity.X * .025f,
            Projectile.localAI[0], Projectile.ai[2] > 0f ? 1.1f : .85f, 1f);
        return false;
    }
}

public sealed class SussurroHealingBloom : ModProjectile
{
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 40;
    }
    public override bool? CanDamage() => false;
    public override bool ShouldUpdatePosition() => false;
    public override void AI()
    {
        int target = (int)Projectile.ai[1];
        if (Main.player.IndexInRange(target) && Main.player[target].active)
            Projectile.Center = Main.player[target].MountedCenter;
        if (Projectile.localAI[0]++ == 0 && !Main.dedServ)
        {
            SussurroVisuals.HealBloom(Projectile.Center, Projectile.ai[0]);
            SoundEngine.PlaySound(SoundID.Item4 with { Volume = .24f, Pitch = .35f, MaxInstances = 3 }, Projectile.Center);
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        SussurroVisuals.DrawHealSeal(Projectile.Center, 40 - Projectile.timeLeft, Projectile.ai[0]);
        return false;
    }
}

public sealed class SussurroMedicalAura : ModProjectile
{
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 30;
        Projectile.netImportant = true;
    }
    public override bool? CanDamage() => false;
    public override bool ShouldUpdatePosition() => false;
    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead || owner.HeldItem.ModItem is not SussurroStaff)
        {
            Projectile.Kill();
            return;
        }
        Projectile.Center = owner.MountedCenter;
        Projectile.localAI[0]++;
        if (Projectile.owner == Main.myPlayer)
        {
            SussurroStaffPlayer medic = owner.GetModPlayer<SussurroStaffPlayer>();
            if (medic.Casting)
                Projectile.timeLeft = 30;
            float deep = medic.DeepTreatment ? 1f : 0f;
            if (Projectile.ai[0] != deep || (int)Projectile.localAI[0] % 15 == 0)
            {
                Projectile.ai[0] = deep;
                Projectile.netUpdate = true;
            }
        }
        if (!Main.dedServ && Projectile.ai[0] > 0f && (int)Projectile.localAI[0] % 8 == 0)
        {
            Vector2 offset = (Projectile.localAI[0] * .1f).ToRotationVector2() * new Vector2(56f, 28f);
            SussurroVisuals.Firefly(Projectile.Center + offset, new Vector2(0, -1.1f), .8f);
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        float fade = Math.Min(1f, Projectile.localAI[0] / 15f) * Math.Min(1f, Projectile.timeLeft / 12f);
        SussurroVisuals.DrawAura(Projectile.Center, Projectile.localAI[0], Projectile.ai[0] > 0f, fade);
        return false;
    }
}
