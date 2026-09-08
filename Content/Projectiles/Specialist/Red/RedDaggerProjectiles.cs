using System;
using ArknightsMod.Common.Particle;
using ArknightsMod.Content.Projectiles.Guard.Hellagur;
using ArknightsMod.Content.Buffs;
using ArknightsMod.Content.Items.Weapons.Specialist.Red;
using ArknightsMod.Content.Projectiles.BasePROJ;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Specialist.Red;

// 参考本地 Calamity MantisClawHoldout 的“持有体定时生成短命斩痕”结构；无灾厄代码/资源依赖。
public sealed class RedDaggerHoldout : ModProjectile
{
    private float shotTimer;
    private int age;
    private int swipe;
    private float frontAngle;
    private float backAngle;
    private Player Owner => Main.player[Projectile.owner];
    public override string Texture => "Terraria/Images/Item_" + ItemID.PsychoKnife;
    internal static float SlashInterval(float attackSpeed) =>
        Math.Clamp((int)MathF.Round(6f / Math.Max(.1f, attackSpeed)), 2, 60) / RedDagger.AttackSpeedBonus;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.netImportant = true;
        Projectile.timeLeft = 2;
    }
    public override bool? CanDamage() => false;
    public override bool ShouldUpdatePosition() => false;
    public override void AI()
    {
        if (!Owner.active || Owner.dead || Owner.CCed || Owner.noItems
            || Owner.HeldItem.ModItem is not RedDagger
            || (Projectile.owner == Main.myPlayer && !Owner.channel))
        {
            Projectile.Kill();
            return;
        }
        Projectile.Center = Owner.MountedCenter;
        Projectile.timeLeft = 2;
        if (Projectile.owner == Main.myPlayer)
        {
            Vector2 aim = (Main.MouseWorld - Owner.MountedCenter).SafeNormalize(new Vector2(Owner.direction, 0));
            if (Vector2.DistanceSquared(aim, Projectile.velocity) > .008f || age % 6 == 0)
                Projectile.netUpdate = true;
            Projectile.velocity = aim;
            if (shotTimer <= 0f)
            {
                var combat = Owner.GetModPlayer<RedDaggerPlayer>();
                // 累加间隔保留上次剩余的小数，使加速不会因整数帧取整而丢失或变成 20%。
                shotTimer += SlashInterval(Owner.GetTotalAttackSpeed(DamageClass.Melee) * combat.AttackSpeedMultiplier);
                float direction = (++swipe & 1) == 0 ? -1f : 1f;
                float angle = aim.ToRotation() + Main.rand.NextFloat(-.4363f, .4363f);
                float scale = Math.Clamp(Owner.GetAdjustedItemScale(Owner.HeldItem), .7f, 1.3f);
                int index = Projectile.NewProjectile(Projectile.GetSource_FromThis(),
                    Owner.MountedCenter + aim * 12f, angle.ToRotationVector2() * 4.5f,
                    ModContent.ProjectileType<RedDaggerSlash>(), Owner.GetWeaponDamage(Owner.HeldItem),
                    Owner.HeldItem.knockBack, Projectile.owner, angle,
                    direction * (combat.DeploymentActive ? 1.25f : 1f), scale);
                if (Main.projectile.IndexInRange(index))
                    Main.projectile[index].CritChance = Owner.GetWeaponCrit(Owner.HeldItem);
            }
            shotTimer -= 1f;
        }
        Vector2 directionVector = Projectile.velocity.SafeNormalize(new Vector2(Owner.direction, 0));
        float rotation = directionVector.ToRotation();
        Owner.ChangeDir(directionVector.X < 0 ? -1 : 1);
        Owner.heldProj = Projectile.whoAmI;
        Owner.itemTime = Owner.itemAnimation = 2;
        Owner.itemRotation = rotation + (Owner.direction < 0 ? MathHelper.Pi : 0);
        float swing = MathF.Sin(age * MathHelper.Pi / SlashInterval(Owner.GetTotalAttackSpeed(DamageClass.Melee)
            * Owner.GetModPlayer<RedDaggerPlayer>().AttackSpeedMultiplier)) * .9f;
        frontAngle = rotation - MathHelper.PiOver2 + swing;
        backAngle = rotation - MathHelper.PiOver2 - swing;
        Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, frontAngle);
        Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, backAngle);
        age++;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D knife = TextureAssets.Item[ItemID.PsychoKnife].Value;
        DrawKnife(knife, Owner.GetBackHandPosition(Player.CompositeArmStretchAmount.Full, backAngle),
            backAngle, lightColor * .7f);
        DrawKnife(knife, Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, frontAngle),
            frontAngle, lightColor);
        return false;
    }
    private static void DrawKnife(Texture2D knife, Vector2 hand, float arm, Color color)
    {
        Main.spriteBatch.Draw(knife, hand - Main.screenPosition, null, color,
            arm + MathHelper.PiOver2 + MathHelper.PiOver4, new Vector2(2, knife.Height - 2),
            .62f, SpriteEffects.None, 0);
    }
}

