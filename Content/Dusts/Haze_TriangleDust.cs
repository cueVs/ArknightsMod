using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Dusts
{
	public class Haze_TriangleDust : ModDust
	{
		private static Random random = new Random();
		private static Color[] colors = new Color[]
		{
			new Color(55, 49, 181),
			new Color(22, 173, 254),
			new Color(140, 238, 255)
		};

		public override void OnSpawn(Dust dust) {
			dust.noGravity = true;
			dust.frame = new Rectangle(0, 0, 32, 32);
			dust.noLight = true;

			int index = random.Next(colors.Length);
			dust.color = colors[index];
		}

		public override bool Update(Dust dust) {
			dust.position += dust.velocity;
			dust.velocity *= 0.8f;
			dust.rotation += dust.velocity.X * 0.15f;
			dust.scale *= 0.95f;

			float light = 0.2f * dust.scale;
			Lighting.AddLight(dust.position, light, light, light);

			if (dust.scale < 0.2f)
				dust.active = false;

			return false;
		}

		public override bool PreDraw(Dust dust) {
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			Color color = dust.color * dust.scale;

			Main.spriteBatch.Draw(
				ModContent.Request<Texture2D>("ArknightsMod/Content/Dusts/Haze_TriangleDust").Value,
				dust.position - Main.screenPosition,
				dust.frame,
				color,
				dust.rotation,
				dust.frame.Size() * 0.5f,
				dust.scale,
				SpriteEffects.None,
				0f
			);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}
	}
}