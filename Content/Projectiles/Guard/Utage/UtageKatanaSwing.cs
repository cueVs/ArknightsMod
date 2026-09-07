using System;
using System.Collections.Generic;
using ArknightsMod.Common.Particle;
using ArknightsMod.Content.Items.Weapons.Guard.Utage;
using ArknightsMod.Content.Projectiles.BasePROJ;
using ArknightsMod.Content.Projectiles.Guard.Hellagur;
using ArknightsMod.Systems.Gameplay.Damage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Utage;

// 直接继承赫拉格三段普通挥砍、三次子更新、刀身扫掠碰撞和姿态同步。
// 不调用赫拉格的技能桥、回血或月相特效；原文件无需改动。
public sealed class UtageKatanaSwing : HellagurOdachiSwing
{
    private readonly HashSet<int> struck = new();
    private readonly float[] angles = new float[48];
    private readonly Vector2[] hands = new Vector2[48];
    private int samples;
    private float bladeLight;
    private Player Wielder => Main.player[Projectile.owner];
    private bool Arts => SkillMode == 4;
    protected override int BindingItemType() => ModContent.ItemType<UtageKatana>();
    protected override Texture2D GetWeaponTexture() => Main.dedServ ? null
        : TextureAssets.Item[ModContent.ItemType<UtageKatana>()].Value;
    public override bool CanHitPvp(Player target) => false;

    public override void AI()
    {
        if (Wielder.GetModPlayer<UtageCombatPlayer>().Resting)
        {
            Projectile.Kill();
            return;
        }
        // 在所有端根据 ai 快照设置，法抗由现有伤害系统处理，不扩充公共注册表。
        Projectile.GetGlobalProjectile<ArtsProjectileMarker>().IsArtsDamage = Arts;
        base.AI();
        if (!Projectile.active || Main.dedServ)
            return;
        bladeLight = MathHelper.Lerp(bladeLight, Damaging ? 1f : 0f, Damaging ? .18f : .12f);
        if (Damaging)
        {
            for (int i = angles.Length - 1; i > 0; i--)
            {
                angles[i] = angles[i - 1];
                hands[i] = hands[i - 1];
            }
            angles[0] = CurrentAngle;
            hands[0] = HandWorld;
            samples = Math.Min(samples + 1, angles.Length);
        }
        else
            samples = Math.Max(0, samples - 2);
        Lighting.AddLight(TipWorld, new Vector3(.33f, .065f, .5f) * bladeLight);
    }

    public override bool? CanHitNPC(NPC target) =>
        struck.Count > 0 && !struck.Contains(target.whoAmI) ? false : null;

    protected override void OnStrike(NPC target, in NPC.HitInfo hit, int damageDone)
    {
        if (damageDone <= 0 || target.friendly || target.lifeMax <= 5)
            return;
        struck.Add(target.whoAmI);
        UtageVisuals.Burst(target.Center, Arts ? 1.35f : 1f, 9);
        if (Projectile.owner == Main.myPlayer && target.type != NPCID.TargetDummy && !target.SpawnedFromStatue)
            Wielder.GetModPlayer<UtageCombatPlayer>().HealOnHit();
    }

    protected override void ModifyStrike(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (Arts)
            modifiers.DefenseEffectiveness *= 0f;
    }

    protected override void OnActionEvent(int eventId, Vector2 tipWorld)
    {
        if (Main.dedServ)
            return;
        SoundEngine.PlaySound(SoundID.Item1 with { Volume = .38f, Pitch = Arts ? .16f : -.08f }, tipWorld);
        UtageVisuals.Burst(tipWorld, Arts ? 1.3f : .85f, eventId == 1 ? 4 : 7);
    }

    protected override void DrawWeaponVfxBehind(Color lightColor)
    {
        if (samples < 2)
            return;
        Texture2D body = UtageVisuals.Asset("HellagurSlashBody");
        Texture2D edge = UtageVisuals.Asset("HellagurSlashEdge");
        SpriteEffects flip = FaceLeftPose ? SpriteEffects.FlipVertically : SpriteEffects.None;
        float power = Arts ? 1.35f : 1f;
        // 深紫实体底层，保留刀幕的重量；后续亮边只占小部分。
        for (int i = samples - 1; i >= 0; i -= 3)
        {
            float fade = (1f - i / (float)samples) * bladeLight;
            DrawCrescent(body, hands[i], angles[i], new Color(34, 8, 57) * (fade * .24f),
                new Vector2(1f, .78f) * power, flip);
        }
        BaseHeldMeleeSupport.BeginAdditive(Main.spriteBatch);
        for (int i = samples - 1; i >= 0; i -= 3)
        {
            float fade = MathF.Pow(1f - i / (float)samples, 1.7f) * bladeLight;
            DrawCrescent(body, hands[i], angles[i], UtageVisuals.Violet * (fade * .14f),
                new Vector2(1f, .78f) * power, flip);
            DrawCrescent(edge, hands[i], angles[i], UtageVisuals.Lilac * (fade * .13f),
                new Vector2(1f, .78f) * power, flip);
        }
        BaseHeldMeleeSupport.EndAdditive(Main.spriteBatch);
    }

