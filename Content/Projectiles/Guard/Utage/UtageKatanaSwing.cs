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
// 共用赫拉格的着色器刀幕与亮度层次，保留宴自己的技能、回血和紫色配色。
public sealed class UtageKatanaSwing : HellagurOdachiSwing
{
    private readonly HashSet<int> struck = new();
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

    protected override bool ShowMoonPhaseVfx => false;

    protected override Color BladeVfxColor(Color color)
    {
        // 保留赫拉格每层的透明度和明暗变化：暗部深紫，热刃淡紫，最亮处接近白色。
        float value = Math.Max(color.R, Math.Max(color.G, color.B)) / 255f;
        float highlight = color.R > 0 ? MathHelper.Clamp(color.G / (float)color.R, 0f, 1f) : 0f;
        Color violet = Color.Lerp(new Color(148, 30, 255), UtageVisuals.Pale, highlight * highlight);
        return new Color((byte)(violet.R * value), (byte)(violet.G * value),
            (byte)(violet.B * value), color.A);
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
