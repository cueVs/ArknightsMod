using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace ArknightsMod.Common.Particle
{
	public class ParticleManager : ModSystem
	{
		public override void OnModLoad()
		{
			if (Main.netMode == 2)
			{
				return;
			}
			ParticleManager.LoadParticles(base.Mod.Code);
			On_Main.DrawInfernoRings += new On_Main.hook_DrawInfernoRings(this.DrawParticlesDetour);
		}

		private void DrawParticlesDetour(On_Main.orig_DrawInfernoRings orig, Main self)
		{
			ParticleManager.DrawParticles();
			orig.Invoke(self);
		}

		private static void DrawParticles()
		{
			if (!ParticleManager.activeParticles.Any())
			{
				return;
			}
			Main.spriteBatch.End();
			foreach (IGrouping<BlendState, Particle> blendGroup in from p in ParticleManager.activeParticles
			group p by p.DrawBlendState)
			{
				RasterizerState screenCull = Main.Rasterizer;
				Main.spriteBatch.Begin(0, blendGroup.First().DrawBlendState, Main.DefaultSamplerState, DepthStencilState.None, screenCull, null, Main.GameViewMatrix.TransformationMatrix);
				foreach (Particle particle in blendGroup)
				{
					particle.Draw();
				}
				Main.spriteBatch.End();
			}
            Main.spriteBatch.Begin(0, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		}

		public override void PostUpdateDusts()
		{
			ParticleManager.activeParticles.RemoveAll((Particle p) => p.Time >= p.Lifetime);
			int particleCount = ParticleManager.activeParticles.Count;
			for (int i = 0; i < particleCount; i++)
			{
				ParticleManager.activeParticles[i].Time++;
				ParticleManager.activeParticles[i].Update();
				ParticleManager.activeParticles[i].Position += ParticleManager.activeParticles[i].Velocity;
			}
		}

        public static void LoadParticles(Assembly assembly)
        {
            int currentParticleID = 0;
            Dictionary<Type, int> dictionary = ParticleManager.particleIDLookup;
            if (dictionary != null && dictionary.Any())
            {
                currentParticleID = ParticleManager.particleIDLookup.Values.Max() + 1;
            }
            foreach (Type particleType in AssemblyManager.GetLoadableTypes(assembly))
            {
                if (particleType.IsSubclassOf(typeof(Particle)) && !particleType.IsAbstract)
                {
                    Particle particle = (Particle)RuntimeHelpers.GetUninitializedObject(particleType);
                    ParticleManager.particleIDLookup[particleType] = currentParticleID;
                    Texture2D particleTexture = ModContent.Request<Texture2D>(particle.TexturePath, AssetRequestMode.ImmediateLoad).Value;
                    ParticleManager.particleTextureLookup[currentParticleID] = particleTexture;
                    particle.Load();
                    currentParticleID++;
                }
            }
        }


        internal static readonly List<Particle> activeParticles = [];

		internal static readonly Dictionary<Type, int> particleIDLookup = [];

		internal static readonly Dictionary<int, Texture2D> particleTextureLookup = [];
	}
}
