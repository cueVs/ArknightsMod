using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Color = Microsoft.Xna.Framework.Color;

namespace ArknightsMod.Content.Projectiles
{
    public class TYTX : ModProjectile
    {
        //public override string Texture => "ArknightsWeaponryExpansion/Content/Items/YinHui2";
        public override void SetDefaults()
        {
            Projectile.extraUpdates = 1;
            Projectile.width = 40;// 弹幕判定体积的宽(碰撞箱)
            Projectile.height = 40;//弹幕判定体积的高
            Projectile.scale = 1f;//放大弹幕
            Projectile.friendly = false;//是否对敌对NPC造成伤害
            Projectile.DamageType = DamageClass.Melee;//伤害类型
            Projectile.ignoreWater = true;//弹幕在水里的时候会不会减速 
            Projectile.tileCollide = false;//弹幕会不会穿墙
            Projectile.timeLeft = 20;//消散时间60=1秒
            Projectile.penetrate = -1;//弹幕打中几个怪物之后会消失
            Projectile.alpha = 255;//弹幕的透明度
            Projectile.light = 0.5f;//弹幕光照的强度
        }
        int jsq = 0;
        
        public override void OnSpawn(IEntitySource source)
        {
            jsq = Projectile.timeLeft;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0;
        }
        public override void AI()
        {
            
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,//这里是用来改弹幕图层的
        List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overWiresUI.Add(index);
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D 贴图 = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/ex24").Value;
            int d = (int)(200f / jsq * Projectile.timeLeft);
            Main.spriteBatch.Draw(贴图, Projectile.Center - Main.screenPosition - Projectile.velocity.SafeNormalize(Vector2.Zero) * 2f,
        null, new Color(d + (int)Projectile.ai[0], d + (int)Projectile.ai[1], d + (int)Projectile.ai[2], d / 4 + 200), Projectile.rotation, 贴图.Size() / 2f,
        new Vector2(1.5f / jsq * Projectile.timeLeft, 2.5f) / 1f
        , SpriteEffects.None, 0);//绘制

            Main.spriteBatch.Draw(贴图, Projectile.Center - Main.screenPosition - Projectile.velocity.SafeNormalize(Vector2.Zero) * 2f,
             null, new Color(d + (int)Projectile.ai[0], d + (int)Projectile.ai[1], d + (int)Projectile.ai[2], 180), Projectile.rotation, 贴图.Size() / 2f,
             new Vector2(1.5f / jsq * Projectile.timeLeft, 2.5f) / 1.5f
             , SpriteEffects.None, 0);//绘制
            return false;
        }
    }
}