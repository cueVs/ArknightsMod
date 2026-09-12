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
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = 1;
        Projectile.extraUpdates = 3;
        Projectile.timeLeft = 180 * 4;
        Projectile.netImportant = true;
    }
    private bool Attacking => Projectile.ai[0] == -2f && Projectile.damage > 0;
    public override bool? CanDamage() => Attacking ? null : false;
    public override bool CanHitPvp(Player target) => false;
    public override bool? CanHitNPC(NPC target) => Attacking && target.CanBeChasedBy(Projectile) ? null : false;
    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        if (owner.HeldItem.ModItem is not SussurroStaff || !owner.active || owner.dead)
        {
            Projectile.Kill();
            return;
        }
        // 目标模式仅由发射者决定并同步；满血也发射，在飞行中随队伍血量重新选择用途。
        if (Projectile.owner == Main.myPlayer && Projectile.numUpdates == 0 && Projectile.damage > 0)
        {
            int next = (int)Projectile.ai[0];
            if (SussurroStaffPlayer.AllPlayersHealthy())
                next = -2;
            else if (!Main.player.IndexInRange(next) || !SussurroStaffPlayer.CanTreat(owner, Main.player[next])
                || Main.player[next].statLife >= Main.player[next].statLifeMax2)
                next = SussurroStaffPlayer.FindPatient(owner, Projectile.Center);
            if (next != (int)Projectile.ai[0])
            {
                Projectile.ai[0] = next;
                Projectile.netUpdate = true;
            }
        }
        Projectile.friendly = Attacking;
        Projectile.localAI[0] += 1f / Projectile.MaxUpdates;
        float age = Projectile.localAI[0];
        if (!Main.dedServ && Projectile.numUpdates == 0 && (int)age % 3 == 0)
            SussurroVisuals.Firefly(Projectile.Center, -Projectile.velocity * .12f, .65f);

        int targetIndex = (int)Projectile.ai[0];
        Player target = Main.player.IndexInRange(targetIndex) ? Main.player[targetIndex] : null;
        if (Attacking)
        {
            NPC enemy = null;
            float best = 1200f * 1200f;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                float distance = npc.DistanceSQ(Projectile.Center);
                if (distance < best && npc.CanBeChasedBy(Projectile))
                {
                    best = distance;
                    enemy = npc;
                }
            }
            if (enemy != null)
                Projectile.velocity = Vector2.Lerp(Projectile.velocity,
                    (enemy.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * 6f, .06f);
            return;
        }
        if (target == null || !SussurroStaffPlayer.CanTreat(owner, target))
        {
            // 仍有缺血玩家但不在治疗范围内时暂留空中，不能误判为全员满血。
            Projectile.velocity *= .98f;
            return;
        }
        Vector2 toTarget = target.MountedCenter - Projectile.Center;
        // LivingShard 蝴蝶先舒展开翅膀，再以柔和惯性靠近；近身治疗不必等命中敌人。
        if (age < 3f)
            Projectile.velocity *= .98f;
        else
        {
            float speed = MathHelper.Clamp(5f + toTarget.Length() * .018f + age * .04f, 5f, 16f);
            // 右键蝴蝶有伤害基数；左键被动治疗蝴蝶仍使用原有速度与追踪。
            float movementScale = Projectile.damage > 0 ? .5f : 1f;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity,
                toTarget.SafeNormalize(Vector2.UnitY) * speed * movementScale, .16f * movementScale);
        }
        if (age >= 3f && toTarget.Length() < 24f && Projectile.owner == Main.myPlayer)
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
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        => SussurroVisuals.Impact(target.Center, Projectile.velocity.ToRotation(), .85f);
    public override bool PreDraw(ref Color lightColor)
    {
        // 蝴蝶绘制的头部朝上，补 90 度后与实际飞行向量一致。
        SussurroVisuals.DrawButterfly(Projectile.Center, Projectile.velocity.ToRotation() + MathHelper.PiOver2,
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
