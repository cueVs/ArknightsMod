using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Sniper.Wisadel
{
	public class WisdelShotBigCircle : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;
		public override void SetStaticDefaults()
		{
		}
        public Color color = Color.White;
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteRGB(color);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            color = reader.ReadRGB();
        }
        public int timeLeft = 20;
        private Effect trail = ModContent.Request<Effect>("ArknightsMod/Content/Projectiles/Sniper/Wisadel/Wisdel",
            AssetRequestMode.ImmediateLoad).Value;
        public override void SetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.scale = 1f;
			Projectile.alpha = 255;
			Projectile.timeLeft = timeLeft;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
		}
        public override bool ShouldUpdatePosition()
        {
            return false;
        }
        public override bool? CanDamage()
        {
            return false;
        }
        private bool syncPos;
        private Vector2 relPos;
        public override void AI()
		{
            Player player = Main.player[Projectile.owner];
            if (!syncPos)
            {
                relPos = Projectile.position - player.position;
                syncPos = true;
            }
            Projectile.position = player.position + relPos;
            Projectile.rotation = Projectile.velocity.ToRotation() + 1.57079637f;

        }
        public override bool PreDraw(ref Color lightColor)
        {
            float prog = (float)Projectile.timeLeft / timeLeft;
            prog = MathHelper.Clamp(prog, 0, 1);

            VertexStrip.StripColorFunction colorFunction = (prog) =>
            {
                return color;
            };

            VertexStrip.StripHalfWidthFunction widthFunction = (prog) =>
            {
                return 8f * MathHelper.Lerp(1f, 0.5f, prog / 1.5f);
            };

            // 椭圆参数
            float a = Projectile.ai[0]; // 水平半径
            float b = Projectile.ai[1]; // 垂直半径
            Vector2 center = Projectile.Center;

            // 椭圆旋转角度（可动态调整，添加动画）
            float rotationAngle = Projectile.rotation; // 每帧增加一点角度，制造旋转效果

            // 椭圆轨迹上的点
            int pointCount = 30;
            var ellipsePos = new Vector2[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                float theta = MathHelper.TwoPi * i / (pointCount - 1); // 均匀分布角度
                float x = a * (float)Math.Cos(theta);
                float y = b * (float)Math.Sin(theta);

                // 应用旋转变换
                float rotatedX = x * (float)Math.Cos(rotationAngle) - y * (float)Math.Sin(rotationAngle);
                float rotatedY = x * (float)Math.Sin(rotationAngle) + y * (float)Math.Cos(rotationAngle);

                ellipsePos[i] = center + new Vector2(rotatedX, rotatedY);
            }

            // 计算旋转方向
            var rotations = ellipsePos.Zip(ellipsePos.Skip(1), (a, b) => a - b).Select((a) => a.ToRotation());

            Main.graphics.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Sniper/Wisadel/Trail1_NonPrem").Value;
            var strip = new VertexStrip();
            strip.PrepareStrip(ellipsePos, rotations.Prepend(rotations.FirstOrDefault()).ToArray(),
                colorFunction, widthFunction, -Main.screenPosition);

            if (color != Color.Black)
            {
                Main.graphics.GraphicsDevice.BlendState = BlendState.Additive;
            }
            Main.graphics.GraphicsDevice.Textures[1] = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Sniper/Wisadel/noise26").Value;
            trail.Parameters["uOpacity"].SetValue(prog * 0.8f);
            trail.Parameters["uColor"].SetValue(new Vector3(color.R, color.G, color.B));
            trail.Parameters["uSecondaryColor"].SetValue(new Vector3(color.R, color.G, color.B) / 255f);
            trail.CurrentTechnique.Passes[0].Apply();
            strip.DrawTrail();

            return false;
        }
    }
}
