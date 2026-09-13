using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.ElementalImpairment.Effect
{
	//����Ч����Ŀǰ�Ǹ���Ч����һ�𣬵�ʱ��������
    public class WeaknessData
    {
        public float RemainingTime;
        public float TotalTime;
        public float DamagePerSecond;
        public float DamageAccum;
        public float Strength;
        public Color MistColor;
        public string MistTexture;
        public List<MistParticle> Particles = new();
    }

  
    public struct MistParticle
    {
        public Vector2 Offset;
        public float Life;
        public float MaxLife;
        public float Size;
        public float Alpha;
    }

   
    public static class WeaknessSystem
    {
        private static Dictionary<int, WeaknessData> npcWeakness = new();


        public static float MistSizeMultiplier = 0.85f;


        public static void ApplyWeakness(NPC npc, float durationSeconds,
            float damagePerSecond = 160f, float strength = 0.5f,
            Color? mistColor = null, string mistTexture = "ArknightsMod/Content/ElementalImpairment/Effect/MistMask")
        {
            if (npc == null || !npc.active) return;

            int id = npc.whoAmI;
            if (!npcWeakness.ContainsKey(id) || npcWeakness[id] == null)
                npcWeakness[id] = new WeaknessData();

            var data = npcWeakness[id];
            data.RemainingTime = Math.Max(data.RemainingTime, durationSeconds);
            data.TotalTime = data.RemainingTime;
            data.DamagePerSecond = damagePerSecond;
            data.Strength = strength;
            data.MistColor = mistColor ?? new Color(29, 11, 46, 200);
            data.MistTexture = mistTexture;
            data.DamageAccum = 0f;
        }

    
        public static void Update(NPC npc)
        {
            int id = npc.whoAmI;
            if (!npcWeakness.ContainsKey(id) || npcWeakness[id] == null) return;

            var data = npcWeakness[id];
            if (data.RemainingTime <= 0)
            {
                npcWeakness.Remove(id);
                return;
            }

         
            float frameDamage = data.DamagePerSecond / 60f;
            data.DamageAccum += frameDamage;
            int intDamage = (int)data.DamageAccum;
            if (intDamage > 0)
            {
                npc.life -= intDamage;
                data.DamageAccum -= intDamage;

                if (npc.life <= 0)
                {
                    npc.life = 0;
                    npc.checkDead();
                    npc.active = false;
                    npcWeakness.Remove(id);
                    return;
                }
            }

    
            data.Strength = 0.5f * (data.RemainingTime / data.TotalTime);
            data.RemainingTime -= 1f / 60f;

        
            UpdateParticles(npc, data);

            if (!npc.active)
                npcWeakness.Remove(id);
        }

        private static void UpdateParticles(NPC npc, WeaknessData data)
        {
            Random rand = new Random();

            
            int baseSpawnCount = 2 + rand.Next(2); 
            float timeRatio = data.RemainingTime / data.TotalTime;

          
            int spawnCount;
            if (timeRatio <= 0.05f && data.TotalTime > 0)
            {
                float fadeFactor = timeRatio / 0.05f; 
                spawnCount = (int)(baseSpawnCount * fadeFactor);
                if (spawnCount < 0) spawnCount = 0;
            }
            else
            {
                spawnCount = baseSpawnCount;
            }

            for (int i = 0; i < spawnCount; i++)
            {
                float angle = (float)(rand.NextDouble() * MathHelper.TwoPi);
                float dist = (float)(rand.NextDouble() * Math.Max(npc.width, npc.height) * 0.6f);
                float npcSize = Math.Max(npc.width, npc.height);
                float dynamicMultiplier = MathHelper.Clamp(npcSize/ 400, 0.05f, 0.23f); 
                float size = (0.05f + (float)rand.NextDouble() * 0.1f) * MistSizeMultiplier * dynamicMultiplier;
                data.Particles.Add(new MistParticle
                {
                    Offset = new Vector2((float)Math.Cos(angle) * dist, (float)Math.Sin(angle) * dist),
                    Life = 0f,
                    MaxLife = 0.6f + (float)rand.NextDouble() * 0.4f,
                    Size = size,
                    Alpha = 0.8f
                });
            }

         
            for (int i = data.Particles.Count - 1; i >= 0; i--)
            {
                var p = data.Particles[i];
                p.Life += 1f / 60f;
                if (p.Life >= p.MaxLife)
                    data.Particles.RemoveAt(i);
                else
                    data.Particles[i] = p;
            }
        }

        public static bool IsWeakened(NPC npc)
            => npcWeakness.ContainsKey(npc.whoAmI) && npcWeakness[npc.whoAmI].RemainingTime > 0;

        public static float GetStrength(NPC npc)
        {
            if (!npcWeakness.ContainsKey(npc.whoAmI)) return 0f;
            var data = npcWeakness[npc.whoAmI];
            return data.RemainingTime > 0 ? data.Strength : 0f;
        }

        public static List<(NPC npc, WeaknessData data)> GetDrawList()
        {
            var list = new List<(NPC npc, WeaknessData data)>();
            foreach (var kv in npcWeakness)
            {
                if (kv.Value.RemainingTime > 0 && Main.npc[kv.Key].active)
                    list.Add((Main.npc[kv.Key], kv.Value));
            }
            return list;
        }
    }

  
    public class WeaknessGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override bool PreAI(NPC npc)
        {
            WeaknessSystem.Update(npc);
            return true;
        }

        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            float strength = WeaknessSystem.GetStrength(npc);
            if (strength > 0)
                modifiers.FinalDamage *= (1f - strength);
        }

        public override void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers)
        {
            float strength = WeaknessSystem.GetStrength(npc);
            if (strength > 0)
                modifiers.FinalDamage *= (1f - strength);
        }
    }

 
    public class WeaknessDrawSystem : ModSystem
    {
        private static DynamicVertexBuffer vertexBuffer;
        private static BasicEffect basicEffect;
        private static VertexPositionColorTexture[] quadVertices = new VertexPositionColorTexture[6];

        public override void Load() => Main.OnPostDraw += DrawMist;
        public override void Unload() => Main.OnPostDraw -= DrawMist;

        private void DrawMist(GameTime gameTime)
        {
            var drawList = WeaknessSystem.GetDrawList();
            if (drawList.Count == 0) return;

            GraphicsDevice device = Main.graphics.GraphicsDevice;
            BlendState oldBlend = device.BlendState;
            DepthStencilState oldDepth = device.DepthStencilState;
            RasterizerState oldRaster = device.RasterizerState;
            SamplerState oldSampler = device.SamplerStates[0];

            device.BlendState = BlendState.Additive;
            device.DepthStencilState = DepthStencilState.None;
            device.RasterizerState = RasterizerState.CullNone;

            foreach (var (npc, data) in drawList)
            {
                if (data.Particles.Count == 0) continue;
                Texture2D tex = null;
                try { tex = ModContent.Request<Texture2D>(data.MistTexture).Value; }
                catch { continue; }

                if (vertexBuffer == null)
                    vertexBuffer = new DynamicVertexBuffer(device, typeof(VertexPositionColorTexture), 6, BufferUsage.WriteOnly);
                if (basicEffect == null)
                {
                    basicEffect = new BasicEffect(device)
                    {
                        TextureEnabled = true,
                        VertexColorEnabled = true,
                        View = Matrix.Identity,
                        Projection = Matrix.Identity
                    };
                }

                basicEffect.World = Main.GameViewMatrix.TransformationMatrix;
                basicEffect.View = Matrix.Identity;
                basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);

                foreach (var particle in data.Particles)
                {
                    float progress = particle.Life / particle.MaxLife;
                    float alpha = particle.Alpha * (1f - progress);
                    if (alpha <= 0) continue;

                    Color col = new Color(data.MistColor.R, data.MistColor.G, data.MistColor.B, (int)(alpha * 255));
                    Vector2 screenPos = npc.Center + particle.Offset - Main.screenPosition;
                    float scale = particle.Size;
                    float halfW = tex.Width * 0.5f * scale;
                    float halfH = tex.Height * 0.5f * scale;

                    Vector3 topLeft = new Vector3(screenPos.X - halfW, screenPos.Y - halfH, 0);
                    Vector3 topRight = new Vector3(screenPos.X + halfW, screenPos.Y - halfH, 0);
                    Vector3 bottomLeft = new Vector3(screenPos.X - halfW, screenPos.Y + halfH, 0);
                    Vector3 bottomRight = new Vector3(screenPos.X + halfW, screenPos.Y + halfH, 0);

                    quadVertices[0] = new VertexPositionColorTexture(topLeft, col, new Vector2(0, 0));
                    quadVertices[1] = new VertexPositionColorTexture(bottomRight, col, new Vector2(1, 1));
                    quadVertices[2] = new VertexPositionColorTexture(topRight, col, new Vector2(1, 0));
                    quadVertices[3] = new VertexPositionColorTexture(topLeft, col, new Vector2(0, 0));
                    quadVertices[4] = new VertexPositionColorTexture(bottomLeft, col, new Vector2(0, 1));
                    quadVertices[5] = new VertexPositionColorTexture(bottomRight, col, new Vector2(1, 1));

                    vertexBuffer.SetData(quadVertices, 0, 6, SetDataOptions.Discard);
                    device.SetVertexBuffer(vertexBuffer);
                    basicEffect.Texture = tex;

                    foreach (var pass in basicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
                    }
                }
            }

            device.BlendState = oldBlend;
            device.DepthStencilState = oldDepth;
            device.RasterizerState = oldRaster;
            device.SamplerStates[0] = oldSampler;
        }
    }
}