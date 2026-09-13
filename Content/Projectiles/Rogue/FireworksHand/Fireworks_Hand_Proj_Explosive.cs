using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace  ArknightsMod.Content.Projectiles.Rogue.FireworksHand
{

    public class Fireworks_Hand_Proj_Explosive : ModProjectile
    {
        public override string Texture => "ArknightsMod/Content/Projectiles/Rogue/FireworksHand/NoiseTexture";

       
        private const int MAX_LIFE = 28;                    // 总寿命（帧数）
        private const int DEBRIS_COUNT = 12;                // 碎屑数量
        private const int MIN_VERTICES = 14;                // 最少顶点数（偶数）
        private const int MAX_VERTICES = 24;                // 最多顶点数（偶数）

        // 尺寸参数
        private const float MAX_RADIUS = 64f;                // 最大半径
        private const float DEBRIS_MIN_SIZE = 2.5f;          // 碎屑最小尺寸
        private const float DEBRIS_MAX_SIZE = 5f;            // 碎屑最大尺寸
        private const float DEBRIS_MIN_SPEED = 4f;           // 碎屑最小速度
        private const float DEBRIS_MAX_SPEED = 9f;           // 碎屑最大速度
        private const int DEBRIS_MIN_LIFE = 18;              // 碎屑最小寿命
        private const int DEBRIS_MAX_LIFE = 26;              // 碎屑最大寿命

        // 颜色参数
        private static readonly Color EXPLOSION_COLOR = new Color(255, 220, 80);  // 亮黄色
        private static readonly Color DEBRIS_COLOR = new Color(255, 220, 80);     // 碎屑颜色

        // 动画曲线参数
        private const float SCALE_FAST_PHASE = 0.35f;        // 快速放大阶段占比 
        private const float SCALE_FAST_TARGET = 0.675f;       // 快速放大阶段目标比例 

        // 扫描消散参数 - 优化后的参数
        private const float SCAN_START_TIME = 0.35f;          // 开始的时间点（45%时开始消散，更早开始）
        private const float SCAN_DURATION = 0.45f;            // 持续时长（55%的时间完成消散）
        private const float SCAN_EDGE_SOFTNESS = 55f;         // 边缘柔和度（增大过渡带，让消散更平滑）
        private const float SCAN_ALPHA_BASE = 0.4f;           // 基础透明度（避免完全透明后的突兀消失）

        // 旋转参数
        private const float ROTATION_SPEED = 0.03f;           // 旋转速度

        // 多边形生成参数
        private const float SHORT_RADIUS_MIN = 0.2f;          // 短半径最小值
        private const float SHORT_RADIUS_MAX = 0.4f;          // 短半径最大值
        private const float LONG_RADIUS_MIN = 0.75f;          // 长半径最小值
        private const float LONG_RADIUS_MAX = 1.0f;           // 长半径最大值
        private const float ANGLE_OFFSET_RANGE = 0.35f;       // 角度偏移范围
        private const float RADIUS_SWAP_CHANCE = 0.008f;        // 半径交换概率

        // 视觉效果参数
        private const float CENTER_BRIGHTNESS = 1.3f;           // 中心亮度倍数
        private const float EDGE_BRIGHTNESS_MIN = 1.5f;       // 边缘最小亮度
        private const float EDGE_BRIGHTNESS_MAX = 1.8f;       // 边缘最大亮度

       
        private List<Vector2> vertices;

 
        private List<DebrisParticle> debris = new List<DebrisParticle>();

      
        private float scale = 0f;          
        private int currentLife;

     
        private Vector2 scanCenter;        
        private float scanRadius = 0f;       
        private float scanMaxRadius;         
        private float[] vertexDistances;      

       
        private Texture2D noiseTexture;

   
        private bool initialized = false;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MAX_LIFE;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.hostile = false;
        }

        public override void AI()
        {
            if (!initialized)
            {
                InitializePolygon();
                InitializeDebris();
                InitializeScanCenter();
                initialized = true;
                noiseTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Rogue/FireworksHand/NoiseTexture").Value;
            }

            currentLife++;


            float progress = (float)currentLife / MAX_LIFE;

          
            if (progress < SCALE_FAST_PHASE)
            {
                scale = MathHelper.SmoothStep(0f, SCALE_FAST_TARGET, progress / SCALE_FAST_PHASE);
            }
            else
            {
                float slowProgress = (progress - SCALE_FAST_PHASE) / (1f - SCALE_FAST_PHASE);
                scale = MathHelper.SmoothStep(SCALE_FAST_TARGET, 1f, slowProgress);
            }

         
            if (progress > SCAN_START_TIME)
            {
                float scanProgress = (progress - SCAN_START_TIME) / SCAN_DURATION;
                scanProgress = MathHelper.Clamp(scanProgress, 0f, 1f);

              
                float easedProgress = EaseOutCubic(scanProgress);
                scanRadius = scanMaxRadius * easedProgress;
            }
            else
            {
                scanRadius = 0f;
            }

          
            Projectile.rotation += ROTATION_SPEED;

      
            for (int i = debris.Count - 1; i >= 0; i--)
            {
                debris[i].Update();
                if (debris[i].IsDead)
                {
                    debris.RemoveAt(i);
                }
            }
        }

        
        private float EaseOutCubic(float t)
        {
            return 1f - (1f - t) * (1f - t) * (1f - t);
        }

        private void InitializePolygon()
        {
        
            int vertexCount;
            do
            {
                vertexCount = Main.rand.Next(MIN_VERTICES, MAX_VERTICES + 1);
            } while (vertexCount % 2 != 0);

            vertices = GenerateSharpPolygon(vertexCount);
        }

        
        private List<Vector2> GenerateSharpPolygon(int count)
        {
            List<Vector2> points = new List<Vector2>();

            
            float[] radii = new float[count];

          
            for (int i = 0; i < count; i++)
            {
                if (i % 2 == 0)
                {
                    radii[i] = SHORT_RADIUS_MIN + (SHORT_RADIUS_MAX - SHORT_RADIUS_MIN) * (float)Main.rand.NextDouble();
                }
                else
                {
                    radii[i] = LONG_RADIUS_MIN + (LONG_RADIUS_MAX - LONG_RADIUS_MIN) * (float)Main.rand.NextDouble();
                }
            }

  
            for (int i = 0; i < count - 1; i++)
            {
                if (Main.rand.NextFloat() < RADIUS_SWAP_CHANCE)
                {
                    float temp = radii[i];
                    radii[i] = radii[i + 1];
                    radii[i + 1] = temp;
                }
            }

          
            float angleStep = MathHelper.TwoPi / count;
            for (int i = 0; i < count; i++)
            {
                float angle = i * angleStep;
                float randomOffset = (float)Main.rand.NextDouble() * ANGLE_OFFSET_RANGE - (ANGLE_OFFSET_RANGE / 2f);
                angle += randomOffset;

                float radius = MAX_RADIUS * radii[i];
                Vector2 offset = new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius
                );
                points.Add(offset);
            }

            // 按角度排序
            points.Sort((a, b) => Math.Atan2(a.Y, a.X).CompareTo(Math.Atan2(b.Y, b.X)));

            return points;
        }

       
        private void InitializeScanCenter()
        {
            
            scanCenter = Projectile.Center;

          
            scanMaxRadius = 0f;
            vertexDistances = new float[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                float dist = vertices[i].Length(); 
                vertexDistances[i] = dist;
                if (dist > scanMaxRadius)
                {
                    scanMaxRadius = dist;
                }
            }
           
            scanMaxRadius += 10f;
        }

       
        private float GetScanAlpha(Vector2 worldPos, Vector2 localPos)
        {
            float progress = (float)currentLife / MAX_LIFE;

          
            if (progress <= SCAN_START_TIME)
            {
                return 1f;
            }


            float distance = Vector2.Distance(worldPos, scanCenter);


            float radiusRatio;
            if (scanRadius <= 0f)
            {
                radiusRatio = 1f;
            }
            else
            {
                radiusRatio = MathHelper.Clamp(distance / scanRadius, 0f, 1f);
            }

           
            float alpha;
            if (radiusRatio <= 0.8f)
            {
              
                alpha = 0f;
            }
            else
            {
                
                float edgeRatio = (radiusRatio - 0.8f) / 0.2f;
                alpha = MathHelper.SmoothStep(0f, 1f, edgeRatio);
            }

   
            float overallProgress = (progress - SCAN_START_TIME) / SCAN_DURATION;
            overallProgress = MathHelper.Clamp(overallProgress, 0f, 1f);
            float overallAlpha = 1f - overallProgress * 0.7f; // 整体缓慢降低到30%

            float finalAlpha = alpha * overallAlpha;

           
            finalAlpha = MathHelper.Max(finalAlpha, SCAN_ALPHA_BASE * (1f - overallProgress));

            return finalAlpha;
        }

        // 初始化碎屑粒子
        private void InitializeDebris()
        {
            for (int i = 0; i < DEBRIS_COUNT; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(DEBRIS_MIN_SPEED, DEBRIS_MAX_SPEED);
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                Vector2 position = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
                float size = Main.rand.NextFloat(DEBRIS_MIN_SIZE, DEBRIS_MAX_SIZE);
                float rotSpeed = Main.rand.NextFloat(-0.25f, 0.25f);
                int life = Main.rand.Next(DEBRIS_MIN_LIFE, DEBRIS_MAX_LIFE);

                debris.Add(new DebrisParticle(position, velocity, size, rotSpeed, life, DEBRIS_COLOR, MAX_LIFE));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (!initialized || vertices == null || vertices.Count < 3) return false;

            // 获取纹理
            Texture2D texture = noiseTexture;
            if (texture == null || texture.IsDisposed)
            {
                return false;
            }

           
            Main.spriteBatch.End();

     
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.LinearClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            GraphicsDevice gd = Main.graphics.GraphicsDevice;
            DrawPolygon(gd, texture);

        
            Main.spriteBatch.End();

 
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            DrawDebris();

            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        private void DrawPolygon(GraphicsDevice gd, Texture2D texture)
        {
            List<VertexData> vertexList = new List<VertexData>();
            int vertexCount = vertices.Count;

            float progress = (float)currentLife / MAX_LIFE;


            for (int i = 0; i < vertexCount; i++)
            {
                // 计算当前缩放后的顶点世界坐标
                Vector2 scaledLocal = vertices[i] * scale;
                Vector2 worldPos = Projectile.Center + scaledLocal;
                Vector2 screenPos = worldPos - Main.screenPosition;

    
                float pointAlpha = GetScanAlpha(worldPos, scaledLocal);


                float angle = (float)Math.Atan2(scaledLocal.Y, scaledLocal.X);
                float u = (angle + MathHelper.Pi) / MathHelper.TwoPi;

                float radiusRatio = scaledLocal.Length() / MAX_RADIUS;
                float v = radiusRatio;

      
                float edgeStrength = EDGE_BRIGHTNESS_MIN + radiusRatio * (EDGE_BRIGHTNESS_MAX - EDGE_BRIGHTNESS_MIN);
                edgeStrength *= (0.9f + 0.1f * (1f - progress));

        
                if (progress > SCAN_START_TIME)
                {
                    float fadeProgress = (progress - SCAN_START_TIME) / SCAN_DURATION;
                    edgeStrength *= (1f - fadeProgress * 0.5f);
                }

      
                Color finalColor = new Color(
                    (int)(EXPLOSION_COLOR.R * pointAlpha),
                    (int)(EXPLOSION_COLOR.G * pointAlpha),
                    (int)(EXPLOSION_COLOR.B * pointAlpha)
                ) * edgeStrength;

                vertexList.Add(new VertexData(screenPos, new Vector3(u, v, 1), finalColor));
            }

          
            Vector2 centerWorld = Projectile.Center;
            float centerAlpha = GetScanAlpha(centerWorld, Vector2.Zero);
            Vector2 centerScreen = Projectile.Center - Main.screenPosition;
            Vector3 centerUV = new Vector3(0.5f, 0, 1);
            Color centerColor = EXPLOSION_COLOR * centerAlpha * CENTER_BRIGHTNESS;

        
            if (vertexList.Count >= 3)
            {
                List<VertexData> triangles = new List<VertexData>();

                for (int i = 0; i < vertexList.Count; i++)
                {
                    int nextIndex = (i + 1) % vertexList.Count;

                    triangles.Add(new VertexData(centerScreen, centerUV, centerColor));
                    triangles.Add(vertexList[i]);
                    triangles.Add(vertexList[nextIndex]);
                }

                if (triangles.Count >= 3)
                {
                    gd.Textures[0] = texture;
                    gd.DrawUserPrimitives(PrimitiveType.TriangleList, triangles.ToArray(), 0, triangles.Count / 3);
                }
            }
        }

        private void DrawDebris()
        {
            foreach (var particle in debris)
            {
              
                float debrisAlpha = GetScanAlpha(particle.GetPosition(), Vector2.Zero);
                if (debrisAlpha <= 0.01f) continue;
                particle.DrawWithAlpha(debrisAlpha);
            }
        }
    }


    public class DebrisParticle
    {
        private Vector2 position;
        private Vector2 velocity;
        private float size;
        private float rotation;
        private float rotationSpeed;
        private int life;
        private int maxLife;
        private Color color;

        public bool IsDead => life <= 0;

        public DebrisParticle(Vector2 position, Vector2 velocity, float size, float rotationSpeed, int life, Color color, int totalEffectLife)
        {
            this.position = position;
            this.velocity = velocity;
            this.size = size;
            this.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            this.rotationSpeed = rotationSpeed;
            this.life = life;
            this.maxLife = life;
            this.color = color;
        }

        public void Update()
        {
            life--;
            position += velocity;
            velocity *= 0.96f;
            rotation += rotationSpeed;
        }

        public Vector2 GetPosition()
        {
            return position;
        }

        public void DrawWithAlpha(float extraAlpha)
        {
            if (life <= 0) return;

            float progress = 1f - (float)life / maxLife;

        
            float alpha = MathHelper.SmoothStep(1f, 0f, progress * 1.2f);

    
            float finalAlpha = alpha * extraAlpha;
            if (finalAlpha <= 0.01f) return;

            Color drawColor = color * finalAlpha;

            Texture2D texture = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Rogue/FireworksHand/NoiseTexture").Value;
            if (texture == null || texture.IsDisposed) return;

            Vector2 screenPos = position - Main.screenPosition;
            float drawSize = size * (0.7f + alpha * 0.5f);

            Main.spriteBatch.Draw(
                texture,
                screenPos,
                null,
                drawColor,
                rotation,
                new Vector2(texture.Width / 2f, texture.Height / 2f),
                drawSize / texture.Width,
                SpriteEffects.None,
                0
            );
        }
    }
}