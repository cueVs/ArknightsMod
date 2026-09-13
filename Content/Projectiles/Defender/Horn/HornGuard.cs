using System.IO;
using System;
using System.Collections.Generic;
using ArknightsMod.Content.Items.Weapons.Defender.Horn;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Defender.Horn;

public sealed class HornGuard : ModProjectile
{
    private byte visualState;
    public override void SendExtraAI(BinaryWriter writer) => writer.Write(visualState);
    public override void ReceiveExtraAI(BinaryReader reader) => visualState = reader.ReadByte();
    public override string Texture => "Terraria/Images/Item_" + ItemID.GrenadeLauncher;
    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 24;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 2;
        Projectile.hide = true;
    }
    public override bool? CanDamage() => false;
    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead || owner.HeldItem.ModItem is not HornGrenadeLauncher || owner.CCed || owner.noItems)
        { Projectile.Kill(); return; }
        Projectile.timeLeft = 2;
        if (Projectile.owner == Main.myPlayer)
        {
            byte nextState = owner.GetModPlayer<HornLauncherPlayer>().VisualState;
            if (nextState != visualState) { visualState = nextState; Projectile.netUpdate = true; }
            float angle = (Main.MouseWorld - owner.MountedCenter).SafeNormalize(new Vector2(owner.direction, 0)).ToRotation();
            float guard = owner.GetModPlayer<Defender_Player>().ShieldMode ? 1f : 0f;
            if (MathF.Abs(MathHelper.WrapAngle(angle - Projectile.ai[0])) > .04f || guard != Projectile.ai[1])
            { Projectile.ai[0] = angle; Projectile.ai[1] = guard; Projectile.netUpdate = true; }
        }
        Vector2 aim = Projectile.ai[0].ToRotationVector2();
        owner.ChangeDir(aim.X >= 0 ? 1 : -1);
        owner.heldProj = Projectile.whoAmI;
        Projectile.Center = owner.MountedCenter + new Vector2(owner.direction * (Projectile.ai[1] == 1 ? 24f : 15f), 1f);
        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.ai[0] - MathHelper.PiOver2);
        if (Projectile.ai[2] > 0) Projectile.ai[2]--;
        HornVisuals.SkillBody(owner, visualState, ++Projectile.localAI[0]);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Player owner = Main.player[Projectile.owner];
        HornVisuals.SkillBodyGlow(owner, visualState, Projectile.localAI[0]);
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Vector2 aim = Projectile.ai[0].ToRotationVector2();
        Vector2 gunCenter = owner.MountedCenter + aim * (19f - Projectile.ai[2] * .5f);
        // The vanilla launcher points right at rotation zero. Vertical flipping keeps the grip below it when aiming left.
        Main.EntitySpriteDraw(texture, gunCenter - Main.screenPosition, null, lightColor, Projectile.ai[0],
            texture.Size() * .5f, 1f, owner.direction < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None);
        if (Projectile.ai[1] == 1)
        {
            Texture2D shield = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Defender/Cuora/Cuora_Shield").Value;
            Main.EntitySpriteDraw(shield, Projectile.Center - Main.screenPosition, null, lightColor, 0f,
                shield.Size() * .5f, 1.25f, owner.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
        }
        return false;
    }
    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,
        List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overPlayers.Add(index);
}
