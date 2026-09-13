using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Sniper.KroosAlter
{
    public class KroosAlterCrossbow_Effect : ModProjectile
    {
        public override string Texture => "Terraria/Images/Extra_98";
        private float Skill => (int)Projectile.ai[2];
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 8;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 10;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.RotatedBy(MathHelper.PiOver2).ToRotation();

            Projectile.velocity -= Projectile.velocity * 0.1f;
            Projectile.Opacity -= 0.1f;

            Lighting.AddLight(Projectile.Center, 0.35f, 0.93f, 0.81f);

            base.AI();
        }
        public override bool PreDraw(ref Color lightColor)
        {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 position = Projectile.Center - Main.screenPosition;

            Color color;
            if (modPlayer.Skill == 0 && modPlayer.SkillActive)
            {
                color = new Color(204, 126, 57, 0) * Projectile.Opacity;
            }
            else if (modPlayer.Skill == 1 && modPlayer.SkillActive)
            {
                color = new Color(9, 161, 130, 0) * Projectile.Opacity;
            }
            else
            {
                color = Color.DarkOrange * Projectile.Opacity;
                color.A = 0;
            }

            Vector2 scale = Projectile.Size / texture.Size() * Projectile.scale * new Vector2(3, Main.rand.NextFloat(4, 6));

            Main.EntitySpriteDraw(texture,
                position,
                null,
                color,
                Projectile.rotation,
                origin,
                scale,
                SpriteEffects.None,
                0
            );

            return false;
        }
    }

    public class KroostheKeenGlint_Crossbow_Circle1 : ModProjectile
    {
        public override string Texture => "Terraria/Images/Extra_174";
        ref float opa => ref Projectile.ai[0];
        ref float sca => ref Projectile.ai[1];
        private float Skill => (int)Projectile.ai[2];
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 10;
            Projectile.Opacity = 0;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            if (Projectile.timeLeft > 5)
            {
                opa += 0.2f;
            }
            else
            {
                opa -= 0.2f;
            }
            sca += 0.2f;

            Projectile.Opacity = opa;
            Projectile.scale = sca;

            Projectile.velocity *= 0.9f;
            Projectile.rotation = Projectile.velocity.ToRotation();

            base.AI();
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 position = Projectile.Center - Main.screenPosition;

            Color color;
            if (modPlayer.Skill == 0 && modPlayer.SkillActive)
            {
                color = new Color(204, 126, 57, 0) * Projectile.Opacity;
            }
            else if (modPlayer.Skill == 1 && modPlayer.SkillActive)
            {
                color = new Color(9, 161, 130, 0) * Projectile.Opacity;
            }
            else
            {
                color = Color.DarkOrange * Projectile.Opacity;
                color.A = 0;
            }

            Vector2 scale = Projectile.Size / texture.Size() * Projectile.scale * new Vector2(0.2f, 1f);

            Main.EntitySpriteDraw(texture,
                position,
                null,
                color,
                Projectile.rotation,
                origin,
                scale,
                SpriteEffects.None,
                0
            );

            return false;
        }
    }

    public class KroostheKeenGlint_Crossbow_Circle2 : ModProjectile
    {
        public override string Texture => "Terraria/Images/Extra_174";
        ref float opa => ref Projectile.ai[0];
        ref float sca => ref Projectile.ai[1];
        private float Skill => (int)Projectile.ai[2];
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 10;
            Projectile.Opacity = 0;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            if (Projectile.timeLeft > 5)
            {
                opa += 0.2f;
            }
            else
            {
                opa -= 0.2f;
            }
            sca += 0.1f;

            Projectile.Opacity = opa;
            Projectile.scale = sca;

            base.AI();
        }
        public override bool PreDraw(ref Color lightColor)
        {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 position = Projectile.Center - Main.screenPosition;

            Color color;
            if (modPlayer.Skill == 0 && modPlayer.SkillActive)
            {
                color = new Color(204, 126, 57, 0) * Projectile.Opacity;
            }
            else if (modPlayer.Skill == 1 && modPlayer.SkillActive)
            {
                color = new Color(9, 161, 130, 0) * Projectile.Opacity;
            }
            else
            {
                color = Color.DarkOrange * Projectile.Opacity;
                color.A = 0;
            }

            Vector2 scale = Projectile.Size / texture.Size() * Projectile.scale;

            Main.EntitySpriteDraw(texture,
                position,
                null,
                color,
                Projectile.rotation,
                origin,
                scale,
                SpriteEffects.None,
                0
            );

            return false;
        }
    }
}
