using ArknightsMod.Common.VisualEffects;
using ArknightsMod.Content.Items.Weapons.Defender.Beagle;
using ArknightsMod.Content.Items.Weapons.Defender.Cardigan;
using ArknightsMod.Content.Items.Weapons.Defender.Cuora;
using ArknightsMod.Content.Items.Weapons.Defender.Durnar;
using ArknightsMod.Content.Projectiles.Defender.Cuora;
using ArknightsMod.Content.Projectiles.Defender.Durnar;
using ArknightsMod.Content.SwingHelper;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RuneSKill.Content.NeedTool;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Defender.Caridgan;

public class Cardi_Shield : ModProjectile
{
	Player player => Main.player[Projectile.owner];
    Item item => player.HeldItem;

    private Texture2D ShieldTex => TextureAssets.Projectile[ModContent.ProjectileType<Cardi_Shield>()].Value;
    private readonly ShieldHelper shieldHelper = new();
    public override void SetDefaults()
    {
        shieldHelper.SetDefaults(Projectile);
    }
    private ProjMode projMode = ProjMode.Move;
    private Defender_Player mp;
	public override void AI()
    {
        Projectile.damage = item.damage;
        if (player.dead || !player.active ||
            item.type != ModContent.ItemType<CardiWeapon>() )
	        Projectile.Kill();
        Projectile.timeLeft = 2;
        switch(projMode)
        {
            case ProjMode.Move:
            Move();
            break;
            case ProjMode.Defender:
            Defender();
            break;
        }

    }

    public override bool? CanDamage()
    {
        return false;
    }


	public override bool PreDraw(ref Color lightColor)
    {
        SpriteBatch sb = Main.spriteBatch;
        sb.End();
        sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
            SamplerState.AnisotropicClamp, DepthStencilState.None,
            RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        Draw_Shield(sb);
        shieldHelper.mp.DrawEffect(TextureAssets.Projectile[Projectile.type].Value);
        sb.End();
        sb.Begin();
        return false;
    }
    public void Move() {
        shieldHelper.UpdateMovePose(Projectile, player);

        if(Main.myPlayer == player.whoAmI)
        {
	        var modPlayer = player.GetModPlayer<CardiProj_Player>();
            if(Main.mouseRight&&player.itemTime==0)
            {
                projMode = ProjMode.Defender;
            }
        }
    }
    public void Defender()
    {
        if(Main.myPlayer == player.whoAmI)
        {
            if(!Main.mouseRight)
            {
                projMode = ProjMode.Move;
            }
        }
        shieldHelper.UpdateDefenderPose(Projectile, player);
    }

    public void Draw_Shield(SpriteBatch sb)
    {
        shieldHelper.DrawShield(Projectile, player, ShieldTex, projMode == ProjMode.Defender);
    }
	public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        overPlayers.Add(index);
    }
}
