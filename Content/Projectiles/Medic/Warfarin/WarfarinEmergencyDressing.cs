using System;
using ArknightsMod.Content.Items.Weapons.Medic;
using ArknightsMod.Content.Items.Weapons.Medic.Sussurro;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Medic.Warfarin;

/// <summary>紧急包扎抛出的血浆包；先越过目标，再在半秒后回转结算医疗。</summary>
public sealed class WarfarinEmergencyDressing : ModProjectile
{
    public override string Texture => "Terraria/Images/Extra_98";

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 18;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 180;
        Projectile.netImportant = true;
    }

    public override bool? CanDamage() => false;

    public override void AI()
    {
        Projectile.localAI[0]++;
        float age = Projectile.localAI[0];
        Player owner = Main.player[Projectile.owner];
        int targetIndex = (int)Projectile.ai[0];
        Player target = Main.player.IndexInRange(targetIndex) ? Main.player[targetIndex] : owner;
        if (!owner.active || owner.dead || !target.active || target.dead)
        {
            Projectile.Kill();
            return;
        }

        if (age < MedicalTreatment.InitialTreatmentDelayTicks)
        {
            Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + .14f, 10f);
        }
        else
        {
            Vector2 toTarget = target.MountedCenter - Projectile.Center;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity,
                toTarget.SafeNormalize(Vector2.UnitY) * 11f, .13f);
            if (toTarget.LengthSquared() < 24f * 24f && Projectile.owner == Main.myPlayer)
            {
                int amount = MedicalTreatment.Apply(target, Math.Max(1, (int)Projectile.ai[1]));
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
                        NetMessage.SendData(MessageID.SpiritHeal, -1, -1, null, target.whoAmI, amount);
                    }
                    Burst(target.MountedCenter, 12);
                    SoundEngine.PlaySound(SoundID.Item4 with { Volume = .3f, Pitch = -.1f }, target.MountedCenter);
                }
                Projectile.Kill();
                return;
            }
        }

        Projectile.rotation = Projectile.velocity.ToRotation();
        Lighting.AddLight(Projectile.Center, .42f, .025f, .09f);
        if (!Main.dedServ && Main.rand.NextBool(3))
            Dust.NewDustPerfect(Projectile.Center, DustID.Blood, -Projectile.velocity * .08f,
                20, default, .85f).noGravity = true;
    }

    private static void Burst(Vector2 position, int count)
    {
        if (Main.dedServ)
            return;
        for (int i = 0; i < count; i++)
        {
            Dust dust = Dust.NewDustPerfect(position, DustID.Blood,
                Main.rand.NextVector2Circular(2.5f, 2.5f), 20, default, Main.rand.NextFloat(.8f, 1.25f));
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D glow = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
        Vector2 origin = glow.Size() * .5f;
        Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null,
            new Color(125, 4, 30), Projectile.rotation, origin, new Vector2(25f, 17f) / glow.Size(), SpriteEffects.None);
        Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null,
            new Color(255, 105, 135, 0), Projectile.rotation, origin, new Vector2(17f, 10f) / glow.Size(), SpriteEffects.None);
        return false;
    }
}
