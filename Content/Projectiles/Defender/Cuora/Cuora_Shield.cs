using ArknightsMod.Common.VisualEffects;
using ArknightsMod.Content.Items.Weapons.Defender.Cuora;
using ArknightsMod.Content.Projectiles.Defender.Durnar;
using ArknightsMod.Content.SwingHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
//TODO : 需要制作龟龟二技能开启效果的特效
namespace ArknightsMod.Content.Projectiles.Defender.Cuora;
public class Cuora_Shield: ModProjectile
{
	Player player => Main.player[Projectile.owner];
    Item item => player.HeldItem;

    private Texture2D ShieldTex {
        get {return TextureAssets.Projectile[ModContent.ProjectileType<Cuora_Shield>()].Value;}
    }
    private readonly ShieldHelper shieldHelper = new();
    public override void SetDefaults()
    {
        shieldHelper.SetDefaults(Projectile);
        LoadAssets();
    }
    private ProjMode projMode = ProjMode.Move;
	public override void AI()
    {
        Projectile.damage = item.damage;
        if (player.dead || !player.active || item.type != ModContent.ItemType<CuoraWeapon>()) Projectile.Kill();
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
        Draw_Shield(sb,lightColor);
        shieldHelper.mp.ParryThemeColor = Color.AliceBlue;
        shieldHelper.mp.DrawEffect(TextureAssets.Projectile[Projectile.type].Value);
        sb.End();
        sb.Begin();
        return false;
    }
    public void Move() {
        shieldHelper.UpdateMovePose(Projectile, player);
        if(Main.myPlayer == player.whoAmI)
        {
	        var modPlayer = player.GetModPlayer<CuoraProj_Player>();
            if((Main.mouseRight||modPlayer.DefensiveStance)&&player.itemTime==0)
            {
                projMode = ProjMode.Defender;
            }
        }
    }
    public void Defender()
    {
        if(Main.myPlayer == player.whoAmI)
        {
	        var modPlayer = player.GetModPlayer<CuoraProj_Player>();
	        if (!modPlayer.DefensiveStance) {
		        if(!Main.mouseRight)
		        {
			        projMode = ProjMode.Move;
		        }
	        }
        }
        shieldHelper.UpdateDefenderPose(Projectile, player);
    }

    public void Draw_Shield(SpriteBatch sb,Color lightColor)
    {
        shieldHelper.DrawShield(Projectile, player, ShieldTex, projMode == ProjMode.Defender);
		var mp = player.GetModPlayer<CuoraProj_Player>();
		float hitRad = 0f;
		bool hitting = false;
		if (mp.DefensiveStance) {
			DrawShield(sb, lightColor);
		}

    }
	public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        overPlayers.Add(index);
    }

	private static Texture2D Shield;//高90宽90
	private static Texture2D ShieldLine;//高80宽90
	private int index = 0;
	public void LoadAssets() {
		Shield = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Defender/Cuora/Shield",AssetRequestMode.ImmediateLoad).Value;
		ShieldLine = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Defender/Cuora/ShieldLine",AssetRequestMode.ImmediateLoad).Value;
	}

	private int frameCounter;
	public void DrawShield(SpriteBatch sb,Color lightColor) {
		Color Shieldcolor = lightColor *0.6f;
		SpriteEffects a = player.direction == 1? SpriteEffects.None:SpriteEffects.FlipHorizontally;
		Vector2 pos = player.MountedCenter - Main.screenPosition;
		Lighting.AddLight(player.MountedCenter,lightColor.ToVector3());
		sb.Draw(Shield,new Rectangle((int)pos.X,(int)pos.Y,90,82)
			,new Rectangle(0,index*82,90,82),Shieldcolor,0f,new Vector2(45,41),a,0f);
		sb.Draw(ShieldLine,new Rectangle((int)pos.X,(int)pos.Y,90,82)
			,new Rectangle(0,index*82,90,82),lightColor,0f,new Vector2(45,41),a,0f);
		frameCounter++;
		if (frameCounter >= 4)
		{
			frameCounter = 0;
			index++;
			if (index >= 15) index = 0;
		}
	}

}
