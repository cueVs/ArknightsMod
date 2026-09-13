using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content.Projectiles.Rogue.FireworksHand;

namespace ArknightsMod.Content.Projectiles.Rogue.FireworksHand
{

    public class FireworksHandProj : ModProjectile
    {

        private const int TRAIL_LENGTH = 16;
        private const float TRAIL_WIDTH_START_DEFAULT = 15f;
        private const float TRAIL_WIDTH_END_DEFAULT = 7f;
        private readonly Color TRAIL_COLOR = new Color(255, 220, 80);  // 亮黄色
        public static float TrailFlowSpeed = 1.4f;      // 纹理滚动速度
        public static float TrailFadePower = 1.7f;      // 褪色曲线强度（越大尾部越淡）
        // 追踪参数
        private const float HOMING_STRENGTH = 0.12f;
        private const float MAX_SPEED = 26f;
        private const float HOMING_RANGE = 300f;

        // 配置选项
        public static float ProjectileSize = 1.2f;          // 弹幕大小
        public static float Brightness = 1.7f;               // 亮度系数（1 = 原亮度）,没什么效果
        public static float TrailWidthStart = TRAIL_WIDTH_START_DEFAULT; // 拖尾起始宽度
        public static float TrailWidthEnd = TRAIL_WIDTH_END_DEFAULT;     // 拖尾结束宽度

        // 拖尾点队列
        private Queue<Vector2> trailPositions = new Queue<Vector2>();
        private Color projectileColor = Color.Yellow;

        // 纹理尺寸缓存
        private Texture2D projectileTexture;
        private Vector2 textureSize;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 0;
            Projectile.velocity = new Vector2(8f, 0f);

            // 获取纹理信息
            projectileTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Rogue/FireworksHand/Projectileslight").Value;
            if (projectileTexture != null && !projectileTexture.IsDisposed)
            {
                textureSize = new Vector2(projectileTexture.Width, projectileTexture.Height);
            }
        }

        public override void AI()
        {
            HomingBehavior();
            UpdateTrail();
        }

