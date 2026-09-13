using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Valarqvin
{
    public class ValarqvinProj_Hit : ModProjectile
    {
        // 特效总帧数
        private const int TotalFrames = 20;
        private const int PeakFrame = 10;
        private const int ShrinkEndFrame = 13;

        // 十字光效尺寸（缩小、缩短）
        private const float MainStartWidth = 18f;
        private const float MainStartHeight = 3f;
        private const float MainPeakWidth = 45f;
        private const float MainPeakHeight = 6f;
        private const float MainShrinkWidth = 30f;
        private const float MainShrinkHeight = 4f;
        private const float MainEndWidth = 6f;
        private const float MainEndHeight = 1.5f;

        private const float OuterScaleStart = 1.5f;
        private const float OuterScalePeak = 1.3f;
        private const float OuterScaleShrink = 1.1f;
        private const float OuterScaleEnd = 1.0f;

        private const float CrossAngleError = 0.1f;
        private static readonly Color MainColor = new Color(120, 150, 220);
        private static readonly Color OuterColor = new Color(50, 70, 140);

        // 三层圆形光晕（缩小）
        private const int CoreGlowTotalFrames = 15;
        private const int CoreGlowPeakFrame = 5;
        private const float CoreGlowStartSize = 8f;
        private const float CoreGlowPeakSize = 25f;
        private const float CoreGlowEndSize = 3f;
        private const float CoreGlowOuterScale = 1.6f;
        private static readonly Color CoreGlowMainColor = new Color(160, 190, 220);
        private static readonly Color CoreGlowOuterColor = new Color(60, 80, 160);

        private const int MidGlowTotalFrames = 15;
        private const int MidGlowPeakFrame = 6;
        private const float MidGlowStartSize = 14f;
        private const float MidGlowPeakSize = 40f;
        private const float MidGlowEndSize = 5f;
        private const float MidGlowOuterScale = 1.5f;
        private static readonly Color MidGlowMainColor = new Color(110, 150, 210);
        private static readonly Color MidGlowOuterColor = new Color(40, 60, 130);

        private const int BgGlowTotalFrames = 12;
        private const float BgGlowStartSize = 20f;
        private const float BgGlowPeakSize = 70f;
        private const float BgGlowEndSize = 10f;
        private const float BgGlowOuterScale = 1.3f;
        private static readonly Color BgGlowMainColor = new Color(80, 110, 170);
        private static readonly Color BgGlowOuterColor = new Color(30, 40, 90);

        // 粒子数量（削减）
        private const int LightParticleCount = 5;
        private const int HitParticleCount = 6;
        private const int PolyParticleCount = 6;
        private const int PolyParticleMinLife = 20;
        private const int PolyParticleMaxLife = 35;

        private const int RandomParticleCount = 5;
        private const int RandomParticleMinLife = 12;
        private const int RandomParticleMaxLife = 25;

        // 锯齿爆炸参数（缩小）
        private const int ExplosionSpikeCount = 30;
        private const float ExplosionMaxRadius = 28f;
        private const float ExplosionSpikeMinLength = 0.2f;
        private const float ExplosionSpikeMaxLength = 0.8f;
        private const float ExplosionDirectionWeight = 0.15f;

        private const int MaskStartFrame = 2;
        private const float MaskExpandSpeed = 1.8f;
        private const float MaskFeatherWidth = 0.5f;

        private static readonly Color ExplosionFillColor = new Color(15, 25, 45);
        private static readonly Color ExplosionEdgeColor = new Color(35, 50, 85);
        private const float ExplosionAlpha = 0.45f;

        private static readonly Color[] PolyColors = new Color[]
        {
            new Color(80, 170, 220),
            new Color(60, 130, 200),
            new Color(40, 90, 180),
        };

        private struct LightParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Size;
            public float MaxSize;
            public int Life;
            public int MaxLife;
            public Color Color;
        }

        private struct PolyParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Size;
            public float MaxSize;
            public int Sides;
            public float Rotation;
            public float RotationSpeed;
            public Color Color;
            public int Life;
            public int MaxLife;
        }

        private struct RandomParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Size;
            public float MaxSize;
            public int Life;
            public int MaxLife;
            public Color Color;
        }

        private struct ExplosionData
        {
            public Vector2[] SpikeEnds;
            public float[] SpikeLengths;
        }

        private Texture2D _lightTexture;
        private Texture2D _crossTexture;
        private Vector2 _spawnPosition;
        private float _crossBaseAngle;

        private LightParticle[] _lightParticles;
        private int _lightCount;
        private PolyParticle[] _polyParticles;
        private int _polyCount;
        private RandomParticle[] _randomParticles;
        private int _randomCount;
        private ExplosionData _explosionData;
        private BasicEffect _basicEffect;
        private VertexPositionColor[] _explosionVertices;
        private bool _spawned;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = TotalFrames;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            if (!_spawned)
            {
                _spawned = true;
                _spawnPosition = Projectile.Center;
                _crossBaseAngle = Main.rand.NextFloat(-CrossAngleError, CrossAngleError);

                int maxLights = LightParticleCount * 2;
                int maxPolys = PolyParticleCount * 2;
                int maxRandoms = RandomParticleCount * 2;
                _lightParticles = new LightParticle[maxLights];
                _polyParticles = new PolyParticle[maxPolys];
                _randomParticles = new RandomParticle[maxRandoms];

                InitializeExplosion();
                SpawnParticles();
            }

            UpdateParticles();
        }

        private void UpdateParticles()
        {
            for (int i = _lightCount - 1; i >= 0; i--)
            {
                _lightParticles[i].Life--;
                if (_lightParticles[i].Life <= 0) { RemoveLightAt(i); continue; }
                _lightParticles[i].Position += _lightParticles[i].Velocity;
                _lightParticles[i].Velocity *= 0.94f;
                float p = 1f - (float)_lightParticles[i].Life / _lightParticles[i].MaxLife;
                _lightParticles[i].Size = _lightParticles[i].MaxSize / (1f + p * 3f);
            }

            for (int i = _polyCount - 1; i >= 0; i--)
            {
                _polyParticles[i].Life--;
                if (_polyParticles[i].Life <= 0) { RemovePolyAt(i); continue; }
                _polyParticles[i].Position += _polyParticles[i].Velocity;
                _polyParticles[i].Velocity *= 0.96f;
                _polyParticles[i].Rotation += _polyParticles[i].RotationSpeed;
                float p = 1f - (float)_polyParticles[i].Life / _polyParticles[i].MaxLife;
                _polyParticles[i].Size = _polyParticles[i].MaxSize / (1f + p * 2f);
                if (_polyParticles[i].Life % 8 == 0 && _polyParticles[i].Sides > 3)
                {
                    _polyParticles[i].Sides--;
                }
                _polyParticles[i].Position.X += (float)Math.Sin(_polyParticles[i].Life * 0.3f) * 0.3f;
                _polyParticles[i].Position.Y += (float)Math.Cos(_polyParticles[i].Life * 0.5f) * 0.3f;
            }

            for (int i = _randomCount - 1; i >= 0; i--)
            {
                _randomParticles[i].Life--;
                if (_randomParticles[i].Life <= 0) { RemoveRandomAt(i); continue; }
                _randomParticles[i].Position += _randomParticles[i].Velocity;
                _randomParticles[i].Velocity *= 0.92f;
                float p = 1f - (float)_randomParticles[i].Life / _randomParticles[i].MaxLife;
                _randomParticles[i].Size = _randomParticles[i].MaxSize * (1f - p);
            }
        }

        private void RemoveLightAt(int index)
        {
            _lightCount--;
            if (index < _lightCount)
                _lightParticles[index] = _lightParticles[_lightCount];
        }

        private void RemovePolyAt(int index)
        {
            _polyCount--;
            if (index < _polyCount)
                _polyParticles[index] = _polyParticles[_polyCount];
        }

        private void RemoveRandomAt(int index)
        {
            _randomCount--;
            if (index < _randomCount)
                _randomParticles[index] = _randomParticles[_randomCount];
        }

        private void InitializeExplosion()
        {
            _explosionData.SpikeEnds = new Vector2[ExplosionSpikeCount];
            _explosionData.SpikeLengths = new float[ExplosionSpikeCount];

            float step = MathHelper.TwoPi / ExplosionSpikeCount;
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            float baseAngle = dir.ToRotation();

            for (int i = 0; i < ExplosionSpikeCount; i++)
            {
                float angle = i * step;
                float noise = MathF.Sin(i * 1.7f + 0.5f) * 0.3f
                            + MathF.Sin(i * 4.1f + 2.3f) * 0.2f;
                if (Main.rand.NextFloat() < 0.3f) noise += Main.rand.NextFloat(-0.25f, 0.35f);
                float height = MathHelper.Clamp(0.5f + noise, 0.15f, 0.85f);
                float ratio = MathHelper.Lerp(ExplosionSpikeMinLength, ExplosionSpikeMaxLength, height);
                float diff = angle - baseAngle;
                float dirFactor = 1f + ExplosionDirectionWeight * MathF.Cos(diff);
                ratio *= MathHelper.Clamp(dirFactor, 0.7f, 1.3f);
                _explosionData.SpikeLengths[i] = ratio;
                float r = ExplosionMaxRadius * ratio;
                _explosionData.SpikeEnds[i] = new Vector2(MathF.Cos(angle) * r, MathF.Sin(angle) * r);
            }

            _explosionVertices = new VertexPositionColor[ExplosionSpikeCount * 3];
        }

        private void SpawnParticles()
        {
            Vector2 hitPos = _spawnPosition;
            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            float baseSpeed = Projectile.velocity.Length();

            for (int i = 0; i < HitParticleCount; i++)
            {
                float angle = dir.ToRotation() + Main.rand.NextFloat(-1.0f, 1.0f);
                float speed = baseSpeed * Main.rand.NextFloat(0.3f, 0.6f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                vel += Main.rand.NextVector2Circular(1.5f, 1.5f);

                int life = Main.rand.Next(25, 40);
                float size = Main.rand.NextFloat(10f, 16f);
                Color col = new Color(68, 90, 172).MultiplyRGB(Color.White * Main.rand.NextFloat(0.7f, 0.9f));

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), hitPos, vel,
                    ModContent.ProjectileType<ValarqvinHitParticle>(), 0, 0f, Projectile.owner,
                    ai0: life, ai1: size, ai2: col.PackedValue);
            }

            for (int i = 0; i < LightParticleCount; i++)
            {
                AddLightParticle(hitPos + Main.rand.NextVector2Circular(5f, 5f),
                    Main.rand.NextVector2Circular(0.8f, 1.5f) + dir * Main.rand.NextFloat(0.3f, 1f),
                    Main.rand.NextFloat(0.8f, 1.5f), Main.rand.Next(20, 35), Color.White);
            }

            for (int i = 0; i < PolyParticleCount; i++)
            {
                PolyParticle pp;
                pp.Position = hitPos + Main.rand.NextVector2Circular(4f, 4f);
                pp.Velocity = Main.rand.NextVector2Circular(1.5f, 2.5f) + dir * Main.rand.NextFloat(0.8f, 2f);
                pp.MaxSize = Main.rand.NextFloat(6f, 10f);
                pp.Size = pp.MaxSize;
                pp.Sides = Main.rand.Next(3, 6);
                pp.Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                pp.RotationSpeed = Main.rand.NextFloat(-0.08f, 0.08f);
                pp.Color = PolyColors[Main.rand.Next(PolyColors.Length)];
                pp.MaxLife = Main.rand.Next(PolyParticleMinLife, PolyParticleMaxLife);
                pp.Life = pp.MaxLife;
                _polyParticles[_polyCount++] = pp;
            }

            for (int i = 0; i < RandomParticleCount; i++)
            {
                RandomParticle rp;
                rp.Position = hitPos + Main.rand.NextVector2Circular(4f, 4f);
                rp.Velocity = Main.rand.NextVector2Circular(2f, 3f);
                rp.MaxSize = Main.rand.NextFloat(0.5f, 1.0f);
                rp.Size = rp.MaxSize;
                rp.MaxLife = Main.rand.Next(RandomParticleMinLife, RandomParticleMaxLife);
                rp.Life = rp.MaxLife;
                rp.Color = PolyColors[Main.rand.Next(PolyColors.Length)];
                _randomParticles[_randomCount++] = rp;
            }
        }

        private void AddLightParticle(Vector2 pos, Vector2 vel, float maxSize, int maxLife, Color color)
        {
            _lightParticles[_lightCount].Position = pos;
            _lightParticles[_lightCount].Velocity = vel;
            _lightParticles[_lightCount].MaxSize = maxSize;
            _lightParticles[_lightCount].Size = maxSize;
            _lightParticles[_lightCount].MaxLife = maxLife;
            _lightParticles[_lightCount].Life = maxLife;
            _lightParticles[_lightCount].Color = color;
            _lightCount++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (_lightTexture == null)
                _lightTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Caster/Valarqvin/Light").Value;
            if (_crossTexture == null)
                _crossTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Rogue/Dedication/Light_horizontal").Value;
            if (_lightTexture == null || _crossTexture == null) return false;

            int frame = TotalFrames - Projectile.timeLeft;
            if (frame < 0 || frame >= TotalFrames) return false;

            DrawExplosion(frame);
            DrawBgGlow(frame);
            DrawMidGlow(frame);
            DrawCoreGlow(frame);
            DrawLightParticles();
            DrawPolyParticles();
            DrawCrossEffect(frame);
            DrawRandomParticles();
            return false;
        }

        private void DrawExplosion(int frame)
        {
            if (_explosionData.SpikeEnds == null) return;
            Main.spriteBatch.End();
            GraphicsDevice gd = Main.graphics.GraphicsDevice;
            BlendState prevBlend = gd.BlendState;
            RasterizerState prevRaster = gd.RasterizerState;
            gd.BlendState = BlendState.AlphaBlend;
            gd.RasterizerState = RasterizerState.CullNone;

            if (_basicEffect == null) { _basicEffect = new BasicEffect(gd); _basicEffect.VertexColorEnabled = true; }
            _basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
            _basicEffect.View = Main.GameViewMatrix.TransformationMatrix;
            _basicEffect.World = Matrix.Identity;

            float progress = (float)frame / TotalFrames;
            float globalAlpha = ExplosionAlpha * (1f - progress * progress);
            float scaleT = MathHelper.Clamp(progress / 0.35f, 0f, 1f);
            float scale = 1f - (1f - scaleT) * (1f - scaleT);

            Vector2 screenCenter = _spawnPosition - Main.screenPosition;
            float maskRadius = 0f;
            if (frame >= MaskStartFrame) maskRadius = (frame - MaskStartFrame) * MaskExpandSpeed;
            float featherOuter = maskRadius;
            float featherInner = maskRadius * (1f - MaskFeatherWidth);

            int triCount = 0;
            for (int i = 0; i < ExplosionSpikeCount; i++)
            {
                int next = (i + 1) % ExplosionSpikeCount;
                Vector2 v1 = screenCenter + _explosionData.SpikeEnds[i] * scale;
                Vector2 v2 = screenCenter + _explosionData.SpikeEnds[next] * scale;
                float d1 = Vector2.Distance(v1, screenCenter);
                float d2 = Vector2.Distance(v2, screenCenter);

                float a1 = GetMaskAlpha(d1, featherOuter, featherInner, globalAlpha);
                float a2 = GetMaskAlpha(d2, featherOuter, featherInner, globalAlpha);
                float aC = GetMaskAlpha(0f, featherOuter, featherInner, globalAlpha);
                if (aC < 0.01f && a1 < 0.01f && a2 < 0.01f) continue;

                _explosionVertices[triCount++] = new VertexPositionColor(new Vector3(screenCenter, 0), ExplosionFillColor * aC);
                _explosionVertices[triCount++] = new VertexPositionColor(new Vector3(v1, 0), ExplosionEdgeColor * a1);
                _explosionVertices[triCount++] = new VertexPositionColor(new Vector3(v2, 0), ExplosionEdgeColor * a2);
            }

            if (triCount >= 3)
                foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
                { pass.Apply(); gd.DrawUserPrimitives(PrimitiveType.TriangleList, _explosionVertices, 0, triCount / 3); }

            gd.BlendState = prevBlend; gd.RasterizerState = prevRaster;
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private float GetMaskAlpha(float dist, float maskOuter, float maskInner, float baseAlpha)
        {
            if (maskOuter <= 0f) return baseAlpha;
            if (dist <= maskInner) return 0f;
            if (dist >= maskOuter) return baseAlpha;
            float t = (dist - maskInner) / (maskOuter - maskInner);
            return baseAlpha * t;
        }

        private void DrawCoreGlow(int frame) { DrawGlow(frame, CoreGlowTotalFrames, CoreGlowPeakFrame, CoreGlowStartSize, CoreGlowPeakSize, CoreGlowEndSize, CoreGlowOuterScale, CoreGlowMainColor, CoreGlowOuterColor, 1f); }
        private void DrawMidGlow(int frame) { DrawGlow(frame, MidGlowTotalFrames, MidGlowPeakFrame, MidGlowStartSize, MidGlowPeakSize, MidGlowEndSize, MidGlowOuterScale, MidGlowMainColor, MidGlowOuterColor, 0.9f); }

        private void DrawBgGlow(int frame)
        {
            float progress = (float)frame / BgGlowTotalFrames;
            if (progress >= 1f) return;
            float alpha = 1f - progress * progress * progress;
            DrawGlow(frame, BgGlowTotalFrames, MidGlowPeakFrame, BgGlowStartSize, BgGlowPeakSize, BgGlowEndSize, BgGlowOuterScale, BgGlowMainColor, BgGlowOuterColor, alpha);
        }

        private void DrawGlow(int frame, int totalFrames, int peakFrame, float startSize, float peakSize, float endSize, float outerScale, Color mainColor, Color outerColor, float baseAlpha)
        {
            if (frame >= totalFrames) return;
            if (_lightTexture == null || _lightTexture.IsDisposed) return;

            float progress = (float)frame / totalFrames;
            float size;
            if (frame <= peakFrame) { float t = frame / (float)peakFrame; float e = 1f - (1f - t) * (1f - t); size = startSize + (peakSize - startSize) * e; }
            else { float t = (frame - peakFrame) / (float)(totalFrames - 1 - peakFrame); float e = t * t; size = peakSize - (peakSize - endSize) * e; }

            float alpha = baseAlpha * (1f - progress * progress);
            Vector2 sp = _spawnPosition - Main.screenPosition;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Rectangle mr = new Rectangle((int)(sp.X - size * 0.5f), (int)(sp.Y - size * 0.5f), (int)size, (int)size);
            Main.spriteBatch.Draw(_lightTexture, mr, null, mainColor * alpha);
            float os = size * outerScale;
            Rectangle or = new Rectangle((int)(sp.X - os * 0.5f), (int)(sp.Y - os * 0.5f), (int)os, (int)os);
            Main.spriteBatch.Draw(_lightTexture, or, null, outerColor * alpha * 0.6f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawLightParticles()
        {
            if (_lightCount == 0) return;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            for (int i = 0; i < _lightCount; i++)
            {
                float alpha = (float)_lightParticles[i].Life / _lightParticles[i].MaxLife;
                float size = _lightParticles[i].Size * 10f;
                Vector2 pos = _lightParticles[i].Position - Main.screenPosition;
                Rectangle rect = new Rectangle((int)(pos.X - size * 0.5f), (int)(pos.Y - size * 0.5f), (int)size, (int)size);
                Main.spriteBatch.Draw(_lightTexture, rect, null, _lightParticles[i].Color * alpha, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawPolyParticles()
        {
            if (_polyCount == 0) return;
            Main.spriteBatch.End();
            GraphicsDevice gd = Main.graphics.GraphicsDevice;
            BlendState prevBlend = gd.BlendState; RasterizerState prevRaster = gd.RasterizerState;
            gd.BlendState = BlendState.Additive; gd.RasterizerState = RasterizerState.CullNone;
            if (_basicEffect == null) { _basicEffect = new BasicEffect(gd); _basicEffect.VertexColorEnabled = true; }
            _basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
            _basicEffect.View = Main.GameViewMatrix.TransformationMatrix; _basicEffect.World = Matrix.Identity;

            for (int i = 0; i < _polyCount; i++)
            {
                float alpha = (float)_polyParticles[i].Life / _polyParticles[i].MaxLife;
                Color drawColor = _polyParticles[i].Color * alpha;
                Color centerColor = drawColor * 0.3f;
                Vector2 center = _polyParticles[i].Position - Main.screenPosition;
                float r = _polyParticles[i].Size;
                int sides = _polyParticles[i].Sides;
                if (sides < 3) sides = 3;

                VertexPositionColor[] verts = new VertexPositionColor[sides * 3];
                for (int j = 0; j < sides; j++)
                {
                    float a1 = _polyParticles[i].Rotation + j * MathHelper.TwoPi / sides;
                    float a2 = _polyParticles[i].Rotation + ((j + 1) % sides) * MathHelper.TwoPi / sides;
                    Vector2 o1 = center + new Vector2(MathF.Cos(a1) * r, MathF.Sin(a1) * r);
                    Vector2 o2 = center + new Vector2(MathF.Cos(a2) * r, MathF.Sin(a2) * r);
                    verts[j * 3 + 0] = new VertexPositionColor(new Vector3(center, 0), centerColor);
                    verts[j * 3 + 1] = new VertexPositionColor(new Vector3(o1, 0), drawColor);
                    verts[j * 3 + 2] = new VertexPositionColor(new Vector3(o2, 0), drawColor);
                }
                foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes) { pass.Apply(); gd.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, sides); }
            }
            gd.BlendState = prevBlend; gd.RasterizerState = prevRaster;
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawCrossEffect(int frame)
        {
            GetCrossSize(frame, out float mW, out float mH, out float oW, out float oH);
            float alpha = CalculateAlpha(frame);
            Vector2 sp = _spawnPosition - Main.screenPosition;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            DrawRotatedCrossPart(sp, mW, mH, oW, oH, alpha, MathHelper.PiOver4 + _crossBaseAngle);
            DrawRotatedCrossPart(sp, mW, mH, oW, oH, alpha, -MathHelper.PiOver4 + _crossBaseAngle);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawRotatedCrossPart(Vector2 c, float mW, float mH, float oW, float oH, float a, float r)
        {
            Vector2 o = _crossTexture.Size() / 2f;
            Main.spriteBatch.Draw(_crossTexture, c, null, MainColor * a, r, o, new Vector2(mW / _crossTexture.Width, mH / _crossTexture.Height), SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(_crossTexture, c, null, OuterColor * a, r, o, new Vector2(oW / _crossTexture.Width, oH / _crossTexture.Height), SpriteEffects.None, 0f);
        }

        private void DrawRandomParticles()
        {
            if (_randomCount == 0) return;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            for (int i = 0; i < _randomCount; i++)
            {
                float alpha = (float)_randomParticles[i].Life / _randomParticles[i].MaxLife;
                float size = _randomParticles[i].Size * 8f;
                Vector2 pos = _randomParticles[i].Position - Main.screenPosition;
                Rectangle rect = new Rectangle((int)(pos.X - size * 0.5f), (int)(pos.Y - size * 0.5f), (int)size, (int)size);
                Main.spriteBatch.Draw(_lightTexture, rect, null, _randomParticles[i].Color * alpha, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void GetCrossSize(int f, out float mW, out float mH, out float oW, out float oH)
        {
            float mw, mh;
            if (f <= PeakFrame) { float t = f / (float)PeakFrame; float e = 1f - (1f - t) * (1f - t); mw = MainStartWidth + (MainPeakWidth - MainStartWidth) * e; mh = MainStartHeight + (MainPeakHeight - MainStartHeight) * e; }
            else if (f <= ShrinkEndFrame) { float t = (f - PeakFrame) / (float)(ShrinkEndFrame - PeakFrame); float e = t * t; mw = MainPeakWidth + (MainShrinkWidth - MainPeakWidth) * e; mh = MainPeakHeight + (MainShrinkHeight - MainPeakHeight) * e; }
            else { float t = (f - ShrinkEndFrame) / (float)(TotalFrames - 1 - ShrinkEndFrame); float e = 1f - (1f - t) * (1f - t); mw = MainShrinkWidth + (MainEndWidth - MainShrinkWidth) * e; mh = MainShrinkHeight + (MainEndHeight - MainShrinkHeight) * e; }
            mW = Math.Max(1, mw); mH = Math.Max(1, mh);
            float s;
            if (f <= PeakFrame) { float t = f / (float)PeakFrame; float e = 1f - (1f - t) * (1f - t); s = OuterScaleStart + (OuterScalePeak - OuterScaleStart) * e; }
            else if (f <= ShrinkEndFrame) { float t = (f - PeakFrame) / (float)(ShrinkEndFrame - PeakFrame); float e = t * t; s = OuterScalePeak + (OuterScaleShrink - OuterScalePeak) * e; }
            else { float t = (f - ShrinkEndFrame) / (float)(TotalFrames - 1 - ShrinkEndFrame); float e = 1f - (1f - t) * (1f - t); s = OuterScaleShrink + (OuterScaleEnd - OuterScaleShrink) * e; }
            oW = mW * s; oH = mH * s;
        }

        private float CalculateAlpha(int f) { float t = f / (float)(TotalFrames - 1); return 1f - t * t; }
    }
}