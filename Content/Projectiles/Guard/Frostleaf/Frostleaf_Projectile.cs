using ArknightsMod.Content.Items.Weapons.Guard.Chen;
using ArknightsMod.Content.Items.Weapons.Guard.Frostleaf;
using ArknightsMod.Content.SwingHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ModLoader;
using Vertex = ArknightsMod.Content.SwingHelper.Vertex;


namespace ArknightsMod.Content.Projectiles.Guard.Frostleaf
{
	public class Frostleaf_Projectile : ModProjectile
	{
		private Player player=> Main.player[Projectile.owner];
		private Vector2[] oldPos = new Vector2[16];
		private enum ProjMode{Attack,Buff}
		private ProjMode mode = ProjMode.Attack;
		public override void SetDefaults() {
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.ownerHitCheck = true;
			Projectile.DamageType = DamageClass.MeleeNoSpeed;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 60;
			Projectile.aiStyle = -1;
		}

		private void SavePos(Vector2 Pos) {
			for (int i =15; i > 0; i--)
			{
				oldPos[i] = oldPos[i - 1];
			}
			oldPos[0] = Pos;
		}
		public Vector2 mousePosition;
		public float projRotation;
		public override void OnSpawn(IEntitySource source) {
			mousePosition = Main.MouseWorld - player.Center;
			projRotation = MathF.Atan2(mousePosition.Y, mousePosition.X);
		}


		public override void AI() {
			Projectile.velocity = mousePosition.SafeNormalize(Vector2.Zero) * 12;
			Projectile.rotation = projRotation + MathF.PI;
			SavePos(Projectile.Center);
		}
		public Texture2D tex => TextureAssets.Projectile[Projectile.type].Value;
		public Vector2 TexWidth => new Vector2(TextureAssets.Projectile[Projectile.type].Value.Height, 0);
		public List<Vertex> trip = new List<Vertex>();
		public Effect noise => SwingHelper.SwingHelper.NoiseTrail;
		public override bool PreDraw(ref Color lightColor) {
			SpriteBatch sb = Main.spriteBatch;
			trip.Clear();
			sb.End();
			sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
				SamplerState.AnisotropicClamp, DepthStencilState.None,
				RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			sb.Draw( tex,Projectile.Center - Main.screenPosition,null,lightColor,Projectile.rotation, (tex.Size())/2
				,new Vector2(1,1),SpriteEffects.None,0);
				sb.End();
				sb.Begin();
			// 	Main.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
			// 	for (int i = 0; i < oldPos.Length; i++) {
			// 		if (oldPos[i] != Vector2.Zero) {
			// 			float progress = i / (float)oldPos.Length;
			// 			trip.Add(new Vertex(oldPos[i]  - Main.screenPosition + (TexWidth / 3).RotatedBy(Projectile.rotation + MathF.PI/2), new Vector3(progress, 0, 0),lightColor * (1-progress)));
			// 			trip.Add(new Vertex(oldPos[i]  - Main.screenPosition - (TexWidth / 3).RotatedBy(Projectile.rotation + MathF.PI/2), new Vector3(progress, 1, 0),lightColor * (1-progress)));
			// 		}
			// 		Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>("ArknightsMod/Content/SwingHelper/Images/Hz").Value;
			// 		if (trip.Count >= 3)
			// 			Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, trip.ToArray(), 0,
			// 				trip.Count - 2);
			// }
			return false;
		}
	}
}