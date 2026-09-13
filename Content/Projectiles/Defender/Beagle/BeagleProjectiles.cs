using ArknightsMod.Content.Items.Weapons.Defender.Beagle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Defender.Beagle
{

	public class MGL_Player : ModPlayer
	{
		public override void PostUpdate() {
			var it = Player.HeldItem;
			if (it.type == ModContent.ItemType<BeagleWeapon>()) {
				if (Player.ownedProjectileCounts[ModContent.ProjectileType<MGL_Sword>()] == 0) {
					Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<MGL_Sword>(), it.damage, it.knockBack, Player.whoAmI);
				}
			}

			base.PostUpdate();
		}
		public override void UpdateEquips() {
			var it = Player.HeldItem;
			if (it.type == ModContent.ItemType<BeagleWeapon>() && Main.mouseRight) {
				Player.noKnockback = true;
			}
			base.UpdateEquips();
		}
		public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers) {/*
            var it = Player.HeldItem;
            if (it.type == ModContent.ItemType<MGL>() && Main.mouseRight)
                if ((npc.Center.X - Player.Center.X) * Player.direction >= 0)
                {
                    var defense = Player.statDefense;
                    var v = 0.5f;
                    v += Main.expertMode ? 0.25f : 0;
                    v += Main.masterMode ? 0.25f : 0;
                    Main.NewText(v);
                    var dam1 = modifiers.GetDamage(npc.damage, defense * 1f, v);
                    var dam2 = modifiers.GetDamage(npc.damage, defense * 1.5f, v);

                    modifiers.FinalDamage *= dam2 / dam1;
                }
            */
			base.ModifyHitByNPC(npc, ref modifiers);
		}
		public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers) {
			/*
            var it = Player.HeldItem;
            if (it.type == ModContent.ItemType<MGL>() && Main.mouseRight)
                if ((proj.Center.X - Player.Center.X) * Player.direction >= 0)
                {
                    var defense = Player.statDefense;
                    var v = 0.5f;
                    v += Main.expertMode ? 0.25f : 0;
                    v += Main.masterMode ? 0.25f : 0;
                    Main.NewText(v);
                    var dam1 = modifiers.GetDamage(proj.damage, defense * 1f, v);
                    var dam2 = modifiers.GetDamage(proj.damage, defense * 1.5f, v);

                    modifiers.FinalDamage *= dam2 / dam1;
                }
            */
			base.ModifyHitByProjectile(proj, ref modifiers);
		}
		public static void QuicklyDraw_Proj(Projectile proj, float? scale = null, Color? col = null, float? rotation = null, Vector2? Center = null, Texture2D tx = null, SpriteEffects? spE = null, Vector2? Ori = null) {
			Player player = Main.player[proj.owner];
			Texture2D TX = tx == default ? TextureAssets.Projectile[proj.type].Value : tx;
			Color Col = !col.HasValue ? Lighting.GetColor((int)(player.Center.X / 16f), (int)(player.Center.Y / 16f)) : col.Value;


			if (player != null) {
				float sc = !scale.HasValue ? 1 : scale.Value;
				SpriteEffects spe = !spE.HasValue ? player.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None : spE.Value;
				float Ro = !rotation.HasValue ? proj.rotation - MathHelper.PiOver4 * (spe == SpriteEffects.None ? 1 : -1) : rotation.Value - MathHelper.PiOver4 * (spe == SpriteEffects.None ? 1 : -1);

				float Dir = spe == SpriteEffects.None ? 1 : -1;
				Vector2 Cent = !Center.HasValue ? proj.Center : Center.Value;

				var ori = !Ori.HasValue ? new Vector2((TX.Width / 2 - TX.Width / 2 * Dir), TX.Height) : Ori.Value;
				Main.spriteBatch.Draw(TX,
				  Cent - Main.screenPosition,
				  null,
				  Col,
				  Ro,
				  ori,
				  sc,
				  spe,
				  0);
			}
		}

	}
	public class MGL_Sword : ModProjectile
    {
		Player player => Main.player[Projectile.owner];
        Item item => player.HeldItem;
        public override void SetDefaults()
        {
            Projectile.width = 10; // ?�������?�����
            Projectile.height = 10; // ?�������?��?�
            Projectile.friendly = true; // ?������?��?���
            Projectile.penetrate = -1; // ?�������?�?
            Projectile.tileCollide = false; // ?���?����?��?
            Projectile.usesLocalNPCImmunity = true; // ?��?�����?
            Projectile.ownerHitCheck = true; // ?��?�����?���������?�����??�?�����?�?��?����?�?
            Projectile.DamageType = DamageClass.MeleeNoSpeed; // ?����?��??����
            Projectile.ignoreWater = true;
            Projectile.localNPCHitCooldown = 1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            {
                var p = Projectile.NewProjectileDirect(source, player.Center, Vector2.Zero, ModContent.ProjectileType<MGL_Shield>(), 0,0, player.whoAmI);
                p.ai[2] = Projectile.whoAmI;
            }

            base.OnSpawn(source);
        }
        public override bool ShouldUpdatePosition()
        {
            return false;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;//��?�??2���?���?��?�?�������������?��
            ProjectileID.Sets.TrailCacheLength[Type] = 7;//��?���������??�����?�?�����?��?(?�����??����)
        }
        Stack<NPC> RecordNPC = new Stack<NPC>();
        public override bool? CanHitNPC(NPC target)
        {
            if (target.townNPC)
                return false; // ???? NPC
            return !RecordNPC.Contains(target) && Projectile.ai[1] > 0;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            RecordNPC.Push(target);
            base.OnHitNPC(target, hit, damageDone);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 start = player.MountedCenter;
            Vector2 end = start + (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * Projectile.scale * 60;
            float collisionPoint = 0f;
            bool coll = Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(),
                                                     targetHitbox.Size(),
                                                     start,
                                                     end,
                                                     24,
                                                     ref collisionPoint);
            return coll;
        }
        public override void AI()
        {
            Projectile.velocity = -Vector2.UnitY.RotatedBy(Projectile.rotation);
            if (player.dead || !player.active || item.type != ModContent.ItemType<BeagleWeapon>()) Projectile.Kill();
            if (Projectile.ai[1] <= 0)
            {
                var LerpVal = Math.Clamp(Projectile.ai[0] / 40f, 0, 1);

                var speed = Math.Abs(player.velocity.X) * Math.Clamp(0.0000001 - Math.Abs(player.velocity.Y), 0f, 1f) * 10000000f;
                Projectile.localAI[0] += Math.Abs(player.velocity.X);
                if (Main.mouseRight)
                    Projectile.localAI[1] = MathHelper.Lerp(Projectile.localAI[1], 0.4f * player.direction, 0.08f);
                else
                Projectile.localAI[1] = MathHelper.Lerp(Projectile.localAI[1], (float)(Math.Sin(Projectile.localAI[0] * 0.04f) * speed) * 0.2f, 0.07f);
                Projectile.rotation = Projectile.rotation.AngleLerp(MathHelper.PiOver2 * player.direction + Projectile.localAI[1], LerpVal);
                Projectile.Center = Vector2.Lerp(Projectile.Center, player.MountedCenter + new Vector2(0, player.gfxOffY) + new Vector2(0, -9).RotatedBy(Projectile.rotation + MathHelper.PiOver2 * player.direction), LerpVal);
                player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2 * player.direction);

                Projectile.ai[0]++;

                if(player.controlUseItem && !Main.mouseRight)
                {
                    RecordNPC.Clear();
                    Projectile.ai[0] = 0;
                    Projectile.ai[1] = 28;
                    player.direction = Main.MouseWorld.X > player.Center.X ? 1 : -1;
                    player.itemAnimation = player.itemTime = 4;
                    Projectile.rotation = -0.3f * player.direction;
                    for (int i = 0; i < Projectile.oldRot.Length; i++)
                        Projectile.oldRot[i] = Projectile.rotation;


                }
            }
            else
            {
                var LerpVal = Math.Clamp((28f - Projectile.ai[1]) / 40f, 0, 1);
                player.itemAnimation = player.itemTime = 4;

                Projectile.Center = player.MountedCenter + new Vector2(0, -7).RotatedBy(Projectile.rotation);
                Projectile.rotation = MathHelper.Lerp(Projectile.rotation, 2.8f * player.direction, LerpVal);

                player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation + MathHelper.Pi);
                Projectile.ai[1]--;

            }
            base.AI();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.ai[1] > 0)
            {
                var c = Color.Gray * (lightColor.ToVector3().Length() / 1.76f) * 0.5f;
                c.A = 0;
                for (int i = 1; i < Projectile.oldRot.Length; i++)
                    for (float j = 0; j < 1; j += 0.2f)
                    {
                        var ro = MathHelper.Lerp(Projectile.oldRot[i], Projectile.oldRot[i - 1], j);
                        MGL_Player.QuicklyDraw_Proj(Projectile, rotation: ro, col:c);
                    }
            }
            MGL_Player.QuicklyDraw_Proj(Projectile);
            return false;
        }
    }
    public class MGL_Shield : ModProjectile
    {
        Player player => Main.player[Projectile.owner];
        Item item => player.HeldItem;
        Projectile proj => Main.projectile[(int)Projectile.ai[2]];

        public override void SetDefaults()
        {
            Projectile.hide = true;
            Projectile.width = 10; // ?�������?�����
            Projectile.height = 10; // ?�������?��?�
            Projectile.friendly = true; // ?������?��?���
            Projectile.penetrate = -1; // ?�������?�?
            Projectile.tileCollide = false; // ?���?����?��?
            Projectile.usesLocalNPCImmunity = true; // ?��?�����?
            Projectile.ownerHitCheck = true; // ?��?�����?���������?�����??�?�����?�?��?����?�?
            Projectile.DamageType = DamageClass.MeleeNoSpeed; // ?����?��??����
            Projectile.ignoreWater = true;
        }
        public override bool ShouldUpdatePosition()
        {
            return false;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
            base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
        }
        public override void AI()
        {
            if (player.dead || !player.active || item.type != ModContent.ItemType<BeagleWeapon>()) Projectile.Kill();
            if (Projectile.ai[1] <= 0)
            {
                var LerpVal = Math.Clamp(Projectile.ai[0] / 40f, 0, 1);

                Projectile.rotation = Projectile.rotation.AngleLerp(0.2f * player.direction, LerpVal);
                Projectile.Center = Vector2.Lerp(Projectile.Center, player.MountedCenter + new Vector2(-5 * player.direction, 6 + player.gfxOffY) , LerpVal);
                Projectile.ai[0]++;
                if (proj.ai[1] <= 0 && Main.mouseRight)
                {
                    Projectile.ai[0] = 0;
                    Projectile.ai[1] += 0.1f;
                    player.direction = Main.MouseWorld.X > player.Center.X ? 1 : -1;

                }
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, 0);

            }
            else
            {
                player.itemAnimation = player.itemTime = 4;

                if (proj.ai[1] <= 0 && Main.mouseRight)
                {
                    Projectile.ai[1] = MathHelper.Lerp(Projectile.ai[1], 1, 0.07f);
                }
                else
                {
                    Projectile.ai[1] -= 0.1f;
                }
                Projectile.rotation = Projectile.rotation.AngleLerp(0, Projectile.ai[1]);
                Projectile.Center = Vector2.Lerp(Projectile.Center, player.MountedCenter + new Vector2(12 * player.direction, 0 + player.gfxOffY), Projectile.ai[1]);

                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, -MathHelper.PiOver2 * player.direction);

            }
            base.AI();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            var t = TextureAssets.Projectile[Type].Value;
            Main.spriteBatch.Draw(t, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, t.Size() * 0.5f, 0.9f, default, 0);
            return false;
        }

    }
}