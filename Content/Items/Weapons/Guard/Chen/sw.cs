using ArknightsMod.Content.Tiles.Infrastructure;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Guard.Chen
{ 
	public enum ChenSword_Style {
		Normal = 0,
		Skill_1 = 1,
		Skill_2 = 2,
		Skill_3 = 3
	}
	public class ChenSword_Item : UpgradeWeaponBase
	{
        public override void SetDefaults()
		{
            Item.damage = 122;
            Item.useAnimation = 39;
            Item.useTime = 39;
            Item.width = 64;
            Item.height = 64;
            Item.scale = 1f;
            Item.rare = 10;
            Item.knockBack = 9f;
            Item.value = Item.buyPrice(0, 0, 80, 0);
            Item.useTurn = false;
            Item.autoReuse = false;
            Item.DamageType = DamageClass.Melee;
            Item.shoot = ModContent.ProjectileType<sw_Proj_2>();
            Item.useStyle = ItemUseStyleID.HiddenAnimation;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
        }
		/// <summary>
		/// 这个是生成手持弹幕的代码，如果只是要设置使用的技能，就不用动后面的其他屎山了-只需要填第一个参数就能选择你使用的技能  使用样例看下面shoot函数 
		/// </summary>
		/// <param name="st"></param>
		static void CreatProj(ChenSword_Style st,Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {


			var p1 = Projectile.NewProjectileDirect(source, player.Center, Vector2.Zero, ModContent.ProjectileType<sw_Proj_1>(), damage, knockback, player.whoAmI, 0, 0, (int)st).ModProjectile as sw_Proj_1;


			var p2 = Projectile.NewProjectileDirect(source, player.Center, Vector2.Zero, ModContent.ProjectileType<sw_Proj_2>(), damage, knockback, player.whoAmI, 0, 0, (int)st).ModProjectile as sw_Proj_2;
			p1.Sword_Style = st;
				p2.Sword_Style = st;

		}
		private ChenSword_Style testv = ChenSword_Style.Normal;
		public override bool AltFunctionUse(Player player) => false;
		public override bool CanUseItem(Player player) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				if (ArknightsKeybinds.SkillActivatePressed(player)) {
					if (!modPlayer.SummonMode) {
						//s2
						if (modPlayer.Skill == 1 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
							player.controlUseItem = false;
							modPlayer.SkillActive = true;
							modPlayer.SkillTimer = 0;

							modPlayer.DelStockCount();
						}
						//s3
						else if (modPlayer.Skill == 2 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
							modPlayer.SkillActive = true;
							modPlayer.SkillTimer = 0;
							modPlayer.DelStockCount();
						}
						else
						return false;
					}
				}
				else {
					if (!modPlayer.SummonMode) {
						// S1
						if (modPlayer.Skill == 0) {
							if (modPlayer.StockCount == 0) {
								modPlayer.OffensiveRecovery();
							}
							else if (modPlayer.StockCount > 0) {
								modPlayer.SkillActive = true;
								modPlayer.SkillTimer = 0;
								modPlayer.DelStockCount();
							}
						}
						//S2
						if (modPlayer.Skill == 1&&!modPlayer.SkillActive) {
							if (modPlayer.StockCount == 0) {
								modPlayer.OffensiveRecovery();
							}
						}
						//s3
						if (modPlayer.Skill == 2 && !modPlayer.SkillActive) {
							if (modPlayer.StockCount == 0) {
								modPlayer.OffensiveRecovery();
							}
						}
					}
				}
			}
			return base.CanUseItem(player);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
			{
			///写在这 这是一个简单的四种方式轮流使用的样例
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();

			//CreatProj(ChenSword_Style.Normal, player, source, position, velocity, type, (int)(damage * 5), knockback);

			
			SoundStyle NS = new SoundStyle("ArknightsMod/Sounds/ChenSwordNoSkill") {
				MaxInstances = 4
			};
			if (modPlayer.Skill == 0 && modPlayer.SkillActive) {
				CreatProj(ChenSword_Style.Skill_1, player, source, position, velocity, type, (int)(damage * 3.2), knockback);

			}
			else if (modPlayer.Skill == 1 && modPlayer.SkillActive) {
				CreatProj(ChenSword_Style.Skill_2, player, source, position, velocity, type, (int)(damage * 5), knockback);
				//CreatProj(ChenSword_Style.Skill_2, player, source, position, velocity, type, (int)(damage * 5), knockback);

				modPlayer.StockCount = 0;
			}
			else if (modPlayer.Skill == 2 && modPlayer.SkillActive) {
				CreatProj(ChenSword_Style.Skill_3, player, source, position, velocity, type, (int)(damage * 3.2), knockback);

				modPlayer.StockCount = 0;
			}
			else {
				if (!modPlayer.SkillActive) {
					SoundEngine.PlaySound(NS, player.position);
					CreatProj(ChenSword_Style.Normal, player, source, position, velocity, type, damage, knockback);
				}
			}
			
			return false;
			//return base.Shoot(player, source, position, velocity, type, damage, knockback);
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<Material.PolymerizationPreparation>(4);
			recipe.AddIngredient<Material.WhiteHorseKohl>(6);
			recipe.AddTile(ModContent.TileType<FactoryTile>());
			recipe.Register();
		}
	}
    public class OnHit_Dust_1 : ModDust
    {
        static Texture2D t1;
        static Texture2D t2;
		public override void Load()
		{
			t1 = ModContent.Request<Texture2D>("ArknightsMod/Content/Items/Weapons/Guard/Chen/OnHit_Dust_1", AssetRequestMode.ImmediateLoad).Value;
			t2 = ModContent.Request<Texture2D>("ArknightsMod/Content/Items/Weapons/Guard/Chen/OnHit_Dust_2", AssetRequestMode.ImmediateLoad).Value;

			base.Load();
		}
        public override void OnSpawn(Dust d)
        {
            d.color = Color.White;
            d.color.A = 0;
            d.scale = 0;
            d.alpha = 255;
            base.OnSpawn(d);
        }
        public override bool Update(Dust dust)
        {
            dust.scale = MathHelper.Lerp(dust.scale, 0.7f, 0.1f);
            dust.alpha -= 15;
            if (dust.alpha < 0)
                dust.active = false;
                return false;
        }
        public override bool PreDraw(Dust dust)
        {
            var sb = Main.spriteBatch;
            //for (int i = 0; i < 2; i++)
            {
                sb.Draw(t2, dust.position - Main.screenPosition, null, dust.color * (dust.alpha / 255f), 0, t2.Size() / 2f, dust.scale * 0.4f, default, 0);

                sb.Draw(t1, dust.position - Main.screenPosition, null, dust.color * (dust.alpha / 255f), 0, t1.Size() / 2f, dust.scale * 0.4f, default, 0);
            }
            return false;
        }
    }
    public class Skill_2_Player : ModPlayer
    {
        private Vector2 NowPos = Vector2.Zero;
        private Vector2 ToVec = Vector2.Zero;
        private float LerpStep = 0;
        private bool SettingScreenPos = false;
        public bool CanBeDamaged = true;
        public void SetScreenPos(Vector2 toPos)
        {
            ToVec = toPos;
            SettingScreenPos = true;
        }
        public override bool ConsumableDodge(Player.HurtInfo info)
        {
            bool c = !CanBeDamaged;
            CanBeDamaged = true;

            return base.ConsumableDodge(info) || c;
        }
        public override void PreUpdate()
        {
            if (SettingScreenPos)
            {
                NowPos = Vector2.Lerp(NowPos, ToVec, LerpStep);
                LerpStep = MathHelper.Lerp(LerpStep, 1, 0.03f);
            }
            else
            {
                NowPos = Vector2.Lerp(NowPos, Player.Center, 1 - LerpStep);
                LerpStep = MathHelper.Lerp(LerpStep, 0, 0.03f);
            }
            SettingScreenPos = false;

            base.PreUpdate();
        }
        public override void ModifyScreenPosition()
        {
            if (SettingScreenPos)
                Main.screenPosition = NowPos - Main.ScreenSize.ToVector2() * 0.5f;
            else
            {
                if (Vector2.Distance(NowPos, Player.Center) > 40)
                    Main.screenPosition = NowPos - Main.ScreenSize.ToVector2() * 0.5f;
                else
                    NowPos = Player.Center;
            }
            base.ModifyScreenPosition();
        }
    }

    /// <summary>
    /// 红色的剑
    /// </summary>
    public class sw_Proj_1 : ModProjectile
    {
        public static Texture2D SwordLightTex;
        public Texture2D LightCircle_1 => sw_Proj_2.LightCircle_1;
        private static Texture2D Skill_1_Release_Effect_1;
        private static Texture2D Skill_1_Release_Effect_2;

        private Player player => Main.player[Projectile.owner];
		public ChenSword_Style Sword_Style = 0;
        public override void Load()
        {
            SwordLightTex = ModContent.Request<Texture2D>("ArknightsMod/Content/Items/Weapons/Guard/Chen/SwordLightTail_7", AssetRequestMode.ImmediateLoad).Value;
            Skill_1_Release_Effect_1 = ModContent.Request<Texture2D>("ArknightsMod/Content/Items/Weapons/Guard/Chen/Skill_1_Release_Effect_1", AssetRequestMode.ImmediateLoad).Value;
            Skill_1_Release_Effect_2 = ModContent.Request<Texture2D>("ArknightsMod/Content/Items/Weapons/Guard/Chen/Skill_1_Release_Effect_2", AssetRequestMode.ImmediateLoad).Value;
            base.Load();
        }
		public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 40;
            Projectile.scale = 1f;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.friendly = true; // 友方弹幕
            Projectile.aiStyle = -1; // 不使用默认的 AI 样式，自定义弹幕行为
            Projectile.tileCollide = false;//false就能让他穿墙
            Projectile.penetrate = -1;//表示能穿透几次
            Projectile.ignoreWater = true;//无视液体
										  //Swing_AI
		}
		public override void OnSpawn(IEntitySource source) {

			if (Projectile.ai[2] == 0) {
				Projectile.timeLeft = 76;
				Projectile.ai[0] = 0;
			}

			if (Projectile.ai[2] == (int)ChenSword_Style.Skill_1) {
				Projectile.timeLeft = 71-19;
				Projectile.ai[0] = 39;

			}
			if (Projectile.ai[2] == (int)ChenSword_Style.Skill_2)
				Projectile.timeLeft = 80;

		}
		public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 14;
            base.SetStaticDefaults();
        }
        private Vector2 Swing_DrawScale = new Vector2(0);

        #region 普攻
        private void Swing_AI()
        {
			//Main.NewText(Projectile.ai[0]);
            Projectile.ai[0]++;
            if (Projectile.ai[0] >= 39)
            {
				if (Sword_Style == 0)
					Projectile.Kill();

				player.heldProj = Projectile.whoAmI;
                player.itemAnimation = player.itemTime = 3;
                Projectile.velocity = new Vector2(0, -2).RotatedBy(Projectile.rotation);
                var time = Projectile.ai[0] - 39;
                var extra_Ro = 2f;
                //Dust.NewDustPerfect(Projectile.Center, 6).noGravity = true;
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - extra_Ro * player.direction + MathHelper.Pi);
				if (time == 0) {
					if (player.controlUseItem)
						Projectile.timeLeft = 90;

					if (Main.MouseWorld.X > player.Center.X)
						player.direction = 1;
					else
						player.direction = -1;
					Projectile.ai[1] = (Main.MouseWorld - player.Center).ToRotation() + MathHelper.PiOver2;
				}
				if (time > 15)
                {

                    Projectile.Center = player.MountedCenter + new Vector2(-7 * player.direction, -15).RotatedBy(Projectile.rotation - extra_Ro * player.direction);

                }
                if (time < 15)
                {
                    float ro = time / 15f * 0.2f;
                    Projectile.rotation = MathHelper.Lerp(Projectile.rotation, MathHelper.WrapAngle(Projectile.ai[1] + (4 + ro) * player.direction), time / 30f);
                    Projectile.Center = Vector2.Lerp(Projectile.Center, player.MountedCenter + new Vector2(-7 * player.direction, -15).RotatedBy(Projectile.rotation - extra_Ro * player.direction), time / 30f);
                }
                else if (time < 17)
                {
                    Projectile.rotation = MathHelper.Lerp(Projectile.rotation, MathHelper.WrapAngle(Projectile.ai[1] + (4 + 0.4f) * player.direction), 0.03f);

                }
                else if (time < 45)
                {
                    var step = Math.Clamp((time - 35f) * 0.04f, 0, 1);
                    Projectile.rotation = MathHelper.Lerp(Projectile.rotation, MathHelper.WrapAngle(Projectile.ai[1] + (4 + 0.4f) * player.direction) - (4.7f) * player.direction, step);

                }
				if (time < 17)
					Projectile.ai[0]++;

				Swing_DrawScale = Vector2.Lerp(Swing_DrawScale, new Vector2(1), 0.1f);
            }
            else
            {
                Projectile.rotation = (-MathHelper.Pi + 0.9f) * player.direction;
                Projectile.Center = player.MountedCenter + new Vector2(21 * player.direction, -20);
                Swing_DrawScale = new Vector2(0.9f);
            }
        }
        private void Swing_Draw(SpriteBatch sb, GraphicsDevice gd)
        {
            if (Projectile.ai[0] > 72)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                {
                    List<Vertex> vertices = [];
                    var count = Math.Clamp(Projectile.ai[0] - 13, 1, Projectile.oldRot.Length - 7);
                    var Vertex_Num = 0.1f;

                    for (float i = 0; i < count; i++)
                    {
                        for (float j = 0.0f; j <= 1f; j += Vertex_Num)
                        {
                            Color coordColor = Color.OrangeRed;
                            coordColor.A = 0;
                            float ro = Projectile.oldRot[(int)i].AngleLerp(Projectile.oldRot[(int)i + 1], j);



                            vertices.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -1).RotatedBy(ro) * 90f,
                            new Vector3((float)(i + j) / count, 1, 1),
                            coordColor));
                            vertices.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -1).RotatedBy(ro) * (80f - 60 * (1 - (i + j) / count)),
                                                    new Vector3((float)(i + j + 1) / count, 0, 1),
                                                    coordColor));

                            //Main.NewText(a);
                        }


                    }
                    if (vertices.Count >= 3)
                    {
                        //Main.NewText(Color.Red.ToVector3());
                        gd.Textures[0] = LightCircle_1;
                        gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);

                    }
                }
                //if(false)
                {
                    List<Vertex> vertices = [];
                    var count = Math.Clamp(Projectile.ai[0] - 13, 1, Projectile.oldRot.Length - 5);
                    var Vertex_Num = 0.1f;

                    for (float i = 0; i < count; i++)
                    {
                        for (float j = 0.0f; j < 1f; j += Vertex_Num)
                        {
                            Color coordColor = Color.Lerp(Color.Red, Color.Silver, (i + j) / count);

                            float ro = Projectile.oldRot[(int)i].AngleLerp(Projectile.oldRot[(int)i + 1], j);



                            vertices.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -1).RotatedBy(ro) * 80f,
                            new Vector3((float)(i + j) / count, 1, 1),
                            coordColor));
                            vertices.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -1).RotatedBy(ro) * 30f,
                                                    new Vector3((float)(i + j) / count, 0, 1),
                                                    coordColor));

                            //Main.NewText(a);
                        }


                    }
                    if (vertices.Count >= 3)
                    {
                        //Main.NewText(Color.Red.ToVector3());
                        gd.Textures[0] = SwordLightTex;
                        gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);

                    }
                }
                // if (false)

                {
                    List<Vertex> vertices = [];
                    var count = Math.Clamp(Projectile.ai[0] - 13, 1, Projectile.oldRot.Length - 8);
                    var Vertex_Num = 0.1f;

                    for (float i = 0; i < count; i++)
                    {
                        for (float j = 0.0f; j < 1f; j += Vertex_Num)
                        {
                            Color coordColor = Color.Lerp(Color.SkyBlue, Color.White, (i + j) / count);

                            float ro = Projectile.oldRot[(int)i].AngleLerp(Projectile.oldRot[(int)i + 1], j);



                            vertices.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -1).RotatedBy(ro) * 80f,
                            new Vector3((float)(i + j) / count, 1, 1),
                            coordColor));
                            vertices.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -1).RotatedBy(ro) * 70f,
                                                    new Vector3((float)(i + j) / count, 0, 1),
                                                    coordColor));

                            //Main.NewText(a);
                        }


                    }
                    if (vertices.Count >= 3)
                    {
                        //Main.NewText(Color.Red.ToVector3());
                        gd.Textures[0] = SwordLightTex;
                        gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);

                    }
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            }



            Projectile.KZ_QuicklyDraw_Proj(Swing_DrawScale, spE: player.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);

        }
        private bool Swing_Coll(Rectangle projHitbox, Rectangle targetHitbox)
        {
			//Main.NewText(Projectile.ai[0]);
            if (Projectile.ai[0] > 70)
            {
                float point = 0f;
                Vector2 startPoint = Projectile.Center;
                Vector2 endPoint = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 80 * Swing_DrawScale.Length();
                bool K =
                    Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(),
                    targetHitbox.Size(),
                    startPoint,
                    endPoint,
                    1
                    , ref point);
                if (K && Collision.CanHit(player.Center, 1, 1, targetHitbox.TopLeft(), targetHitbox.Width, targetHitbox.Height)) return true;
            }
            return false;

        }
		#endregion

		#region 技能1
		#endregion

		#region 技能2
		private bool Skill_2_Coll(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.ai[1] > 0)
            {
                float point = 0f;
                Vector2 startPoint = Projectile.Center;
                Vector2 endPoint = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16 * 15;
                bool K =
                    Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(),
                    targetHitbox.Size(),
                    startPoint,
                    endPoint,
                    30
                    , ref point);
                if (K && Collision.CanHit(player.Center, 1, 1, targetHitbox.TopLeft(), targetHitbox.Width, targetHitbox.Height)) return true;

			}
            return false;
        }

        private class Skill_2_Release_Dust_1 : ModDust
        {
            public override void OnSpawn(Dust dust)
            {
                dust.color = Color.White;
                dust.alpha = 220;
                dust.fadeIn = 0.3f;
                dust.scale = 1f;
                dust.velocity = Vector2.Zero;
                dust.rotation = 1;
                base.OnSpawn(dust);
            }
            public override bool Update(Dust dust)
            {
                dust.fadeIn = MathHelper.Lerp(dust.fadeIn, 1, 0.3f);
				dust.velocity *= .5f;
				dust.rotation = dust.velocity.ToRotation();
				dust.position += dust.velocity;
                if (dust.alpha < 0) dust.active = false;
                else dust.alpha -= 10;
                return false;
            }
            static Texture2D t;
            public override void Load()
            {
                t = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
                base.Load();
            }
            public override bool PreDraw(Dust dust)
            {
				float fadeVal = (float)(dust.alpha / 130f) * dust.fadeIn;

				var plaCopy = Main.LocalPlayer.clientClone();
				plaCopy.CopyVisuals(Main.LocalPlayer);
				for (int i = 0; i < plaCopy.armor.Length; i++) {
					plaCopy.armor[i] = Main.LocalPlayer.armor[i].Clone();
					//Main.NewText(plaCopy.armor[i].type);
				}
				for (int i = 0; i < plaCopy.dye.Length; i++) {
					plaCopy.dye[i] = Main.LocalPlayer.dye[i].Clone();
					//Main.NewText(plaCopy.armor[i].type);
				}
				plaCopy.ResetEffects();
				plaCopy.ResetVisibleAccessories();
				plaCopy.invis = false;
				plaCopy.UpdateDyes();
				plaCopy.DisplayDollUpdate();
				plaCopy.skipAnimatingValuesInPlayerFrame = true;
				plaCopy.PlayerFrame();
				plaCopy.skipAnimatingValuesInPlayerFrame = false;
				plaCopy.immuneAlpha = (int)(255 * (1.0f - fadeVal));
				var rro = dust.rotation;
				if (plaCopy.direction == -1)
					rro -= MathHelper.Pi;
				for (int j = 0; j < 3; j++) {

					Main.PlayerRenderer.DrawPlayer(Main.Camera, plaCopy, dust.position - new Vector2(12, 11), rro, new Vector2(12, 21), 0, 1.3f);
					//sb.Draw(t, Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition, null, Color.White * a * aa, Projectile.rotation + MathHelper.Pi, t.Size() * 0.5f, 0.45f, eff, 0);
				}

				{
					var t = sw_Proj_2. sw_Proj_2_t2.Value;
					Main.spriteBatch.Draw(t,
						  dust.position - Main.screenPosition,
								  null,
								  Color.White * fadeVal * .8f,
								  -plaCopy.direction * 2-MathHelper.PiOver4 + rro,
								  new Vector2(0, t.Height),
								  .9f,
								  0,
								  0);
				}


				//Main.spriteBatch.Draw(t, dust.position - Main.screenPosition, null, dust.color * (float)(dust.alpha / 130f) * dust.fadeIn, dust.rotation == 1 ? 0 : MathHelper.Pi, t.Size() / 2f + new Vector2(25, 0), dust.scale, dust.rotation == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically, 0);
				return false;
            }
        }
        private void Skill_2_AI()
        {
            var rand = Main.rand;
            if (Projectile.ai[0] == 0)
            {
                Projectile.rotation = (MathHelper.Pi + 0.8f) * player.direction;
            }
            player.heldProj = Projectile.whoAmI;
            player.itemAnimation = player.itemTime = 3;
            Swing_DrawScale = new Vector2(1);
			var ro = Projectile.velocity.ToRotation();

			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation + MathHelper.PiOver2 * player.direction);
            //if (Projectile.ai[0] < 40)
            {
				Projectile.rotation = Projectile.rotation.AngleLerp((MathHelper.Pi + MathHelper.PiOver2 * 0.9f) * player.direction, 0.2f);
			}
			if (Projectile.ai[0] > 15 && Projectile.ai[1] == 0)
            {
                if (rand.NextBool(2) && Main.mouseRight)
                {
                    var d = Dust.NewDustPerfect(Projectile.Center + new Vector2(player.direction * -15, 1), 222);
                    d.noGravity = true;
                    d.velocity = new Vector2(player.direction, -1).RotatedByRandom(1.2) * 2 * rand.NextFloat(0.5f, 1.5f);
                    d.fadeIn = 0.4f;
                    d.scale = 0.2f;
                }
            }
			if (Projectile.ai[1] == 0) {
				Projectile.velocity = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero);
				if (Projectile.velocity.X > 0)
					player.direction = 1;
				else
					player.direction = -1;
			}
			if (Main.mouseRight && Projectile.ai[1] == 0)
            {
				Projectile.timeLeft = 10;
            }
            else if (Projectile.ai[0] > 20)
            {
                if (Projectile.ai[1] == 0)
                {
					SoundStyle S2 = new SoundStyle("ArknightsMod/Sounds/ChenSwordSkill2") {

						MaxInstances = 2
					};
					SoundEngine.PlaySound(S2, player.position);
					Projectile.timeLeft = 35;
                    Dust.NewDustPerfect(player.Center - Projectile.velocity * 40, ModContent.DustType<Skill_2_Release_Dust_1>(), Projectile.velocity * 50);
					
					//222黄
                    //219红
                    //for (int ii = 0; ii < 3; ii++)
					
                    {
                        for (int i = 0; i < 14; i++)
                        {
                            var d = Dust.NewDustPerfect(player.Center, 222);
                            d.noGravity = true;
                            d.velocity = (new Vector2(20, 0) * rand.NextFloat(0.4f, 1.5f)).RotatedBy(ro);
                            d.scale = 0.8f;


                        }
                        for (int i = -10; i <= 10; i++)
                        {
                            var d = Dust.NewDustPerfect(player.Center, 222);
                            d.noGravity = true;
                            d.velocity = (new Vector2(13, 0) * rand.NextFloat(0.4f, 1.3f)).RotatedBy(ro);
                            d.scale = 0.6f;


                        }
                        for (int i = -10; i <= 10; i++)
                        {
                            var d = Dust.NewDustDirect(player.Center - new Vector2(0, 20).RotatedBy(ro), 0, 40, 219);
                            d.noGravity = true;
                            d.velocity = (new Vector2(23, 0) * rand.NextFloat(0.0f, 1.4f)).RotatedBy(ro);
                            d.scale = 0.6f;
                        }

                    }
					
                }
                if (Projectile.ai[1] == 6)
                {
                    for (float i = 0.1f; i < 1; i += 0.02f)
                    {
                        var d = Dust.NewDustPerfect(player.Center + Projectile.velocity * i * 16 * 15, 76);
                        d.noGravity = true;
                        d.velocity = new Vector2(0, 4 * (rand.NextBool() ? -1 : 1)) * rand.NextFloat(0.3f, 1.4f) * (1.2f - Math.Abs(i - 0.5f) * 1.8f);
                        var c = Color.Blue;
                        if (rand.NextBool())
                            c = Color.Red;
                        c.A = 0;
                        d.color = c;
                        //d.fadeIn = 1.3f;
                        //d.scale = 0.6f;
                    }

                }
                Projectile.ai[1]++;
            }
            var x_ = Math.Clamp(Projectile.ai[0] * 0.4f, 0, 15) * player.direction;
            Projectile.Center = player.MountedCenter + new Vector2(player.direction * 30 - x_, 3 + x_ * player.direction * 0.3f);
            Projectile.ai[0]++;
        }
        private void Skill_2_Draw(SpriteBatch sb)
        {
            Projectile.KZ_QuicklyDraw_Proj(Swing_DrawScale);
            if (Projectile.ai[1] > 0)
            {
                var ControlVal = 13f;
                var val = Math.Clamp(Projectile.ai[1], 0, ControlVal * 2);
                var posx = Math.Clamp(Projectile.ai[1], 0, ControlVal) / ControlVal;
                var invert_posx = 1f - posx;
                val = Math.Abs(val - ControlVal);
                val = ControlVal - val;
                val /= ControlVal;
                var col = Color.Red * val;
                col.A = 0;
				var ro = Projectile.velocity.ToRotation();

				sb.Draw(Skill_1_Release_Effect_1, Projectile.Center - Main.screenPosition, null, col, ro + Projectile.rotation + MathHelper.PiOver2, Skill_1_Release_Effect_1.Size() / 2f, new Vector2(1, 0.5f), default, 0);
                float Skill_1_Release_Effect_2_Scale = 2;
                {
                    sb.Draw(Skill_1_Release_Effect_2, Projectile.Center + new Vector2(300 * posx, 0).RotatedBy(ro) - Main.screenPosition, null, col, ro /*+ player.direction == 1 ? 0 : MathHelper.Pi*/, Skill_1_Release_Effect_2.Size() / 2f, new Vector2(1.4f, 0.7f * invert_posx) * Skill_1_Release_Effect_2_Scale, default, 0);
                    sb.Draw(Skill_1_Release_Effect_2, Projectile.Center + new Vector2(300 * posx, 0).RotatedBy(ro) - Main.screenPosition, null, col, ro/* + player.direction == 1 ? MathHelper.Pi : 0*/, Skill_1_Release_Effect_2.Size() / 2f, new Vector2(1, 0.7f * invert_posx) * Skill_1_Release_Effect_2_Scale, SpriteEffects.FlipVertically, 0);

                }
                sb.Draw(Skill_1_Release_Effect_1, Projectile.Center + new Vector2(100, 0).RotatedBy(ro) - Main.screenPosition, null, col, ro , Skill_1_Release_Effect_1.Size() / 2f, new Vector2(2, 1f * invert_posx), default, 0);

                col = Color.White * val;
                col.A = 0;
                sb.Draw(Skill_1_Release_Effect_1, Projectile.Center - Main.screenPosition, null, col, ro + Projectile.rotation + MathHelper.PiOver2, Skill_1_Release_Effect_1.Size() / 2f, new Vector2(1, 0.3f), default, 0);
                sb.Draw(Skill_1_Release_Effect_1, Projectile.Center + new Vector2(100, 0).RotatedBy(ro)  - Main.screenPosition, null, col, ro, Skill_1_Release_Effect_1.Size() / 2f, new Vector2(2, 1f * invert_posx) * 0.95f, default, 0);

                {
                    sb.Draw(Skill_1_Release_Effect_2, Projectile.Center + new Vector2(300 * posx, 0).RotatedBy(ro)  - Main.screenPosition, null, col, ro/* + player.direction == 1 ? 0 : MathHelper.Pi*/, Skill_1_Release_Effect_2.Size() / 2f, new Vector2(1.4f, 0.7f * invert_posx) * 0.94f * Skill_1_Release_Effect_2_Scale, default, 0);
                    sb.Draw(Skill_1_Release_Effect_2, Projectile.Center + new Vector2(300 * posx, 0).RotatedBy(ro) - Main.screenPosition, null, col, ro /*+ player.direction == 1 ? MathHelper.Pi : 0*/, Skill_1_Release_Effect_2.Size() / 2f, new Vector2(1, 0.7f * invert_posx) * 0.94f * Skill_1_Release_Effect_2_Scale, SpriteEffects.FlipVertically, 0);

                }

            }
        }
        #endregion

        #region 技能3
        private void Skill_3_AI()
        {
            Projectile.rotation = (-MathHelper.Pi + 0.9f) * player.direction;
            Projectile.Center = player.MountedCenter + new Vector2(21 * player.direction, -20);

            if (!Main.mouseRight)
                Projectile.ai[0]++;
            if (Projectile.ai[0] == 0)
            {
                Swing_DrawScale = new Vector2(0.9f);
            }
            else
                Swing_DrawScale = Vector2.Zero;

            Projectile.timeLeft = 3;

            foreach (var p in Main.projectile)
            {
                if (p.active && p != null)
                    if (p.type == ModContent.ProjectileType<sw_Proj_2>())
                    {
                        return;
                    }
            }
            Projectile.Kill();

        }
        private void Skill_3_Draw()
        {
            Projectile.KZ_QuicklyDraw_Proj(Swing_DrawScale);
        }

        #endregion

        public override void AI()
        {
			if (Sword_Style == ChenSword_Style.Normal) {
				Swing_AI();
			}
			if (Sword_Style == ChenSword_Style.Skill_1) {
				if (Projectile.ai[0] < 38) {
					Projectile.ai[0] = 39;
				}
				Swing_AI();

			}
			if (Sword_Style == ChenSword_Style.Skill_2) {
				Skill_2_AI();
			}
			if (Sword_Style == ChenSword_Style.Skill_3) {
				Skill_3_AI();
			}
            base.AI();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            var sb = Main.spriteBatch;
            var gd = Main.graphics.GraphicsDevice;

			if (Sword_Style == ChenSword_Style.Normal) {
				Swing_Draw(sb, gd);
			}
			if (Sword_Style == ChenSword_Style.Skill_1) {
				Swing_Draw(sb, gd);

			}
			if (Sword_Style == ChenSword_Style.Skill_2) {
				Skill_2_Draw(sb);
			}
			if (Sword_Style == ChenSword_Style.Skill_3) {
				Skill_3_Draw();
			}
			return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
			if (Sword_Style == ChenSword_Style.Normal) {
				return Swing_Coll(projHitbox, targetHitbox);
			}
			if (Sword_Style == ChenSword_Style.Skill_1) {
				return Swing_Coll(projHitbox, targetHitbox);

			}
			if (Sword_Style == ChenSword_Style.Skill_2) {
				return Skill_2_Coll(projHitbox, targetHitbox);
			}
			if (Sword_Style == ChenSword_Style.Skill_3) {
			}
            //return Swing_Coll(projHitbox, targetHitbox);
            return false;
        }
        public override bool ShouldUpdatePosition()
        {
            return false;
        }
        private Queue<NPC> Target = new Queue<NPC>();
        public override bool? CanHitNPC(NPC target)
        {
            if (Target.Contains(target))
                return false;
            return base.CanHitNPC(target);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Target.Enqueue(target);
			SoundStyle S1 = new SoundStyle("ArknightsMod/Sounds/ChenSwordSkill1") {
				MaxInstances = 4
			};



			if (Sword_Style == 0) {
				target.AddBuff(BuffID.Confused, 90);
				SoundEngine.PlaySound(S1, player.position);
				base.OnHitNPC(target, hit, damageDone);
			}
			if(Sword_Style == ChenSword_Style.Skill_2) {
				target.StrikeNPC(hit);
			}
			Dust.NewDustDirect(target.position, target.Hitbox.Width, target.Hitbox.Height, ModContent.DustType<OnHit_Dust_1>());
            base.OnHitNPC(target, hit, damageDone);
        }
    }
    /// <summary>
    /// 灰色的剑
    /// </summary>
    public class sw_Proj_2 : ModProjectile
    {
        Texture2D SwordLightTex => sw_Proj_1.SwordLightTex;
        public static Texture2D LightCircle_1;
        private static Asset<Texture2D> sw_Proj_2_old;
        internal static Asset<Texture2D> sw_Proj_2_t2;
        private static Texture2D SwordLightTex_8;
        private Player player => Main.player[Projectile.owner];
		public ChenSword_Style Sword_Style = 0;

		public override void Load()
        {
            LightCircle_1 = ModContent.Request<Texture2D>("ArknightsMod/Content/Items/Weapons/Guard/Chen/LightCircle_1", AssetRequestMode.ImmediateLoad).Value;
            SwordLightTex_8 = ModContent.Request<Texture2D>("ArknightsMod/Content/Items/Weapons/Guard/Chen/SwordLightTail_8", AssetRequestMode.ImmediateLoad).Value;
            sw_Proj_2_old = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad);
            sw_Proj_2_t2 = ModContent.Request<Texture2D>("ArknightsMod/Content/Items/Weapons/Guard/Chen/sw_Proj_2_t2", AssetRequestMode.ImmediateLoad);
            base.Load();
        }
        public override void SetDefaults()
        {
            Projectile.width = 58;
            Projectile.height = 60;
            Projectile.scale = 1f;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.friendly = true; // 友方弹幕
            Projectile.aiStyle = -1; // 不使用默认的 AI 样式，自定义弹幕行为
            Projectile.tileCollide = false;//false就能让他穿墙
            Projectile.penetrate = -1;//表示能穿透几次
            Projectile.ignoreWater = true;//无视液体
            Projectile.timeLeft = 95;

		}
		public override void OnSpawn(IEntitySource source) {

			if (Projectile.ai[2] == 0) {
				Projectile.timeLeft = 76;
				Projectile.ai[0] = 0;
			}

			if (Projectile.ai[2] == (int)ChenSword_Style.Skill_1) {
				Projectile.timeLeft = 52;
				Projectile.ai[0] = 77;

			}

			if (Projectile.ai[2] == (int)ChenSword_Style.Skill_3) {
				Projectile.timeLeft -= 10;
			}
		}
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 14;
            base.SetStaticDefaults();
        }
        private Vector2 Swing_DrawScale = new Vector2(0);
        public override bool ShouldUpdatePosition()
        {
            return false;
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
			if (Sword_Style == ChenSword_Style.Normal) {
				return Swing_Coll(projHitbox, targetHitbox);
			}
			if (Sword_Style == ChenSword_Style.Skill_1) {
			}
			if (Sword_Style == ChenSword_Style.Skill_2) {
				//return Skill_2_Coll(projHitbox, targetHitbox);
			}

			//return Swing_Coll(projHitbox, targetHitbox);
            //return false;
            return false;
        }
        private Queue<NPC> Target = new Queue<NPC>();
        public override bool? CanHitNPC(NPC target)
        {
            if (Target.Contains(target))
                return false;
            return base.CanHitNPC(target);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Dust.NewDustDirect(target.position, target.Hitbox.Width, target.Hitbox.Height, ModContent.DustType<OnHit_Dust_1>());

            Target.Enqueue(target);
            base.OnHitNPC(target, hit, damageDone);
        }

        #region 普攻
        private void Swing_AI()
        {
            if (Projectile.ai[0] < 38)
            {
                if (Projectile.ai[0] > 6)
                player.heldProj = Projectile.whoAmI;
                Projectile.Center = player.MountedCenter + new Vector2(5).RotatedBy(Projectile.rotation - MathHelper.PiOver2);
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation + MathHelper.Pi);
                player.itemAnimation = player.itemTime = 6;

			}
			else if (Sword_Style == 0)
				Projectile.Kill();
			//Main.NewText(Projectile.ai[0]);
			Projectile.velocity = new Vector2(0, -3).RotatedBy(Projectile.rotation);
            if (Projectile.ai[0] == 0)
            {
                Projectile.ai[1] = (Main.MouseWorld - player.Center).ToRotation() + MathHelper.PiOver2;
                Projectile.rotation = Projectile.ai[1];
                Swing_DrawScale = new Vector2(0);
            }
            else if (Projectile.ai[0] <= 6)
            {
                Swing_DrawScale = new Vector2((Projectile.ai[0]) / 6f);
            }
            else if (Projectile.ai[0] < 13)
            {
                Swing_DrawScale = new Vector2(1);
            }
            else if (Projectile.ai[0] == 13)
            {
                if (Main.MouseWorld.X > player.Center.X) player.direction = 1;
                else player.direction = -1;
                Target.Clear();
                Projectile.ai[1] = (Main.MouseWorld - player.Center).ToRotation() + MathHelper.PiOver2;
                Projectile.rotation = Projectile.ai[1] - 2 * player.direction;
            }
            else if (Projectile.ai[0] < 38)
            {
                Projectile.rotation = MathHelper.Lerp(Projectile.rotation, Projectile.ai[1] + 2.3f * player.direction, 0.22f);
            }
            else 
            {
                if (Projectile.ai[0] == 39)
                {
                    if (player.controlUseItem) Projectile.timeLeft = 94;

                }
                Swing_DrawScale = Vector2.Lerp(Swing_DrawScale, new Vector2(0.8f), 0.1f);
				Projectile.rotation = Projectile.rotation.AngleLerp(MathHelper.Pi + 0.5f * player.direction, 0.1f);
				Projectile.Center = Vector2.Lerp(Projectile.Center, player.MountedCenter + new Vector2(19 * player.direction, -30), 0.1f);

			}
			Projectile.ai[0]++;
        }
        private void Swing_Draw(SpriteBatch sb, GraphicsDevice gd)
        {
            if (Projectile.ai[0] > 13 && Projectile.ai[0] < 38)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                {
                    List<Vertex> vertices = [];
                    var count = Math.Clamp(Projectile.ai[0] - 13, 1, Projectile.oldRot.Length - 7);
                    var Vertex_Num = 0.1f;

                    for (float i = 0; i < count; i++)
                    {
                        for (float j = 0.0f; j <= 1f; j += Vertex_Num)
                        {
                            Color coordColor = Color.SkyBlue;
                            coordColor.A = 0;
                            float ro = Projectile.oldRot[(int)i].AngleLerp(Projectile.oldRot[(int)i + 1], j);



                            vertices.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -1).RotatedBy(ro) * 90f,
                            new Vector3((float)(i + j) / count, 1, 1),
                            coordColor));
                            vertices.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -1).RotatedBy(ro) * (80f - 60 * (1 - (i + j) / count)),
                                                    new Vector3((float)(i + j + 1) / count, 0, 1),
                                                    coordColor));

                            //Main.NewText(a);
                        }


                    }
                    if (vertices.Count >= 3)
                    {
                        //Main.NewText(Color.Red.ToVector3());
                        gd.Textures[0] = LightCircle_1;
                        gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);

                    }
                }

                {
                    List<Vertex> vertices = [];
                    var count =  Math.Clamp(Projectile.ai[0] - 13, 1, Projectile.oldRot.Length - 5);
                    var Vertex_Num = 0.1f;

                    for (float i = 0; i < count; i++)
                    {
                        for (float j = 0.0f; j < 1f; j += Vertex_Num)
                        {
                            Color coordColor = Color.Lerp(Color.DarkRed, Color.Silver, (float)Math.Clamp((i) * 0.07, 0, 1));

                            float ro = Projectile.oldRot[(int)i].AngleLerp(Projectile.oldRot[(int)i + 1], j);



                            vertices.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -1).RotatedBy(ro) * 80f,
                            new Vector3((float)(i + j) / count, 1, 1),
                            coordColor));
                            vertices.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -1).RotatedBy(ro) * 30f,
                                                    new Vector3((float)(i + j) / count, 0, 1),
                                                    coordColor));

                            //Main.NewText(a);
                        }


                    }
                    if (vertices.Count >= 3)    
                    {
                        //Main.NewText(Color.Red.ToVector3());
                        gd.Textures[0] = SwordLightTex;
                        gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);

                    }
                }
                {
                    List<Vertex> vertices = [];
                    var Vertex_Num = 0.1f;

                    for (float j = 0.0f; j <= 1.1f; j += Vertex_Num)
                    {
                        Color coordColor = Color.Lerp(Color.IndianRed, Color.White, j);
                        coordColor.A = 0;
                        float ro = Projectile.rotation.AngleLerp(Projectile.oldRot[4], j);



                        vertices.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -1).RotatedBy(ro) * 83f,
                        new Vector3((float)j, 1, 1),
                        coordColor));
                        vertices.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -1).RotatedBy(ro) * 70f,
                                                new Vector3((float)j, 0, 1),
                                                coordColor));

                        //Main.NewText(a);
                    }
                    if (vertices.Count >= 3)
                    {
                        //Main.NewText(Color.Red.ToVector3());
                        gd.Textures[0] = LightCircle_1;
                        //for(int i = 0; i < 2; i ++)
                        gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);

                    }
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            }

            Projectile.KZ_QuicklyDraw_Proj(Swing_DrawScale);
            //第一段刺出去的效果
            if (Projectile.ai[0] <= 6)
            {
                var ex = LightCircle_1;
                var time = (Projectile.ai[0]) / 6f;
                var scale = new Vector2(0.6f, 1 +  time);
                if (time > 0)
                    scale = new Vector2(0.6f * (1f - time), 1);
                
                scale *= new Vector2(0.1f,1);
                scale *= 0.9f;
                var c = Color.DarkRed;
                c.A = 0;
                c *= 0.7f;
                sb.Draw(ex, player.Center - Main.screenPosition, null, c, Projectile.ai[1], ex.Size() / 2f, scale, default, 0);
                c = Color.White;
                c.A = 0;
                scale *= 0.5f;
                sb.Draw(ex, player.Center - Main.screenPosition, null, c, Projectile.ai[1], ex.Size() / 2f, scale, default, 0);

            }
        }
        private bool Swing_Coll(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Projectile.ai[0] < 36)
            {
                float point = 0f;
                Vector2 startPoint = Projectile.Center;
                Vector2 endPoint = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 80 * Swing_DrawScale.Length() / 1.3f;
                bool K =
                    Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(),
                    targetHitbox.Size(),
                    startPoint,
                    endPoint,
                    1
                    , ref point);
                if (K && Collision.CanHit(player.Center, 1, 1, targetHitbox.TopLeft(), targetHitbox.Width, targetHitbox.Height)) return true;
            }
            return false;

        }
		#endregion

		#region 技能1
		#endregion

		#region 技能2
		private void Skill_2_AI()
        {
            Projectile.rotation = MathHelper.Pi + 0.5f * player.direction;

            Projectile.Center = player.MountedCenter + new Vector2(19 * player.direction, -30);
            Swing_DrawScale = new Vector2(0.88f);
            Projectile.timeLeft = 3;

            foreach (var p in Main.projectile)
            {
                if(p.active && p != null)
                    if(p.type == ModContent.ProjectileType<sw_Proj_1>())
                    {
                        return;
                    }
            }
            Projectile.Kill();
        }
        private void Skill_2_Draw()
        {
            Projectile.KZ_QuicklyDraw_Proj(Swing_DrawScale);
        }
        #endregion

        #region 技能3
        private class Skill_3_Dust_1 : ModDust
        {

            static Texture2D t1;
            public override void Load()
            {
                t1 = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;

                base.Load();
            }
            public override void OnSpawn(Dust d)
            {
                d.color = Color.Red;
                d.color.A = 200;
                d.scale = 0;
                d.alpha = 255;
                base.OnSpawn(d);
            }
            public override bool Update(Dust dust)
            {
                dust.scale = MathHelper.Lerp(dust.scale, 1f, 0.1f);
                dust.alpha -= 10;
                if (dust.alpha < 0)
                    dust.active = false;
                return false;
            }
            public override bool PreDraw(Dust dust)
            {
                var sb = Main.spriteBatch; var c = dust.color;
                c.A = 0;
                sb.Draw(LightCircle_1, dust.position - Main.screenPosition, null, c * (dust.alpha / 255f), MathHelper.PiOver4, LightCircle_1.Size() / 2f, dust.scale * 0.1f, default, 0);

                sb.Draw(t1, dust.position - Main.screenPosition, null, dust.color * (dust.alpha / 225f), 0, t1.Size() / 2f, dust.scale, default, 0);

                return false;
            }

        }
        private class Skill_3_Dust_2 : ModDust
        {

            static Texture2D t1;
            public override void Load()
            {
                t1 = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;

                base.Load();
            }
            public override void OnSpawn(Dust d)
            {
                d.color = Color.Red;
                d.scale = 0.5f;
                d.alpha = 255;
                d.fadeIn = 0.1f;
                d.rotation = 3;
                base.OnSpawn(d);
            }
            public override bool Update(Dust dust)
            {
                dust.scale = MathHelper.Lerp(dust.scale, dust.rotation, dust.fadeIn);
                dust.alpha -= (int)(100 * dust.fadeIn);
                if (dust.alpha < 0)
                    dust.active = false;
                return false;
            }
            public override bool PreDraw(Dust dust)
            {
                var sb = Main.spriteBatch;

                var white = Color.White;
                var black = Color.Black;
                for (int i = 0; i < 3; i++)
                    sb.Draw(t1, dust.position - Main.screenPosition, null, black * (dust.alpha / 255f), dust.velocity.ToRotation(), t1.Size() / 2f, dust.scale * 1.1f * new Vector2(1, 0.5f), default, 0);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                for (int i = 0; i < 4; i++)
                {
                    sb.Draw(t1, dust.position - Main.screenPosition, null, dust.color * (dust.alpha / 255f), dust.velocity.ToRotation()  , t1.Size() / 2f, dust.scale * new Vector2(1, 0.7f), default, 0);

                    sb.Draw(t1, dust.position - Main.screenPosition, null, white * (dust.alpha / 255f), dust.velocity.ToRotation(), t1.Size() / 2f, dust.scale * 0.9f * new Vector2(1, 0.3f), default, 0);
                }
                    sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                return false;
            }

        }
        private class Skill_3_Dust_3 : ModDust
        {

            static Texture2D t1;
            public override void Load()
            {
                t1 = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;

                base.Load();
            }
            public override void OnSpawn(Dust d)
            {
                d.color = Color.Red;
                d.scale = 0.7f;
                d.alpha = 255;
                base.OnSpawn(d);
            }
            public override bool Update(Dust dust)
            {
                dust.position += dust.velocity;
                dust.velocity *= 0.94f;
                if(dust.velocity.Length() < 4)
                dust.alpha -= 20;
                if (dust.alpha < 0)
                    dust.active = false;
                return false;
            }
            public override bool PreDraw(Dust dust)
            {
                var sb = Main.spriteBatch; 
                var c = dust.color;
                c.A = 0; 
                sb.Draw(LightCircle_1, dust.position - Main.screenPosition, null, c * (dust.alpha / 255f), MathHelper.PiOver4, LightCircle_1.Size() / 2f, dust.scale * 0.2f, default, 0);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);


                sb.Draw(t1, dust.position - Main.screenPosition, null, Color.White * (dust.alpha / 255f), dust.velocity.ToRotation(), new Vector2(96, 64), dust.scale, default, 0);
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                return false;
            }

        }
        private class Skill_3_Dust_4 : ModDust
        {

            static Texture2D t1;
            public override void Load()
            {
                t1 = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;

                base.Load();
            }
            public override void OnSpawn(Dust d)
            {
                d.color = Color.Orange;
                d.scale = 0;
                d.fadeIn = 3;
                d.alpha = 255;
                base.OnSpawn(d);
            }
            public override bool Update(Dust dust)
            {
                dust.scale = MathHelper.Lerp(dust.scale, dust.fadeIn, 0.1f);
                dust.alpha -= 10;
                if (dust.alpha < 0)
                    dust.active = false;
                return false;
            }
            public override bool PreDraw(Dust dust)
            {
                var sb = Main.spriteBatch;
                for (int i = 0; i < 2; i++)
                    sb.Draw(t1, dust.position - Main.screenPosition, null, dust.color * (dust.alpha / 135f), MathHelper.PiOver4, t1.Size() / 2f, dust.scale, default, 0);
               
                for (int i = 0; i < 4; i++)
                    sb.Draw(t1, dust.position - Main.screenPosition, null, Color.White * (dust.alpha / 255f), 0, t1.Size() / 2f, dust.scale, default, 0);

                return false;
            }

        }

        private class Dragon_Proj : ModProjectile
        {
            public override void SetDefaults()
            {
                Projectile.width = 40;
                Projectile.height = 40;
                Projectile.scale = 1f;
                Projectile.DamageType = DamageClass.Melee;
                Projectile.friendly = true; // 友方弹幕
				Projectile.hostile = false;
                Projectile.aiStyle = -1; // 不使用默认的 AI 样式，自定义弹幕行为
                Projectile.tileCollide = false;//false就能让他穿墙
                Projectile.penetrate = -1;//表示能穿透几次
                Projectile.ignoreWater = true;//无视液体
                Projectile.timeLeft = 10;
            }
            private List<Vector2> RecordPos = [];
            private Vector2 HeadPos = Vector2.Zero;
            private Vector2 OldHeadPos = Vector2.Zero;
            private Vector2 AttackPos = Vector2.Zero;
            public override void Load()
            {
                base.Load();
            }
            public override bool? CanDamage()
            {
                return AttackPos.Length() > 2;
            }
            public override void AI()
            {
                if (Projectile.ai[1] == 0)

                    Projectile.timeLeft = 40;
                if(RecordPos.Count > 13)
                {
                    RecordPos.RemoveAt(0);
                }
                foreach (var p in Main.projectile)
                {
                    if (p.active && p != null)
                        if (p.type == ModContent.ProjectileType<sw_Proj_2>())
                        {
                            var player = Main.player[Projectile.owner];
                            if(player != null)
                            {
                                if (!Main.mouseRight) 
                                {
                                    if (Projectile.ai[1] == 0 && p.ai[0] > 80)
                                    {
                                        if (Vector2.Distance(Main.MouseWorld, HeadPos) > 500)
                                            AttackPos = HeadPos + new Vector2(500, 0).RotatedBy((Main.MouseWorld - HeadPos).ToRotation());
                                        else AttackPos = Main.MouseWorld;
                                    }
                                    Projectile.ai[1]++;
                                }
                                if (Projectile.ai[1] > 0)
                                {

									HeadPos =  Vector2.Lerp(HeadPos, AttackPos, 0.1f);
                                    Projectile.Center = HeadPos;
                                    if (Vector2.Distance(RecordPos[RecordPos.Count - 1], HeadPos) > 10)
                                    {
                                        RecordPos.Add(HeadPos);

									}
                                    Projectile.rotation = Projectile.rotation.AngleLerp((HeadPos - OldHeadPos).ToRotation(), 0.1f);//ro +  MathHelper.PiOver2 * player.direction;
                                }
                                else
                                {

                                    var val = Math.Clamp(Projectile.ai[0] * 0.03f, 0, 1);
                                    var ro = MathHelper.Lerp(0.7f, MathHelper.Pi + MathHelper.PiOver2, val) * player.direction + (player.direction == 1 ? 0 : MathHelper.Pi);
                                    var center = new Vector2(90, 0).RotatedBy(ro);
                                    Projectile.Center = Vector2.Lerp(Projectile.Center, player.MountedCenter, 0.1f);
                                    HeadPos = center + Projectile.Center;

                                    Projectile.rotation = Projectile.rotation.AngleLerp((HeadPos - OldHeadPos).ToRotation() + 0.1f * player.direction, 0.2f) ;//ro +  MathHelper.PiOver2 * player.direction;
                                    //Projectile.rotation = Projectile.rotation.AngleLerp((Main.MouseWorld - HeadPos).ToRotation(), 0.1f);

                                Projectile.direction = player.direction;
                                    if (Projectile.ai[0] == 0)
                                    {
                                        RecordPos.Add(HeadPos);
                                    }
                                    else
                                    {
                                        if (Vector2.Distance(RecordPos[RecordPos.Count - 1], HeadPos) > 10)
                                        {

											RecordPos.Add(HeadPos);
                                        }
                                    }
                                    for (int i = 0; i < RecordPos.Count - 1; ++i)
                                    {
                                        var valll = (i / (float)(RecordPos.Count)) * 0.1f * (1 - Math.Clamp(player.velocity.Length() * 0.2f, 0, 1));
                                        float oriRo = 2 * player.direction + (player.direction == 1 ? 0 : MathHelper.Pi);
                                        oriRo = MathHelper.Lerp(oriRo, ro, (i / (float)(RecordPos.Count + 1f)));
                                        var tovvv = new Vector2(90, 0).RotatedBy(oriRo) + player.MountedCenter;
                                        RecordPos[i] = Vector2.Lerp(RecordPos[i], tovvv, valll);
                                    }

                                    Projectile.ai[0]++;
                                    HeadPos += new Vector2(0, 10);
                                }
                                OldHeadPos = RecordPos[Math.Clamp(RecordPos.Count - 3, 0, 15)];

                                if (p.ai[0] == 80)
                                {
                                    var d = Dust.NewDustPerfect(HeadPos, ModContent.DustType<Skill_3_Dust_1>());
                                }

                            }
                            return;
                        }
                }
                Projectile.Kill();
                base.AI();
            }
            public override bool PreDraw(ref Color lightColor)
            {
                var sb = Main.spriteBatch;
                var gd = Main.graphics.GraphicsDevice;

                var head = TextureAssets.Projectile[Type].Value;
                var headPos = HeadPos;
                var player = Main.player[Projectile.owner];

                var a = Projectile.timeLeft / 40f;
                if (player != null)
                {
                    {
                    Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                
                    List<Vertex> vertices = [];
                    var i = 0f;
                    var count = RecordPos.Count + 1;
                    var OldPos = RecordPos[0];
                    foreach (var pos in RecordPos)
                        {
                            Color coordColor = Color.Red * a;
                            if (i != 0)
                        {
                            //coordColor.A = 0;
                            float ro = (pos - OldPos).ToRotation();
                                var len = MathHelper.Lerp(15, 30, i / (float)count);

                            vertices.Add(new Vertex(pos + new Vector2(len, 0).RotatedBy(ro - MathHelper.PiOver2 * player.direction) - Main.screenPosition,
                                                    new Vector3((float)(i) / count, 0, 1),
                                                    coordColor));
                            vertices.Add(new Vertex(pos + new Vector2(len, 0).RotatedBy(ro + MathHelper.PiOver2 * player.direction) - Main.screenPosition,
                                                    new Vector3((float)(i) / count, 1, 1),
                                                    coordColor));
                        }
                        else
                        {


                            vertices.Add(new Vertex(pos - Main.screenPosition,
                                                    new Vector3(0, 0.5f, 1),
                                                    coordColor));

                        }
                        i++;
                        OldPos = pos;

                    }
                    if (vertices.Count >= 3)
                    {
                        //Main.NewText(Color.Red.ToVector3());
                        gd.Textures[0] = head;
                        gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);

                    }
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                }


                    sb.Draw(head,
                        headPos - Main.screenPosition,
                        null,
                        Color.Red * a,
                        Projectile.rotation,
                        new Vector2(212, 115),
                        0.3f,
                        (player.direction == 1 ? default : SpriteEffects.FlipVertically),
                        0);
            }
               // Main.NewText(Projectile.direction);
                return false;
            }
        }
        private class Skill_3_Attack_Proj : ModProjectile
        {
            public override void SetDefaults()
            {
                Projectile.width = 30;
                Projectile.height = 30;
                Projectile.scale = 1f;
                Projectile.DamageType = DamageClass.Melee;
                Projectile.friendly = true; // 友方弹幕
                Projectile.aiStyle = -1; // 不使用默认的 AI 样式，自定义弹幕行为
                Projectile.tileCollide = false;//false就能让他穿墙
                Projectile.penetrate = -1;//表示能穿透几次
                Projectile.ignoreWater = true;//无视液体
                Projectile.timeLeft = 45;
				Projectile.hostile = false;

			}
			public override void SetStaticDefaults()
            {
                ProjectileID.Sets.TrailingMode[Type] = 2;
                ProjectileID.Sets.TrailCacheLength[Type] = 10;
                base.SetStaticDefaults();
            }
			Player player => Main.player[Projectile.owner];
            private Queue<NPC> Target = new Queue<NPC>();
            private static Texture2D[] Tex = new Texture2D[4];
            public override void Load()
            {
                Tex[0] = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;
                for(int i = 1; i <  Tex.Length; i++)
                {
                    Tex[i] = ModContent.Request<Texture2D>(Texture + "_" + (i + 1), AssetRequestMode.ImmediateLoad).Value;
                }
                base.Load();
            }
            private int T_Count = 0;
            public override void OnSpawn(IEntitySource source)
            {
                T_Count = Main.rand.Next(0, 4);
                RecordCenter = Projectile.Center;
                base.OnSpawn(source);
            }
            private Vector2 RecordCenter = Vector2.Zero;
            public override void AI()
            {
                Projectile.velocity *= 0.9f;
                Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.ai[0]++;

				/*switch (T_Count)
                {
                    case 0:
                        {

                        }
                        break;
                }*/

				base.AI();
            }
			public override bool PreDraw(ref Color lightColor) {
				var sb = Main.spriteBatch;
				var gd = Main.graphics.GraphicsDevice;
				{

					List<Vertex> vertices = [];

					if (Projectile.ai[0] != 0) {
						sb.End();
						sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

						for (float i = 0; i <= 1; i += 0.1f) {
							var p = Vector2.Lerp(Projectile.Center, RecordCenter, i) - Main.screenPosition;
							var col = Color.Lerp(Color.White, Color.Red, Math.Clamp(i * 2, 0, 1)) * (Projectile.timeLeft / 20f);
							// col = Color.Lerp(col, Color.Orange, Math.Clamp((i - 0.3f) * 2, 0, 1))
							vertices.Add(new Vertex(p + new Vector2(0, -23).RotatedBy(Projectile.rotation), new Vector3(((float)i), 0, 0), col));

							vertices.Add(new Vertex(p + new Vector2(0, 23).RotatedBy(Projectile.rotation), new Vector3(((float)i), 1, 0), col));

							//Main.NewText(p);
						}
						if (vertices.Count >= 3) {
							//Main.NewText(Color.Red.ToVector3());
							gd.Textures[0] = SwordLightTex_8;
							gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);

						}
						sb.End();
						sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

					}

				}
				var a = Projectile.timeLeft / 25f;

				{
					var plaCopy = player.clientClone();
					plaCopy.CopyVisuals(player);
					for (int i = 0; i < plaCopy.armor.Length; i++) {
						plaCopy.armor[i] = player.armor[i].Clone();
						//Main.NewText(plaCopy.armor[i].type);
					}
					for (int i = 0; i < plaCopy.dye.Length; i++) {
						plaCopy.dye[i] = player.dye[i].Clone();
						//Main.NewText(plaCopy.armor[i].type);
					}
					plaCopy.ResetEffects();
					plaCopy.ResetVisibleAccessories();
					plaCopy.invis = false;
					plaCopy.UpdateDyes();
					plaCopy.DisplayDollUpdate();
					plaCopy.skipAnimatingValuesInPlayerFrame = true;
					plaCopy.PlayerFrame();
					plaCopy.skipAnimatingValuesInPlayerFrame = false;

					plaCopy.immuneAlpha = 0;

					var t = Tex[T_Count];
					var eff = Projectile.velocity.X > 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;
					var rota = 0f;
					if (Projectile.velocity.X > 0) {
						plaCopy.direction = 1;
						rota = MathHelper.Pi;
					}
					else {
						plaCopy.direction = -1;
					}
					for (int i = Projectile.oldPos.Length - 2; i >= 0; i--) {
						float aa = i / (float)(Projectile.oldPos.Length);
						plaCopy.immuneAlpha = (int)(255 * (1f - a * aa) * .7f);

						Main.PlayerRenderer.DrawPlayer(Main.Camera, plaCopy, Projectile.oldPos[i] + Projectile.Size * 0.5f - new Vector2(12, 21), Projectile.rotation + rota + MathHelper.Pi, new Vector2(12, 21), 0, 1);
						//sb.Draw(t, Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition, null, Color.White * a * aa, Projectile.rotation + MathHelper.Pi, t.Size() * 0.5f, 0.45f, eff, 0);
					}
				}
				{
					var t = sw_Proj_2_t2.Value;
					Main.spriteBatch.Draw(t,
						  Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 20 - Main.screenPosition,
						  null,
						  Color.White * a * .8f,
						  Projectile.rotation + MathHelper.PiOver4,
						  new Vector2(0, t.Height),
						  .8f,
						  0,
						  0);
				}
				return false;
			}
            public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
            {
                Target.Enqueue(target);
                base.OnHitNPC(target, hit, damageDone);
            }
            public override bool? CanHitNPC(NPC target)
            {
                return !Target.Contains(target);
            }
        }
        private Vector2 AttackPos = Vector2.Zero;
		private bool _playedSkill3Sound = false;
		NPC Skill_3_Target = null;
		private void Skill_3_AI()
        {
            var rand = Main.rand;
            player.heldProj = Projectile.whoAmI;
            Projectile.Center = player.MountedCenter + new Vector2(5).RotatedBy(Projectile.rotation - MathHelper.PiOver2);
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation + MathHelper.Pi);
            player.itemAnimation = player.itemTime = 6;
            Swing_DrawScale = new Vector2(1);
			//Main.NewText(player.MountedCenter);
			StatModifier meleeDamageMod = player.GetTotalDamage(DamageClass.Melee);

			int baseDamage = player.HeldItem.damage;

			float finalDamage = meleeDamageMod.ApplyTo(baseDamage);

			int dynamicDamage = (int)Math.Round(finalDamage * 3.2f);

			if (!Main.mouseRight) {
				if (Projectile.localAI[0] == 0 && Projectile.ai[0] > 80) {
					Projectile.timeLeft = 150;

					Projectile.localAI[0]++;
				}
			}
            if (Projectile.localAI[0] != 0)
            {
                if (Projectile.ai[0] > 80)
                {

					player.velocity *= 0;
                    Swing_DrawScale = Vector2.Zero;
                    player.immuneAlpha = 255;
                    var ScrPla = player.GetModPlayer<Skill_2_Player>();
                    ScrPla.SetScreenPos(AttackPos);
                    ScrPla.CanBeDamaged = false;
                    if (Projectile.timeLeft % 12 == 0 && Projectile.ai[2] < 10)
                    {
						//追加索敌
						if (Skill_3_Target != null) {
							if (!Skill_3_Target.active) {
								var temPos = AttackPos;
								float temDis = float.MaxValue;
								bool MarkedNPC = false;
								foreach (var n in Main.ActiveNPCs) {
									if (n.CanBeChasedBy()) {
										var length___ = (AttackPos - n.Center).Length();
										if (length___ < 500)
											if (temDis > length___) {
												temPos = n.Center;
												temDis = length___;
												Skill_3_Target = n;
												MarkedNPC = true;
											}
									}
								}
								if (MarkedNPC)
									if (Vector2.Distance(temPos, player.MountedCenter) < 1000)
										AttackPos = temPos;
							}
						}

						Projectile.ai[2]++;
                        var AttackRo = rand.NextFloat(-0.9f, 0.9f);
                        var AttackDir = (rand.NextBool() ? 1 : -1);
                        var AttackStartPos = AttackPos + new Vector2(110 * AttackDir, 0).RotatedBy(AttackRo);
                        var AttackStartPos_Vel = new Vector2(-30 * AttackDir, 0).RotatedBy(AttackRo).RotatedByRandom(0.2);

                        var p = Projectile.NewProjectileDirect(player.GetSource_FromThis(), AttackStartPos, AttackStartPos_Vel, ModContent.ProjectileType<Skill_3_Attack_Proj>(), dynamicDamage, 1, player.whoAmI);
                        float dust_RandRo = rand.NextFloat(-0.3f, 0.3f);
						if (!_playedSkill3Sound) {
							// 播放音效并标记为已播放
							SoundStyle S3 = new SoundStyle("ArknightsMod/Sounds/ChenSwordSkill3") {
								MaxInstances = 2
							};
							SoundEngine.PlaySound(S3, player.position);
							_playedSkill3Sound = true;
						}
						for (int dustC = 0; dustC < 30; dustC++)
                        {
                            int dt = 222 + (rand.NextBool(3) ? -3 : 0);
                            var u = Dust.NewDustPerfect(AttackPos, dt);
                            u.noGravity = true;
                            u.velocity = AttackStartPos_Vel.RotatedBy(dust_RandRo).RotatedByRandom(0.4) * 0.2f * rand.NextFloat(0.5f, 1.5f);
                            u.fadeIn = 0.4f;
                            u.scale = 0.5f;

						}
                        //圆圈
                        {
                            var randVec = new Vector2(rand.NextFloat(-20, 20)).RotatedByRandom(7);
                            var u = Dust.NewDustPerfect(AttackPos + randVec, ModContent.DustType<Skill_3_Dust_4>());
                            u.fadeIn = rand.NextFloat(0.15f, 0.3f);
                        }
                        //刀锋
                        {
                            var randVec = new Vector2(rand.NextFloat(-20, 20)).RotatedByRandom(7);
                            var d = Dust.NewDustPerfect(AttackPos + randVec, ModContent.DustType<Skill_3_Dust_2>());
                            d.velocity = AttackRo.ToRotationVector2();
                        }
                    }
                    //Main.NewText(Projectile.ai[2]);
                    if (Projectile.ai[2] == 10 && Projectile.timeLeft == 10)
                    {
						//追加索敌
						if (Skill_3_Target != null) {
							if (!Skill_3_Target.active) {
								var temPos = AttackPos;
								float temDis = float.MaxValue;
								bool MarkedNPC = false;
								foreach (var n in Main.ActiveNPCs) {
									if (n.CanBeChasedBy()) {
										var length___ = (AttackPos - n.Center).Length();
										if (length___ < 500)
											if (temDis > length___) {
												temPos = n.Center;
												temDis = length___;
												Skill_3_Target = n;
												MarkedNPC = true;
											}
									}
								}
								if (MarkedNPC)
									if (Vector2.Distance(temPos, player.MountedCenter) < 1000)
										AttackPos = temPos;
							}
						}

						var AttackRo = 0f;
                        var AttackDir = player.MountedCenter.X < AttackPos.X ? 1 : -1;
                        var AttackStartPos = AttackPos + new Vector2(120 * AttackDir, 0).RotatedBy(AttackRo);
                        var AttackStartPos_Vel = new Vector2(-35 * AttackDir, 0).RotatedBy(AttackRo).RotatedByRandom(0.2);
                        for (int dustC = -30; dustC < 30; dustC++)
                        {
                            int dt = 222 + (rand.NextBool(3) ? -3 : 0);

                            var u = Dust.NewDustPerfect(AttackPos, dt);
                            u.noGravity = true;
                            u.velocity = new Vector2(10 * dustC / (float)(Math.Abs(dustC) + 1), 0).RotatedByRandom(0.6) * 2 * rand.NextFloat(0.5f, 1.5f);
                            u.fadeIn = 0.4f;
                            u.scale = 0.6f;
                        }                               
                        //刀锋切割
                        {
                            var d = Dust.NewDustPerfect(AttackPos, ModContent.DustType<Skill_3_Dust_2>());
                            d.rotation = AttackRo;
                            d.fadeIn = 0.05f;
                            d.rotation = 5;
                            d.velocity = new Vector2(1, 0);
                        }
                        //箭头
                        {
                            for (int i = -1; i <= 1; i += 2)
                            {
                                var u = Dust.NewDustPerfect(AttackPos, ModContent.DustType<Skill_3_Dust_3>());
                                u.velocity = new Vector2(15 * i, 0);
                                u.color = Color.Orange;
                            }
                        }

                        //圆圈
                        {
                            var u = Dust.NewDustPerfect(AttackPos, ModContent.DustType<Skill_3_Dust_4>());
                            u.fadeIn = 0.8f;
                        }

                        var p = Projectile.NewProjectileDirect(player.GetSource_FromThis(), AttackStartPos, AttackStartPos_Vel, ModContent.ProjectileType<Skill_3_Attack_Proj>(), dynamicDamage, 1, player.whoAmI);

                    }
                }
            }
            else
            {
				//索敌
				if (Main.mouseRight) {
					var temPos = player.MountedCenter;
					float temDis = float.MaxValue;
					bool MarkedNPC = false;
					foreach (var n in Main.ActiveNPCs) {
						if (n.CanBeChasedBy()) {
							var length___ = (Main.MouseWorld - n.Center).Length();
							if (length___ < 150)
								if (temDis > length___) {
									temPos = n.Center;
									temDis = length___;
									Skill_3_Target = n;
									MarkedNPC = true;
								}
						}
					}
					if (!MarkedNPC)
						temPos = Main.MouseWorld;
					if (Vector2.Distance(temPos, player.MountedCenter) > 500)
						temPos = player.MountedCenter + new Vector2(500, 0).RotatedBy((temPos - player.MountedCenter).ToRotation());
					AttackPos = Vector2.Lerp(AttackPos, temPos, 0.1f);

					Projectile.timeLeft = 10;
					//if (Main.timeForVisualEffects % 3 == 0)
					{
						var Floating_1 = 1f + (float)Math.Sin(Main.timeForVisualEffects * .04f) * .3f;
						var Floating_2 = 1f + (float)Math.Sin(-Main.timeForVisualEffects * .03f + MathHelper.Pi) * .3f;

						for (float i = 0; i < 3f; i++) {

							{
								var d = Dust.NewDustPerfect(AttackPos + Vector2.One.RotatedBy(i * MathHelper.TwoPi / 3f + Main.timeForVisualEffects * .01f) * 18 * Floating_1, 219);
								d.velocity *= 0.1f;
								d.noGravity = true;
								d.scale *= 0.3f;
							}
							{
								var d = Dust.NewDustPerfect(AttackPos + Vector2.One.RotatedBy(i * MathHelper.TwoPi / 3f + MathHelper.Pi / 3f - Main.timeForVisualEffects * .01f) * 11 * Floating_2, 222);
								d.velocity *= 0.1f;
								d.noGravity = true;
								d.scale *= 0.3f;
							}
						}
					}
				}

				if (Projectile.ai[0] == 0) {
					Projectile.NewProjectile(player.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Dragon_Proj>(), 10, 1, player.whoAmI);
					AttackPos = player.MountedCenter;
				}
				Projectile.timeLeft = 10;
                Projectile.ai[0]++;
				if (player.MountedCenter.X < Main.MouseWorld.X)
                {
                    if (player.direction == -1)
                    {
                        Projectile.rotation += MathHelper.TwoPi * 2;
                    }
                    player.direction = 1;
                }
                else
                {
                    if (player.direction == 1)
                    {
                        Projectile.rotation -= MathHelper.TwoPi * 2;
                    }
                    player.direction = -1;
                }
                var st = Math.Clamp(Projectile.ai[0] / 60f * 0.14f, 0, 1);
                Projectile.rotation = MathHelper.Lerp(Projectile.rotation, 10 * player.direction, st);

            }
        }
        private void Skill_3_Draw(GraphicsDevice gd)
        {
            TextureAssets.Projectile[Type] = sw_Proj_2_t2;
                List<Vertex> vertices = [];
                var count = Projectile.oldRot.Length - 1;
                //var Vertex_Num = 0.1f;

            for (float i = 0; i < count; i++)
            {
                //for (float j = 0.0f; j < 1f; j += Vertex_Num)
                {
                    Color coordColor = Color.Lerp(Color.Orange, Color.DarkRed, (float)Math.Clamp((i ) / 6f, 0, 1));

                    float ro = Projectile.oldRot[(int)i];//.AngleLerp(Projectile.oldRot[(int)i + 1], j);

                    if (i == 0) ro = Projectile.rotation;

                    if (player.direction == 1)
                    {
                        vertices.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -1).RotatedBy(ro) * 77f,
                        new Vector3((float)(i ) / count, 1, 1),
                        coordColor));
                        vertices.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -1).RotatedBy(ro) * 50f,
                                                new Vector3((float)(i ) / count, 0, 1),
                                                coordColor));
                    }
                    else
                    {
                        vertices.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -1).RotatedBy(ro) * 50f,
                                                new Vector3((float)(i ) / count, 0, 1),
                                                coordColor));
                        vertices.Add(new Vertex(Projectile.Center - Main.screenPosition + new Vector2(0, -1).RotatedBy(ro) * 77f,
                        new Vector3((float)(i ) / count, 1, 1),
                        coordColor));

                    }
                    //Main.NewText(a);
                }


            }
                if (vertices.Count >= 3)
                {
                    //Main.NewText(Color.Red.ToVector3());
                    gd.Textures[0] = SwordLightTex_8;
                    gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);

                }

                Projectile.KZ_QuicklyDraw_Proj(Swing_DrawScale);
            TextureAssets.Projectile[Type] = sw_Proj_2_old;
        }

        #endregion
        public override void AI()
        {
			if (Sword_Style == ChenSword_Style.Normal) {
				Swing_AI();
			}
			if (Sword_Style == ChenSword_Style.Skill_1) {
				Swing_AI();
				if (Projectile.ai[0] < 38)
					Projectile.ai[0] = 39;
			}
			if (Sword_Style == ChenSword_Style.Skill_2) {
				Skill_2_AI();
			}
			if (Sword_Style == ChenSword_Style.Skill_3) {
				Skill_3_AI();
			}

			//Swing_AI();
			//Skill_2_AI();
			//Skill_2_AI();
			base.AI();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            var sb = Main.spriteBatch;
            var gd = Main.graphics.GraphicsDevice;
			if (Sword_Style == ChenSword_Style.Normal) {
				Swing_Draw(sb, gd);
			}
			if (Sword_Style == ChenSword_Style.Skill_1) {
				Swing_Draw(sb, gd);

			}
			if (Sword_Style == ChenSword_Style.Skill_2) {
				Skill_2_Draw();
			}
			if (Sword_Style == ChenSword_Style.Skill_3) {
				Skill_3_Draw(gd);
			}
			//Swing_Draw(sb, gd);
			//Skill_2_Draw();
			//Skill_2_Draw(gd);

			return false;
        }

    }
}
