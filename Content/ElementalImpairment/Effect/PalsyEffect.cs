using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Content.ElementalImpairment.Effect
{

    public abstract class PalsyEffect
    {
        public int CurrentStacks;
        public int FreezeTimer;
        public virtual int MaxStacks => 3;
        public virtual int FreezeDuration => 45;

        public abstract string GetTexturePath(int stacks);

        public virtual void OnConsume(NPC npc)
        {
            FreezeTimer = FreezeDuration;
            PalsyLightningEffect.Play(npc);
        }

        public bool CanConsume() => CurrentStacks > 0;

        public void AddStacks(int amount)
        {
            CurrentStacks = Math.Min(MaxStacks, CurrentStacks + amount);
        }
    }


    public class BasicPalsy : PalsyEffect
    {
        public override string GetTexturePath(int stacks)
        {
            return stacks switch
            {
                3 => "ArknightsMod/Content/ElementalImpairment/Effect/PalsyL3",
                2 => "ArknightsMod/Content/ElementalImpairment/Effect/PalsyL2",
                1 => "ArknightsMod/Content/ElementalImpairment/Effect/PalsyL1",
                _ => null
            };
        }
    }


    public class PalsyGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public PalsyEffect Palsy { get; set; }

        private int consumeCooldown;
        private int projectileBlockTimer;

        private const int ConsumeCooldownFrames = 60;
        private const int BlockWindowFrames = 45;

        public void AddPalsyStacks(int stacks, PalsyEffect customType = null)
        {
            if (Palsy == null)
                Palsy = customType ?? new BasicPalsy();
            Palsy.AddStacks(stacks);
        }

        public override bool PreAI(NPC npc)
        {
            if (Palsy == null) return true;

            if (consumeCooldown > 0)
                consumeCooldown--;
            if (projectileBlockTimer > 0)
                projectileBlockTimer--;

            if (Palsy.FreezeTimer > 0)
            {
                Palsy.FreezeTimer--;
                float shakeIntensity = 1.5f;
                Vector2 shake = new Vector2(
                    (float)(Main.rand.NextDouble() - 0.5) * shakeIntensity * 2,
                    (float)(Main.rand.NextDouble() - 0.5) * shakeIntensity * 2
                );
                npc.position += shake;
                npc.velocity = Vector2.Zero;
                return false;
            }

            if (Palsy.CanConsume() && consumeCooldown <= 0)
            {
                int dangerDist = (npc.width + npc.height) / 2 + 40;
                foreach (var player in Main.player)
                {
                    if (!player.active || player.dead) continue;

                    float dist = Vector2.Distance(npc.Center, player.Center);
                    if (dist < dangerDist && npc.velocity.Length() > 0.5f &&
                        Vector2.Dot(npc.velocity, player.Center - npc.Center) > 0)
                    {
                        Palsy.CurrentStacks--;
                        Palsy.OnConsume(npc);
                        consumeCooldown = ConsumeCooldownFrames;
                        projectileBlockTimer = BlockWindowFrames;
                        npc.velocity = Vector2.Zero;
                        break;
                    }
                }
            }

            return true;
        }

        public bool TryConsumeForProjectile(NPC npc)
        {
            if (Palsy == null || !Palsy.CanConsume() || consumeCooldown > 0)
                return false;

            Palsy.CurrentStacks--;
            Palsy.OnConsume(npc);
            consumeCooldown = ConsumeCooldownFrames;
            projectileBlockTimer = BlockWindowFrames;
            return true;
        }

        public bool IsBlockingProjectiles => projectileBlockTimer > 0;
    }


    public class PalsyDrawSystem : ModSystem
    {
        public override void Load() => Main.OnPostDraw += DrawPalsyIcons;
        public override void Unload() => Main.OnPostDraw -= DrawPalsyIcons;

		private void DrawPalsyIcons(GameTime gameTime) {
			SpriteBatch sb = Main.spriteBatch;
			var drawData = new List<(Texture2D tex, Vector2 pos, float scale)>();

			// 缓存 NPC 数组长度，避免遍历时变化
			int npcCount = Main.npc.Length;
			for (int i = 0; i < npcCount; i++) {
				// 确保索引在有效范围内（防止数组在遍历时收缩）
				if (i >= Main.npc.Length)
					break;

				NPC npc = Main.npc[i];

				// 严格检查 NPC 有效性
				if (npc == null || !npc.active || npc.whoAmI != i)
					continue;

				// 安全获取 GlobalNPC，捕获可能的异常
				PalsyGlobalNPC palsyNPC = null;
				try {
					if (npc.TryGetGlobalNPC<PalsyGlobalNPC>(out var pnpc))
						palsyNPC = pnpc;
				}
				catch {
					// 如果获取失败，跳过该 NPC
					continue;
				}

				if (palsyNPC?.Palsy == null || palsyNPC.Palsy.CurrentStacks <= 0)
					continue;

				int stacks = palsyNPC.Palsy.CurrentStacks;
				string texPath = palsyNPC.Palsy.GetTexturePath(stacks);
				if (string.IsNullOrEmpty(texPath))
					continue;

				// 检查纹理是否存在
				if (!ModContent.HasAsset(texPath))
					continue;

				Texture2D tex = null;
				try {
					tex = ModContent.Request<Texture2D>(texPath).Value;
				}
				catch {
					continue;
				}

				if (tex == null || tex.IsDisposed)
					continue;

				Vector2 drawPos = npc.Center - Main.screenPosition + new Vector2(0, -npc.height * 0.5f - 20);
				float scale = MathHelper.Clamp(Math.Max(npc.width, npc.height) / 300f, 0.25f, 1f);

				drawData.Add((tex, drawPos, scale));
			}

			if (drawData.Count == 0)
				return;

			try {
				sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearClamp,
						 DepthStencilState.None, RasterizerState.CullNone, null, Main.Transform);

				foreach (var (tex, pos, scale) in drawData) {
					if (tex == null || tex.IsDisposed)
						continue;
					sb.Draw(tex, pos, null, Color.White, 0f, tex.Size() * 0.5f, scale, SpriteEffects.None, 0);
				}
			}
			finally {
	
				if (sb != null && !sb.IsDisposed)
					sb.End();
			}
		}
	}


    public class PalsyGlobalProjectile : GlobalProjectile
    {
        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (!projectile.hostile) return;
            if (source is EntitySource_Parent parent && parent.Entity is NPC npc)
            {
                var palsyNPC = npc.GetGlobalNPC<PalsyGlobalNPC>();
                if (palsyNPC == null || palsyNPC.Palsy == null) return;

                if (palsyNPC.IsBlockingProjectiles)
                {
                    projectile.active = false;
                    return;
                }

                if (palsyNPC.TryConsumeForProjectile(npc))
                {
                    projectile.active = false;
                }
            }
        }
    }


    public static class PalsyLightningEffect
    {
        public static void Play(NPC npc)
        {
            if (npc == null || !npc.active) return;

            Projectile.NewProjectile(
                npc.GetSource_FromAI(),
                npc.Center,
                Vector2.Zero,
                ModContent.ProjectileType<PalsyLightningSpawner>(),
                0, 0, Main.myPlayer,
                ai0: npc.whoAmI
            );
        }
    }

    public class PalsyLightningSpawner : ModProjectile
    {
        public override string Texture => "ArknightsMod/Content/ElementalImpairment/Effect/LightningParticle";
        private int spawnTimer;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 27;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            int who = (int)Projectile.ai[0];
            if (who < 0 || who >= Main.npc.Length) { Projectile.Kill(); return; }
            NPC npc = Main.npc[who];
            if (npc == null || !npc.active) { Projectile.Kill(); return; }

            Projectile.Center = npc.Center;

            int count = Main.rand.Next(1, 6);
            for (int i = 0; i < count; i++)
            {
                float w = npc.width;
                float h = npc.height;
                float minDim = Math.Min(w, h);
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(minDim * 0.1f, minDim * 0.7f);
                Vector2 startPos = npc.Center + angle.ToRotationVector2() * dist;
                float dirAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 velocity = dirAngle.ToRotationVector2() * 0.2f;
                float length = minDim * 0.3f;
                if (length < 12f) length = 12f;
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), startPos,
                    velocity,
                    ModContent.ProjectileType<PalsyLightning>(),
                    0, 0, Main.myPlayer, ai0: length);
            }
        }
    }

    public class PalsyLightning : ModProjectile
    {
        public override string Texture => "ArknightsMod/Content/ElementalImpairment/Effect/LightningParticle";

        private struct LightningSegment
        {
            public Vector2 Start, End;
            public float Thickness, Life;
            public Color Color;
            public float UMin, UMax, VMin, VMax;
        }

        private struct SparkParticle
        {
            public Vector2 Position, Velocity;
            public float Life, Size;
            public Color Color;
        }

        private List<LightningSegment> segments;
        private List<SparkParticle> sparks;
        private bool initialized;
        private Texture2D cachedTexture;
        private float timer;
        private float maxLength;
        private Vector2 direction;
        private float noiseOffset;

        private const int SegmentCount = 15;
        private const float MaxThickness = 5f;
        private const float LightningDuration = 27;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.penetrate = -1;
            Projectile.timeLeft = (int)LightningDuration;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            if (!initialized)
            {
                maxLength = Projectile.ai[0];
                if (maxLength <= 0) maxLength = 50f;
                InitializeLightning();
                initialized = true;
            }

            timer++;
            noiseOffset += 0.1f;

            if (segments != null)
            {
                float life = 1f - timer / LightningDuration;
                life = MathHelper.Clamp(life, 0f, 1f);
                for (int i = 0; i < segments.Count; i++)
                {
                    var seg = segments[i];
                    seg.Life = life;
                    if (life > 0.6f)
                        seg.Thickness = MaxThickness * (1f + 0.15f * (float)Math.Sin(timer * 0.3f + i * 0.5f));
                    else
                        seg.Thickness = MaxThickness * (life / 0.6f);
                    segments[i] = seg;
                }
            }

            if (sparks != null)
            {
                for (int i = sparks.Count - 1; i >= 0; i--)
                {
                    var s = sparks[i];
                    s.Life -= 0.03f;
                    s.Position += s.Velocity;
                    s.Velocity *= 0.95f;
                    s.Size *= 0.96f;
                    if (s.Life <= 0f) sparks.RemoveAt(i); else sparks[i] = s;
                }
            }

            if (timer >= LightningDuration) Projectile.Kill();
        }

        private void InitializeLightning()
        {
            cachedTexture = ModContent.Request<Texture2D>(Texture).Value;
            if (cachedTexture == null || cachedTexture.IsDisposed) return;

            direction = Projectile.velocity.SafeNormalize(Vector2.Zero);
            if (direction == Vector2.Zero) direction = Vector2.UnitX;

            segments = new List<LightningSegment>();
            sparks = new List<SparkParticle>();

            List<Vector2> points = GenerateSmoothPath(Projectile.Center, direction, maxLength);
            for (int i = 0; i < points.Count - 1; i++)
            {
                float progress = (float)i / (points.Count - 1);
                segments.Add(new LightningSegment
                {
                    Start = points[i],
                    End = points[i + 1],
                    Thickness = MaxThickness * (0.7f + 0.5f * (float)Math.Sin(progress * Math.PI)),
                    Life = 1f,
                    Color = Color.White,
                    UMin = 0,
                    UMax = 1,
                    VMin = 0,
                    VMax = 1
                });
            }

            if (Main.rand.NextBool(2)) CreateBranches(points);
            CreateSparks(points);
        }

        private List<Vector2> GenerateSmoothPath(Vector2 start, Vector2 dir, float totalLength)
        {
            List<Vector2> points = new List<Vector2>();
            int numPoints = SegmentCount;
            float step = totalLength / numPoints;
            Vector2 cur = start;
            Vector2 curDir = dir;
            List<Vector2> cps = new List<Vector2> { start };

            for (int i = 1; i <= numPoints; i++)
            {
                float prog = (float)i / numPoints;
                float angleNoise = Main.rand.NextFloat(-0.6f, 0.6f) + (float)Math.Sin(i * 0.5f + noiseOffset) * 0.3f;
                curDir = curDir.RotatedBy(angleNoise * 0.4f);
                Vector2 next = cur + curDir * step;
                Vector2 perp = new Vector2(-curDir.Y, curDir.X);
                float perpOff = (float)(Math.Sin(i * 0.8f + noiseOffset) * 3f * Math.Min(prog, 1f - prog));
                next += perp * perpOff;
                cps.Add(next);
                cur = next;
            }

            for (int i = 0; i < cps.Count - 1; i++)
            {
                Vector2 p0 = i > 0 ? cps[i - 1] : cps[i];
                Vector2 p1 = cps[i];
                Vector2 p2 = cps[i + 1];
                Vector2 p3 = i < cps.Count - 2 ? cps[i + 2] : cps[i + 1];
                for (int j = 0; j < 2; j++)
                {
                    float t = j / 2f;
                    points.Add(CatmullRom(p0, p1, p2, p3, t));
                }
            }
            points.Add(cps[cps.Count - 1]);
            return points;
        }

        private Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t, t3 = t2 * t;
            return 0.5f * ((2f * p1) + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }

        private void CreateBranches(List<Vector2> mainPath)
        {
            int bCount = Main.rand.Next(1, 3);
            for (int b = 0; b < bCount; b++)
            {
                int idx = Main.rand.Next(2, mainPath.Count - 3);
                Vector2 startPos = mainPath[idx];
                float ang = Main.rand.NextFloat(-1f, 1f);
                Vector2 brDir = direction.RotatedBy(ang);
                int brLen = Main.rand.Next(2, 4);
                float brStep = maxLength * 0.2f;
                Vector2 cur = startPos;
                Vector2 dir = brDir;
                for (int i = 0; i < brLen; i++)
                {
                    float p = (float)i / brLen;
                    dir = dir.RotatedBy(Main.rand.NextFloat(-0.4f, 0.4f));
                    Vector2 next = cur + dir * brStep * (1f - p * 0.3f);
                    segments.Add(new LightningSegment
                    {
                        Start = cur,
                        End = next,
                        Thickness = MaxThickness * 0.4f * (1f - p * 0.4f),
                        Life = 1f,
                        Color = Color.White,
                        UMin = 0.1f,
                        UMax = 0.9f,
                        VMin = 0.1f,
                        VMax = 0.9f
                    });
                    cur = next;
                }
            }
        }

        private void CreateSparks(List<Vector2> mainPath)
        {
            int n = Main.rand.Next(2, 5);
            for (int i = 0; i < n; i++)
            {
                int pi = Main.rand.Next(1, mainPath.Count - 2);
                Vector2 pos = mainPath[pi] + new Vector2(Main.rand.NextFloat(-6, 6), Main.rand.NextFloat(-6, 6));
                sparks.Add(new SparkParticle
                {
                    Position = pos,
                    Velocity = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-0.8f, 0.8f)),
                    Life = Main.rand.NextFloat(0.3f, 0.5f),
                    Size = Main.rand.NextFloat(1f, 2.5f),
                    Color = Color.White
                });
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (cachedTexture == null || cachedTexture.IsDisposed) return false;
            try
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                DrawLightning();
                DrawSparks();
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch
            {
                if (!Main.spriteBatch.IsDisposed) { Main.spriteBatch.End(); Main.spriteBatch.Begin(); }
            }
            return false;
        }

        private void DrawLightning()
        {
            if (segments == null || segments.Count == 0) return;
            var verts = new List<VertexData>();
            foreach (var seg in segments)
            {
                if (seg.Life <= 0.05f) continue;
                Vector2 dir = seg.End - seg.Start;
                if (dir.Length() < 1f) continue;
                dir.Normalize();
                Vector2 perp = new Vector2(-dir.Y, dir.X);
                Color col = seg.Color * seg.Life;
                float thick = seg.Thickness * seg.Life;
                Vector2 ss = seg.Start - Main.screenPosition;
                Vector2 se = seg.End - Main.screenPosition;
                Vector2 lS = ss + perp * thick * 0.5f;
                Vector2 rS = ss - perp * thick * 0.5f;
                Vector2 lE = se + perp * thick * 0.5f;
                Vector2 rE = se - perp * thick * 0.5f;

                verts.Add(new VertexData(lS, new Vector3(0, 0, 0), col));
                verts.Add(new VertexData(rS, new Vector3(1, 0, 0), col));
                verts.Add(new VertexData(lE, new Vector3(0, 1, 0), col));
                verts.Add(new VertexData(rS, new Vector3(1, 0, 0), col));
                verts.Add(new VertexData(rE, new Vector3(1, 1, 0), col));
                verts.Add(new VertexData(lE, new Vector3(0, 1, 0), col));
            }
            if (verts.Count >= 3)
            {
                Main.graphics.GraphicsDevice.Textures[0] = cachedTexture;
                Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts.ToArray(), 0, verts.Count / 3);
            }
        }

        private void DrawSparks()
        {
            if (sparks == null || sparks.Count == 0) return;
            Texture2D pik = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            var verts = new List<VertexData>();
            foreach (var sp in sparks)
            {
                if (sp.Life <= 0f) continue;
                Color col = sp.Color * sp.Life;
                Vector2 screen = sp.Position - Main.screenPosition;
                float s = sp.Size * sp.Life * 0.5f;
                Vector2 tl = screen + new Vector2(-s, -s);
                Vector2 tr = screen + new Vector2(s, -s);
                Vector2 bl = screen + new Vector2(-s, s);
                Vector2 br = screen + new Vector2(s, s);
                verts.Add(new VertexData(tl, new Vector3(0, 0, 0), col));
                verts.Add(new VertexData(tr, new Vector3(1, 0, 0), col));
                verts.Add(new VertexData(bl, new Vector3(0, 1, 0), col));
                verts.Add(new VertexData(tr, new Vector3(1, 0, 0), col));
                verts.Add(new VertexData(br, new Vector3(1, 1, 0), col));
                verts.Add(new VertexData(bl, new Vector3(0, 1, 0), col));
            }
            if (verts.Count >= 3)
            {
                Main.graphics.GraphicsDevice.Textures[0] = pik;
                Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts.ToArray(), 0, verts.Count / 3);
            }
        }
    }


    public struct VertexData : IVertexType
    {
        public Vector2 Position;
        public Vector3 TexCoord;
        public Color Color;
        public VertexData(Vector2 position, Vector3 texCoord, Color color)
        {
            Position = position;
            TexCoord = texCoord;
            Color = color;
        }
        public VertexDeclaration VertexDeclaration => _vertexDeclaration;
        private static readonly VertexDeclaration _vertexDeclaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(8, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(20, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );
    }
}