    private static void DrawCrescent(Texture2D texture, Vector2 hand, float angle, Color color,
        Vector2 scale, SpriteEffects flip)
    {
        Vector2 center = hand + angle.ToRotationVector2() * 55f - Main.screenPosition;
        Main.spriteBatch.Draw(texture, center, null, color, angle - MathHelper.PiOver2,
            texture.Size() * .5f, new Vector2(210f, 145f) / texture.Size() * scale, flip, 0f);
    }

    protected override void DrawWeaponVfx(Color lightColor)
    {
        if (bladeLight < .01f)
            return;
        Texture2D blade = UtageVisuals.Asset("HellagurBladeGlow");
        BaseHeldMeleeSupport.BeginAdditive(Main.spriteBatch);
        Main.spriteBatch.Draw(blade, HandWorld - Main.screenPosition, null,
            UtageVisuals.Violet * (bladeLight * .65f), CurrentAngle, new Vector2(5, 64),
            new Vector2(.98f, .72f), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(blade, HandWorld - Main.screenPosition, null,
            UtageVisuals.Pale * (bladeLight * bladeLight * .32f), CurrentAngle, new Vector2(5, 64),
            new Vector2(.98f, .36f), SpriteEffects.None, 0f);
        BaseHeldMeleeSupport.EndAdditive(Main.spriteBatch);
    }
}

internal static class UtageVisuals
{
    internal static readonly Color Violet = new(157, 49, 236);
    internal static readonly Color Lilac = new(225, 137, 255);
    internal static readonly Color Pale = new(247, 225, 255);
    internal static Texture2D Asset(string name) =>
        ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Guard/Hellagur/" + name).Value;

    internal static void Burst(Vector2 center, float scale, int count)
    {
        if (Main.dedServ || Main.gameMenu)
            return;
        for (int i = 0; i < count; i++)
        {
            Vector2 velocity = Main.rand.NextVector2Circular(4.5f, 3.5f) * scale;
            new DefaultParticle(center, velocity, 12 + i % 6, .45f * scale,
                i % 5 == 0 ? Pale : i % 3 == 0 ? Lilac : Violet, true)
                { Deformation = new Vector2(.32f, 2.1f) }.Spawn();
        }
        for (int i = 0; i < Math.Max(1, count / 4); i++)
        {
            Dust dust = Dust.NewDustPerfect(center, DustID.Shadowflame,
                Main.rand.NextVector2Circular(2.5f, 2.5f), 80, Violet, .75f * scale);
            dust.noGravity = true;
        }
    }
}

public sealed class UtageSkillFlash : ModProjectile
{
    public override string Texture => "Terraria/Images/MagicPixel";
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.timeLeft = 24;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }
    public override bool? CanDamage() => false;
    public override bool ShouldUpdatePosition() => false;
    public override void AI()
    {
        Projectile.Center = Main.player[Projectile.owner].Center;
        if (Projectile.localAI[0]++ == 0)
            UtageVisuals.Burst(Projectile.Center, Projectile.ai[0] == 2 ? 1.35f : 1f, 18);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D ring = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/circle_03").Value;
        float progress = 1f - Projectile.timeLeft / 24f;
        BaseHeldMeleeSupport.BeginAdditive(Main.spriteBatch);
        Main.spriteBatch.Draw(ring, Projectile.Center - Main.screenPosition, null,
            UtageVisuals.Lilac * ((1f - progress) * .4f), 0, ring.Size() * .5f,
            new Vector2(100f + progress * 95f, 45f + progress * 45f) / ring.Size(),
            SpriteEffects.None, 0f);
        BaseHeldMeleeSupport.EndAdditive(Main.spriteBatch);
        return false;
    }
}
