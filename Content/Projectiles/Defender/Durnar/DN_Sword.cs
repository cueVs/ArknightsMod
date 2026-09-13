using ArknightsMod.Common.VisualEffects;
using ArknightsMod.Content.Items.Weapons.Defender.Beagle;
using ArknightsMod.Content.Items.Weapons.Defender.Durnar;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RuneSKill.Content.NeedTool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Defender.Durnar
{
    public class DN_Sword : ModProjectile
    {
        Player player => Main.player[Projectile.owner];
        Item item => player.HeldItem;
        private Texture2D SwordTex {get => TextureAssets.Projectile[ModContent.ProjectileType<DN_Sword>()].Value;}
        private float Length {get => MathF.Sqrt(MathF.Pow(SwordTex.Width,2) + MathF.Pow(SwordTex.Height,2));}
        private Vector2 DrawSetoff => new(3f, 0f);
        private Vector2 handPos;
        private Vector2[] oldHandpos = new Vector2[20];
        private Vector2[] oldPos = new Vector2[20];
        private float[] oldRot = new float[20];
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
            Projectile.localNPCHitCooldown = attackMaxTime+1;
        }

        public void ReloadArray() {
	        oldRot = new float[20];
	        oldPos = new Vector2[20];
	        oldHandpos = new Vector2[20];
        }
        private ProjMode projMode = ProjMode.Move;
		public override void AI()
        {
            if (player.dead || !player.active || item.type != ModContent.ItemType<DN_Weapon>()) Projectile.Kill();
            Projectile.timeLeft = 2;
            switch(projMode)
            {
                case ProjMode.Move:
                Move();
                break;
                case ProjMode.Attack:
                Attack();
                break;
            }
        }
        private int attackTime = 0;
        private int attackMaxTime = 30;
		public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.AnisotropicClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            switch(projMode)
            {
                case ProjMode.Move:
                Move_Draw(sb);
                break;
                case ProjMode.Attack:
                Attack_Draw(sb);
                break;
            }
            sb.End();
            sb.Begin();
            return false;
        }
        private float walkPhase;
        private float armOffsetDeg;
        private bool press = false;
        private float MouseRad;
        public void Move()
        {
            float vx = Math.Abs(player.velocity.X);
            bool isAirborne = Math.Abs(player.velocity.Y) > 0.01f;
            float targetOffsetDeg;
            if (isAirborne) {
                targetOffsetDeg = -20f; // 飞起来后固定
                walkPhase = 0f;
            }
            else if (vx > 0.1f) {
                walkPhase += 0.12f + vx * 0.04f;
                float progress = (MathF.Sin(walkPhase) + 1f) * 0.5f;
                targetOffsetDeg = MathHelper.Lerp(50f, -20f, progress);
            }
            else {
                walkPhase = 0f;
                targetOffsetDeg = 0f; // 落地停下回正
            }
            armOffsetDeg = MathHelper.ToRadians(targetOffsetDeg);
            float armRot = armOffsetDeg * -player.direction;
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armRot);
            handPos = player.GetBackHandPosition(Player.CompositeArmStretchAmount.Full, armRot);
            Projectile.rotation = armRot;
            float moveSwordRot = Projectile.rotation + MathHelper.PiOver2 * player.direction;
            Vector2 moveTip = GetSwordTipWorld(moveSwordRot);
            Projectile.Center = Vector2.Lerp(GetSwordBaseWorld(moveSwordRot), moveTip, 0.5f);
            if(Main.myPlayer == player.whoAmI)
            {
	            var modPlayer = player.GetModPlayer<DNProj_Player>();
                if(!modPlayer.ShieldAttackMode&&!Main.mouseRight&&PlayerInput.MouseInfo.LeftButton == ButtonState.Pressed&&!press)
                {
                    press = true;
                    MouseRad = MathF.Atan2((Main.MouseWorld - player.MountedCenter).Y,(Main.MouseWorld - player.MountedCenter).X);
                    projMode = ProjMode.Attack;
                    player.direction = (Main.MouseWorld - player.MountedCenter).X >=0? 1:-1;
                    attackTime = 0;
                }
            }
        }
        private Vector2 SwordEnd;
        private Vector2 GetSwordBaseWorld(float swordRotation) => handPos - DrawSetoff.RotatedBy(swordRotation);
        private Vector2 GetSwordTipWorld(float swordRotation) => GetSwordBaseWorld(swordRotation) + new Vector2(Length, 0f).RotatedBy(swordRotation);
        private void DrawSwordBody(float swordRotation, Color color)
        {
            Vector2 swordBase = GetSwordBaseWorld(swordRotation);
            SwordEnd = GetSwordTipWorld(swordRotation);
            Vector2 halfPos = (SwordEnd - swordBase) / 2f;
            Vector2 halfLength = new Vector2(-halfPos.Y, halfPos.X);
            Vector2 leftDown = swordBase - Main.screenPosition;
            Vector2[] drawPos =
            [
                leftDown + halfPos + halfLength,
                leftDown + 2 * halfPos,
                leftDown,
                leftDown + halfPos - halfLength
            ];
            List<Vertex> vertices = new List<Vertex>();
            for (int i = 0; i < 6; i++)
                vertices.Add(default);
            {
                vertices[0] = new Vertex(drawPos[0], new Vector3(0, 0, 1), color);
                vertices[1] = vertices[5] = new Vertex(drawPos[1], new Vector3(1, 0, 1), color);
                vertices[2] = vertices[4] = new Vertex(drawPos[2], new Vector3(0, 1, 1), color);
                vertices[3] = new Vertex(drawPos[3], new Vector3(1, 1, 1), color);
            }
            Main.graphics.GraphicsDevice.Textures[0] = SwordTex;
            if (vertices.Count > 4)
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count / 3);
        }
        public void Move_Draw(SpriteBatch sb)
        {
            float setoff = player.direction == 1? 0: MathHelper.Pi;
            DrawSwordBody(setoff + Projectile.rotation, Color.White);
        }
        private void SaveData(Vector2 pos,float rot,Vector2 handPos)
        {
            for(int i=oldPos.Length-1;i>0;i--)
            {
                oldPos[i] = oldPos[i-1];
                oldRot[i] = oldRot[i-1];
                oldHandpos[i] = oldHandpos[i-1];
            }
            oldHandpos[0] = handPos;
            oldPos[0] = pos;
            oldRot[0] = rot;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
	        var modPlayer = player.GetModPlayer<WeaponPlayer>();
	        if (modPlayer.SkillActive)
		        modifiers.SourceDamage *= 1.8f;
        }

        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
	        var modPlayer = player.GetModPlayer<WeaponPlayer>();
	        if (modPlayer.SkillActive)
		        modifiers.SourceDamage *= 1.8f;
        }

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            switch(projMode)
            {
                case ProjMode.Move:
                return false;
                case ProjMode.Attack:
                bool hit = Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                handPos,
                SwordEnd,
                20f,
                ref point);
                return hit;
            }
            return false;
        }
        public void Attack()
        {
            player.itemTime = player.itemAnimation = Projectile.timeLeft = 2;
            float swingRot;
            float armRot;
            if (player.direction == 1)
            {
                swingRot = RotationHelper.GetSwingRotation(
                    MouseRad + MathHelper.ToRadians(90f),
                    MouseRad + MathHelper.ToRadians(-60f),
                    attackTime,
                    attackMaxTime,
                    1,
                    RotationHelper.SwingDir.plus
                );
                armRot = swingRot + MathHelper.ToRadians(90f);
            }
            else
            {
                swingRot = RotationHelper.GetSwingRotation(
                    MouseRad + MathHelper.ToRadians(-90f),
                    MouseRad + MathHelper.ToRadians(60f),
                    attackTime,
                    attackMaxTime,
                    1,
                    RotationHelper.SwingDir.plus
                );
                Vector2 dir = new Vector2(1f, 0f).RotatedBy(swingRot);
                armRot = dir.ToRotation() + MathHelper.PiOver2;
            }

            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, armRot);
            handPos = player.GetBackHandPosition(Player.CompositeArmStretchAmount.Full, armRot);
            Projectile.rotation = player.direction == 1
                ? armRot + MathHelper.ToRadians(160f)
                : armRot - MathHelper.ToRadians(160f);
            SwordEnd = GetSwordTipWorld(Projectile.rotation+ MathHelper.ToRadians(-90f));
            Projectile.Center = Vector2.Lerp(GetSwordBaseWorld(Projectile.rotation), SwordEnd, 0.5f);
            SaveData(SwordEnd,Projectile.rotation,GetSwordBaseWorld(Projectile.rotation));
            attackTime++;
            if(attackTime > attackMaxTime)
            {
                projMode = ProjMode.Move;
                ReloadArray();
                press = false;
            }
        }
        private void Attack_Draw(SpriteBatch sb)
        {
            #region 拖尾绘制
            List<Vertex> tail = new List<Vertex>();
            for(int i=0;i<oldPos.Length;i++)
            {
                if(oldPos[i] != Vector2.Zero)
                {
                    float progress = (float)i/(float)oldPos.Length;
                    tail.Add(new Vertex(oldHandpos[i] - Main.screenPosition,new Vector3(progress,0,1),Color.Purple));
                    tail.Add(new Vertex(oldPos[i] - Main.screenPosition,new Vector3(progress,1,1),Color.Purple));
                }
            }
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                SamplerState.AnisotropicClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Defender/Durnar/Extra_209").Value;
            if(tail.Count>=3)
                Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, tail.ToArray(), 0,tail.Count -2);
            #endregion
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.AnisotropicClamp, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            #region 剑体绘制
            DrawSwordBody(Projectile.rotation - MathHelper.Pi/2, Color.White);
            #endregion
        }
    }
}
