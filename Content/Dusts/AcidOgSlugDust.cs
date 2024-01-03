using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Dusts
{
	public class AcidOgSlugDust : ModDust
	{
		public override void OnSpawn(Dust dust) {
			dust.velocity *= 0.4f; // Multiply the dust's start velocity by 0.4, slowing it down
			dust.noGravity = true; // Makes the dust have no gravity.
			dust.noLight = true; // Makes the dust emit no light.
			dust.scale *= 0.6f; // Multiplies the dust's initial scale by 1.5.
							  // If our texture had 3 different dust on top of each other (a 20x60 pixel image), we might do this:
			dust.frame = new Rectangle(0, Main.rand.Next(3) * 30, 28, 30);
		}

		public override bool Update(Dust dust) { // Calls every frame the dust is active
			dust.scale *= 0.9f;

			if (dust.scale < 0.3f) {
				dust.active = false;
			}

			return false; // Return false to prevent vanilla behavior.
		}
	}
}
