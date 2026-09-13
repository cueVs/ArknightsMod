using ArknightsMod.Content.Items.Weapons.Medic.Sussurro;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Medic.Sussurro;

public sealed class SussurroStaffHoldout : ModProjectile
{
    private const int BurstDelay = 20, ShotInterval = 4, ShotsPerBurst = 3;
    private int attackClock = BurstDelay;
    private int burstShots;
    public override string Texture => "Terraria/Images/Item_" + ItemID.EmeraldStaff;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 40;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 2;
        Projectile.netImportant = true;
    }
    public override bool? CanDamage() => false;
    public override bool ShouldUpdatePosition() => false;
    private bool AdvanceBurstClock()
    {
        if (--attackClock > 0) return false;
        burstShots++;
        attackClock = burstShots < ShotsPerBurst ? ShotInterval : BurstDelay;
        if (burstShots == ShotsPerBurst) burstShots = 0;
        return true;
    }
    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead || owner.CCed || owner.noItems || owner.HeldItem.ModItem is not SussurroStaff
            || (Projectile.owner == Main.myPlayer && !SussurroStaff.CanHoldRightClick(owner)))
        {
            Projectile.Kill();
            return;
        }
        if (Projectile.owner == Main.myPlayer)
        {
            Vector2 aim = (Main.MouseWorld - owner.MountedCenter).SafeNormalize(Vector2.UnitX * owner.direction);
            if (Vector2.DistanceSquared(aim, Projectile.velocity) > .0004f)
            {
                Projectile.velocity = aim;
                Projectile.netUpdate = true;
            }
        }
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX * owner.direction);
        Projectile.Center = owner.MountedCenter + direction * 22f;
        Projectile.rotation = direction.ToRotation();
        Projectile.timeLeft = 2;
        owner.ChangeDir(direction.X < 0f ? -1 : 1);
        owner.heldProj = Projectile.whoAmI;
        owner.itemTime = owner.itemAnimation = 2;
        owner.itemRotation = (direction * owner.direction).ToRotation();
        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

        // 新手持弹幕也要先等待完整一轮；松手重建无法跳过首轮等待。
        if (Projectile.owner != Main.myPlayer || !AdvanceBurstClock())
            return;
        if (!owner.CheckMana(owner.HeldItem, pay: true))
        {
            Projectile.Kill();
            return;
        }
        owner.manaRegenDelay = (int)owner.maxRegenDelay;
        owner.GetModPlayer<SussurroStaffPlayer>().ReleaseButterflies(owner.MountedCenter + direction * 40f, direction);
        SoundEngine.PlaySound(SoundID.Item20 with { Volume = .3f, Pitch = .4f, MaxInstances = 3 }, Projectile.Center);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor,
            Projectile.rotation + MathHelper.PiOver4, texture.Size() * .5f, Projectile.scale, SpriteEffects.None);
        return false;
    }
}
