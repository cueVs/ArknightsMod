using ArknightsMod.Common.VisualEffects;
using ArknightsMod.Content.Projectiles.Defender;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.SwingHelper
{
    public sealed class ShieldHelper
    {
        private readonly Vertex[] vertices = new Vertex[6];
        private float walkPhase;
        public Defender_Player mp;
        public void SetDefaults(Projectile projectile, int localNpcHitCooldown = 0)
        {
            projectile.width = 60;
            projectile.height = 36;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.usesLocalNPCImmunity = true;
            projectile.ownerHitCheck = true;
            projectile.DamageType = DamageClass.MeleeNoSpeed;
            projectile.ignoreWater = true;
            projectile.localNPCHitCooldown = localNpcHitCooldown;
        }

        public void UpdateMovePose(Projectile projectile, Player player)
        {
            float speedX = Math.Abs(player.velocity.X);
            float targetOffsetDegrees;

            if (Math.Abs(player.velocity.Y) > 0.01f)
            {
                walkPhase = 0f;
                targetOffsetDegrees = 20f;
            }
            else if (speedX > 0.1f)
            {
                walkPhase += 0.12f + speedX * 0.04f;
                float progress = (MathF.Sin(walkPhase) + 1f) * 0.5f;
                targetOffsetDegrees = player.direction == 1
                    ? MathHelper.Lerp(50f, -20f, progress)
                    : MathHelper.Lerp(-20f, 50f, progress);
            }
            else
            {
                walkPhase = 0f;
                targetOffsetDegrees = 0f;
            }

            mp = player.GetModPlayer<Defender_Player>();
            player.heldProj = projectile.whoAmI;
            float targetRotation = MathHelper.ToRadians(targetOffsetDegrees);
            float armRotation = targetRotation * -player.direction;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
            projectile.rotation = armRotation;
            projectile.Center = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, armRotation);
        }

        public void UpdateDefenderPose(Projectile projectile, Player player)
        {
	        mp = player.GetModPlayer<Defender_Player>();
	        mp.OpenDefender = true;
            player.heldProj = projectile.whoAmI;
            player.direction = Main.MouseWorld.X >= player.MountedCenter.X ? 1 : -1;
            float mouseRotation = (Main.MouseWorld - player.MountedCenter).ToRotation();
            projectile.rotation = mouseRotation - MathHelper.PiOver2 + MathHelper.PiOver2 * player.direction;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, projectile.rotation);
            projectile.Center = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, projectile.rotation)
                + new Vector2(20f, 0f).RotatedBy(projectile.rotation) * player.direction;
        }

        public void DrawShield(Projectile projectile, Player player, Texture2D texture, bool defending)
        {
            Vector2 center = projectile.Center - Main.screenPosition;
            Vector2 halfWidth = new Vector2(texture.Width * 0.5f, 0f).RotatedBy(projectile.rotation);
            Vector2 halfHeight = new Vector2(0f, texture.Height * 0.5f).RotatedBy(projectile.rotation);
            if (defending)
                halfWidth *= 0.8f;

            Vector2[] pos =
            {
                center + halfHeight + halfWidth * player.direction,
                center + halfHeight - halfWidth * player.direction,
                center - halfHeight - halfWidth * player.direction,
                center - halfHeight + halfWidth * player.direction
            };

            vertices[0] = new Vertex(pos[1], new Vector3(0f, 1f, 1f), Color.White);
            vertices[1] = vertices[5] = new Vertex(pos[0], new Vector3(1f, 1f, 1f), Color.White);
            vertices[2] = vertices[4] = new Vertex(pos[2], new Vector3(0f, 0f, 1f), Color.White);
            vertices[3] = new Vertex(pos[3], new Vector3(1f, 0f, 1f), Color.White);

            Main.graphics.GraphicsDevice.Textures[0] = texture;
            Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertices.Length / 3);
        }
    }
}
