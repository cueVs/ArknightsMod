using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace ArknightsMod.Content.Dusts
{
    public class Orchid_Dust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = true;
            dust.frame = new Rectangle(0, RandomFrame() * 14, 14, 14);
            dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            dust.alpha = 96;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity *= 0.9f;
            dust.scale *= 0.9f;

            Lighting.AddLight(dust.position, 0.13f * dust.scale, 0.38f * dust.scale, 0.93f * dust.scale);

            if (dust.scale < 0.1f)
            {
                dust.active = false;
            }

            return false;
        }

        private int RandomFrame()
        {
            int result = Main.rand.Next(5);

            if (result == 0 || result == 3 || result == 4)
            {
                result = 0;
            }

            return result;//此时粒子纹理的第一帧具有较高权重
        }
    }
}