        private void HomingBehavior()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            NPC target = null;
            float closestDist = HOMING_RANGE;
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        target = npc;
                    }
                }
            }

            if (target != null)
            {
                Vector2 toTarget = target.Center - Projectile.Center;
                float distance = toTarget.Length();
                if (distance < 5f) distance = 5f;


                float dynamicStrength = MathHelper.Lerp(0.35f, 0.12f, MathHelper.Clamp(distance / HOMING_RANGE, 0f, 1f));

                Vector2 direction = toTarget / distance;

                float desiredSpeed = MathHelper.Lerp(MAX_SPEED * 1.2f, MAX_SPEED * 0.8f, MathHelper.Clamp(distance / HOMING_RANGE, 0f, 1f));
                Vector2 desiredVelocity = direction * desiredSpeed;


                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, dynamicStrength);


                Projectile.velocity = Projectile.velocity.RotatedByRandom(0.02f * (1f - dynamicStrength));


                if (Projectile.velocity.Length() < 2f)
                    Projectile.velocity = direction * 2f;
            }
            else
            {

                if (Projectile.velocity.Length() < MAX_SPEED)
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, Utils.SafeNormalize(Projectile.velocity, Vector2.UnitX) * MAX_SPEED, 0.05f);
            }


            if (Projectile.velocity.Length() > MAX_SPEED * 1.2f)
                Projectile.velocity = Utils.SafeNormalize(Projectile.velocity, Vector2.Zero) * MAX_SPEED * 1.2f;
            else if (Projectile.velocity.Length() > MAX_SPEED)
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Utils.SafeNormalize(Projectile.velocity, Vector2.Zero) * MAX_SPEED, 0.1f);
        }

        private void UpdateTrail()
        {
            trailPositions.Enqueue(Projectile.Center);
            while (trailPositions.Count > TRAIL_LENGTH)
                trailPositions.Dequeue();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.owner != Main.myPlayer)
                return;

            CreateExplosion(Projectile.Center);
            CreateExplosionGlow(Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item111, Projectile.position);

            Vector2 offset = Main.rand.NextVector2Circular(1f, 1f);
            CreateExplosion(Projectile.Center + offset);
        }
		private void CreateExplosionGlow(Vector2 center) {
			Projectile.NewProjectile(
				Projectile.GetSource_FromThis(),
				center,
				Vector2.Zero,
				ModContent.ProjectileType<FireworksHandProjGlow>(),
				0,
				0f,
				Projectile.owner,
        ai0: Projectile.scale  // 使用 Projectile.scale
			);
		}
		private void CreateExplosion(Vector2 center)
        {
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                center,
                Vector2.Zero,
                ModContent.ProjectileType<Fireworks_Hand_Proj_Explosive>(),
                0,
                0f,
                Projectile.owner
            );
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            DrawTrail();
            DrawProjectile();
        }

        private void DrawProjectile()
        {
            if (projectileTexture == null || projectileTexture.IsDisposed) return;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            GraphicsDevice gd = Main.graphics.GraphicsDevice;
            DrawProjectileVertices(gd, projectileTexture);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawProjectileVertices(GraphicsDevice gd, Texture2D texture)
        {
            float alphaFactor = (float)Projectile.timeLeft / 300f;

            Color drawColor = TRAIL_COLOR * alphaFactor * Brightness;

            float textureAspect = textureSize.X / textureSize.Y;
            float baseSize = Projectile.width * Projectile.scale * ProjectileSize;
            float width = baseSize * textureAspect;
            float height = baseSize;

            Vector2 center = Projectile.Center - Main.screenPosition;
            float rot = Projectile.rotation;

            Vector2 offset = new Vector2(width / 2f + 6f, 0).RotatedBy(rot);
            Vector2 adjustedCenter = center - offset;

            Vector2 topLeft = adjustedCenter + new Vector2(-width / 2f, -height / 2f).RotatedBy(rot);
            Vector2 topRight = adjustedCenter + new Vector2(width / 2f, -height / 2f).RotatedBy(rot);
            Vector2 bottomLeft = adjustedCenter + new Vector2(-width / 2f, height / 2f).RotatedBy(rot);
            Vector2 bottomRight = adjustedCenter + new Vector2(width / 2f, height / 2f).RotatedBy(rot);

            VertexData[] vertices = new VertexData[]
            {
                new VertexData(topLeft, new Vector3(0, 0, 1), drawColor),
                new VertexData(topRight, new Vector3(1, 0, 1), drawColor),
                new VertexData(bottomLeft, new Vector3(0, 1, 1), drawColor),
                new VertexData(bottomRight, new Vector3(1, 1, 1), drawColor)
            };

            gd.Textures[0] = texture;
            gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, 2);
        }

        private void DrawTrail()
        {
            if (trailPositions.Count < 2) return;

            Texture2D gradientTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Rogue/FireworksHand/LightningGradient").Value;
            if (gradientTexture == null || gradientTexture.IsDisposed) return;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearWrap, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            GraphicsDevice gd = Main.graphics.GraphicsDevice;
            DrawTrailVertices(gd, gradientTexture);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawTrailVertices(GraphicsDevice gd, Texture2D texture)
        {
            Vector2[] points = trailPositions.ToArray();
            int count = points.Length;

            float[] cumLength = new float[count];
            cumLength[0] = 0f;
            float totalLength = 0f;
            for (int i = 1; i < count; i++)
            {
                cumLength[i] = cumLength[i - 1] + Vector2.Distance(points[i], points[i - 1]);
                totalLength = cumLength[i];
            }

            // 动态纹理偏移量（随时间滚动）
            float timeOffset = (float)Main.timeForVisualEffects * 0.02f * TrailFlowSpeed;
            // 取小数部分，避免过大
            timeOffset = timeOffset - (float)Math.Floor(timeOffset);

            List<VertexData> vertices = new List<VertexData>();

            for (int i = 0; i < count; i++)
            {
                Vector2 dir;
                if (i == 0)
                    dir = points[i + 1] - points[i];
                else if (i == count - 1)
                    dir = points[i] - points[i - 1];
                else
                    dir = points[i + 1] - points[i - 1];

                if (dir.LengthSquared() < 0.001f)
                    dir = Vector2.UnitX;
                else
                    dir.Normalize();

                Vector2 perp = new Vector2(-dir.Y, dir.X);
                float t = (float)i / (count - 1); // t=0 尾部，t=1 头部

    
                float alpha = (float)Math.Pow(t, TrailFadePower);
          
                alpha = MathHelper.Clamp(alpha, 0f, 1f);

                float width = MathHelper.Lerp(TrailWidthEnd, TrailWidthStart, t);

                Vector2 left = points[i] - perp * width;
                Vector2 right = points[i] + perp * width;

                // 纹理 V 坐标
                float v = totalLength > 0 ? cumLength[i] / totalLength : 0f;
                v = (v + timeOffset) % 1f;

                Vector2 leftScreen = left - Main.screenPosition;
                Vector2 rightScreen = right - Main.screenPosition;

                Color trailColor = TRAIL_COLOR * alpha * Brightness;
                vertices.Add(new VertexData(leftScreen, new Vector3(0, v, 1), trailColor));
                vertices.Add(new VertexData(rightScreen, new Vector3(1, v, 1), trailColor));
            }

            if (vertices.Count >= 4)
            {
                gd.Textures[0] = texture;
                gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);
            }
        }
    }
}