public sealed class RedDaggerSlash : ModProjectile
{
    internal const int Lifetime = 20;
    private Player Owner => Main.player[Projectile.owner];
    private int Age => Lifetime - Projectile.timeLeft;
    private float Size => Math.Clamp(Projectile.ai[2], .7f, 1.3f);
    private bool Empowered => MathF.Abs(Projectile.ai[1]) > 1.1f;
    public override string Texture => "ArknightsMod/Content/Projectiles/Guard/Hellagur/HellagurSlashBody";
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 100;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.timeLeft = Lifetime;
    }
    public override bool CanHitPvp(Player target) => false;
    public override bool? CanCutTiles() => false;
    public override bool? CanHitNPC(NPC target) => target.CanBeChasedBy(Projectile) ? null : false;
    public override bool? CanDamage() => Age >= 1 && Age <= 10 ? null : false;
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        Vector2 point = Projectile.Center + Projectile.ai[0].ToRotationVector2() * 16f;
        return RedDaggerVisuals.CircleHits(point, 32f * Size, targetHitbox)
            && Collision.CanHitLine(Owner.MountedCenter, 1, 1,
                targetHitbox.TopLeft(), targetHitbox.Width, targetHitbox.Height);
    }
    public override void AI()
    {
        if (!Owner.active || Owner.dead || Owner.HeldItem.ModItem is not RedDagger
            || Vector2.DistanceSquared(Owner.MountedCenter, Projectile.Center) > 140f * 140f)
        {
            Projectile.Kill();
            return;
        }
        Projectile.velocity = (Projectile.velocity * .9f).RotatedBy(.05236f * MathF.Sign(Projectile.ai[1]));
        if (Age == 0 && !Main.dedServ)
            SoundEngine.PlaySound(SoundID.Item1 with { Volume = .24f, Pitch = .45f,
                PitchVariance = .16f, MaxInstances = 4 }, Projectile.Center);
        Lighting.AddLight(Projectile.Center, new Vector3(.36f, .02f, .04f) * (1f - Age / 20f));
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (damageDone > 0)
            RedDaggerVisuals.Hit(target.Center, Empowered);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        float p = Math.Clamp(Age / (float)Lifetime, 0, 1);
        float envelope = MathF.Sin(p * MathHelper.Pi);
        float angle = Projectile.ai[0] + MathF.Sign(Projectile.ai[1]) * (-1.2f + (1f - MathF.Pow(1f - p, 3f)) * 4.3f);
        RedDaggerVisuals.DrawSlash(Projectile.Center, angle, Size * (Empowered ? 1.25f : 1f),
            envelope, Projectile.ai[1] < 0);
        return false;
    }
}

