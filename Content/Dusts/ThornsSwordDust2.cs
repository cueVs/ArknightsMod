using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Dusts
{
	public class ThornsSwordDust2 : ModDust
	{
		public override void OnSpawn(Dust dust)
		{
			dust.velocity *= 0.4f; // Multiply the dust's start velocity by 0.4, slowing it down
			dust.noGravity = false; // Makes the dust have no gravity.
			dust.noLight = true; // Makes the dust emit no light.
			dust.scale *= 1.5f; // Multiplies the dust's initial scale by 1.5.
			// If our texture had 3 different dust on top of each other (a 20x60 pixel image), we might do this:
			dust.frame = new Rectangle(0, Main.rand.Next(3) * 20, 20, 20);
		}

		public override bool Update(Dust dust)
		{ // Calls every frame the dust is active
			dust.position += dust.velocity;
			dust.rotation += dust.velocity.X * 0.15f;
			dust.scale *= 0.96f;

			if (dust.scale < 0.5f)
			{
				dust.active = false;
			}

			return false; // Return false to prevent vanilla behavior.
		}
	}
}
