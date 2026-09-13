using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Dusts
{
	public class Haze_CircleDust : ModDust
	{
		public override void OnSpawn(Dust dust) {
			dust.noGravity = true;
			dust.frame = new Rectangle(0, 0, 36, 36);
			dust.noLight = true;
		}

		public override bool Update(Dust dust) {
			dust.position += dust.velocity;
			dust.velocity *= 0.8f;
			dust.scale *= 0.92f;
			
			float light = 0.2f * dust.scale;
			Lighting.AddLight(dust.position, light, light, light);

			if (dust.scale < 0.2f)
				dust.active = false;

			return false;
		}

		public override bool PreDraw(Dust dust) {
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			Main.spriteBatch.Draw(
				ModContent.Request<Texture2D>("ArknightsMod/Content/Dusts/Haze_CircleDust").Value,
				dust.position - Main.screenPosition,
				dust.frame,
				Color.Black * dust.scale,
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