public sealed class RedWolfPulse : ModProjectile
{
    private int Age => 24 - Projectile.timeLeft;
    public override string Texture => ArknightsMod.noTexture;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 224;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.timeLeft = 24;
    }
    public override bool ShouldUpdatePosition() => false;
    public override bool CanHitPvp(Player target) => false;
    public override bool? CanCutTiles() => false;
    public override bool? CanDamage() => Age >= 2 && Age <= 4 ? null : false;
    public override bool? CanHitNPC(NPC target) => target.CanBeChasedBy(Projectile) ? null : false;
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) =>
        RedDaggerVisuals.CircleHits(Projectile.Center, 112f, targetHitbox)
        && Collision.CanHitLine(Projectile.Center, 1, 1, targetHitbox.TopLeft(), targetHitbox.Width, targetHitbox.Height);
    public override void AI()
    {
        if (Age == 2 && Projectile.ai[0] > 0 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            // 服务器在同一爆发窗口施加控制；不依赖客户端 OnHit 回调去调用服务器限定的眩晕入口。
            foreach (NPC npc in Main.ActiveNPCs)
                if (npc.CanBeChasedBy(Projectile) && !npc.boss && npc.realLife < 0
                    && Colliding(Projectile.Hitbox, npc.Hitbox) == true)
                    OperatorStunNPC.TryApply(npc, Math.Clamp((int)Projectile.ai[0], 60, 180));
        }
        if (Age == 2)
            HellagurOdachiSwing.SpawnHitParticles(Projectile.Center, true,
                new Vector2(Main.player[Projectile.owner].direction, 0f));
    }
    // 两个技能的再部署爆发统一只生成短粒子，不再绘制整张像素纹理或旋转刀幕。
    public override bool PreDraw(ref Color lightColor) => false;

}

internal static class RedDaggerVisuals
{
    internal static bool CircleHits(Vector2 center, float radius, Rectangle box)
    {
        Vector2 closest = new(Math.Clamp(center.X, box.Left, box.Right), Math.Clamp(center.Y, box.Top, box.Bottom));
        return Vector2.DistanceSquared(center, closest) <= radius * radius;
    }
    internal static void DrawSlash(Vector2 center, float angle, float scale, float fade, bool reverse)
    {
        Texture2D body = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Guard/Hellagur/HellagurSlashBody").Value;
        Texture2D edge = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Guard/Hellagur/HellagurSlashEdge").Value;
        SpriteEffects flip = reverse ? SpriteEffects.FlipVertically : SpriteEffects.None;
        Vector2 position = center - Main.screenPosition;
        Main.spriteBatch.Draw(body, position, null, new Color(27, 3, 10) * (fade * .62f), angle,
            body.Size() * .5f, new Vector2(112, 78) / body.Size() * scale, flip, 0);
        BaseHeldMeleeSupport.BeginAdditive(Main.spriteBatch);
        for (int layer = 0; layer < 3; layer++)
        {
            float size = .72f + layer * .14f;
            Color color = layer == 0 ? new Color(255, 216, 224) : new Color(230, 30, 64);
            Main.spriteBatch.Draw(layer == 0 ? edge : body, position, null, color * (fade * (layer == 0 ? .28f : .38f)),
                angle, (layer == 0 ? edge : body).Size() * .5f,
                new Vector2(112, 78) / (layer == 0 ? edge : body).Size() * (size * scale), flip, 0);
        }
        BaseHeldMeleeSupport.EndAdditive(Main.spriteBatch);
    }
    internal static void Hit(Vector2 point, bool empowered)
    {
        if (Main.dedServ || Main.gameMenu)
            return;
        for (int i = 0; i < (empowered ? 8 : 5); i++)
            new DefaultParticle(point, Main.rand.NextVector2Circular(4f, 3f), 10 + i % 4, .32f,
                i % 4 == 0 ? new Color(255, 216, 224) : new Color(210, 25, 57), true)
                { Deformation = new Vector2(.25f, 2f) }.Spawn();
        Dust dust = Dust.NewDustPerfect(point, DustID.Blood, Main.rand.NextVector2Circular(2, 2), Scale: .7f);
        dust.noGravity = true;
    }
}
