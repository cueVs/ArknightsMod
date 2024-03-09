using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Dusts.Bosses
{
	public class FrostNovaDeathDust : ModDust
	{
		public override void OnSpawn(Dust dust) {
			//dust.velocity *= 0.4f; // Multiply the dust's start velocity by 0.4, slowing it down
			dust.noGravity = true; // Makes the dust have no gravity.
			dust.noLight = true; // Makes the dust emit no light.
			dust.scale *= 1f;
			dust.frame = new Rectangle(0, Main.rand.Next(3) * 14, 14, 14);
			dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
		}

		public override bool Update(Dust dust) { // Calls every frame the dust is active
			dust.scale *= 0.99f;
			dust.velocity *= 0.99f;
			dust.position += dust.velocity;
			if (dust.scale < 0.1f) {
				dust.active = false;
			}

			return false; // Return false to prevent vanilla behavior.
		}
	}
}
