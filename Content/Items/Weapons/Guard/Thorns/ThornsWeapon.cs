using ArknightsMod.Content.Buffs;
using ArknightsMod.Content.Rarities;
using ArknightsMod.Content.Tiles.Infrastructure;
using ArknightsMod.Players;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.Graphics.VertexStrip;
using Color = Microsoft.Xna.Framework.Color;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Guard.Thorns
{
    public class ThornsWeapon : UpgradeWeaponBase
	{
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<Material.PolymerizationPreparation>(4);
			recipe.AddIngredient<Material.OrironBlock>(6);
			recipe.AddTile(ModContent.TileType<FactoryTile>());
			recipe.Register();
		}
		private int skillcs = 0;

		private static SoundStyle SkillActive1;
		private static SoundStyle SkillActive3;
		private static SoundStyle jici1;
		private static SoundStyle JiCia;
		private static SoundStyle JiCi2;
		private static SoundStyle JiCi3a;
		public override void Load() {
			SkillActive1 = new SoundStyle("ArknightsMod/Sounds/SkillActive1") {
				Volume = 0.4f,
				MaxInstances = 4,
			};
			SkillActive3 = new SoundStyle("ArknightsMod/Sounds/SkillActive3") {
				Volume = 1f,
				MaxInstances = 4,
			};
			jici1 = new SoundStyle("ArknightsMod/Sounds/jici1") {
				Volume = 1f,
				MaxInstances = 4,
			};
			JiCia = new SoundStyle("ArknightsMod/Sounds/JiCia") {
				Volume = 1f,
				MaxInstances = 5,
			};
			JiCi2 = new SoundStyle("ArknightsMod/Sounds/JiCi2") {
				Volume = 1f,
				MaxInstances = 5,
			};
			JiCi3a = new SoundStyle("ArknightsMod/Sounds/JiCi3a") {
				Volume = 0.8f,
				MaxInstances = 5,
			};
		}
		public override bool MeleePrefix() => true;

		private int skill = 0;
		public override void SetDefaults()
        {
            Item.damage = 142;//攻击力
            Item.DamageType = DamageClass.Melee;
            Item.width = 71;//丢出体积
            Item.height = 104;//丢出体积
            Item.scale = 1;//图片缩放
            Item.useTime = 39;//使用一次时间 
            Item.useAnimation = 39;//动画显示时间
            Item.knockBack = 2f;//击退
            Item.value = 200000;//大概是价格吧
            Item.rare = ModContent.RarityType<ArknightsRarities>();//稀有度
            Item.autoReuse = true;//是否可以连续使用
            Item.noMelee = true;//贴图是否造成伤害
            Item.shoot = 87;
            Item.shootSpeed = 16;//弹幕射速
            Item.useTurn = false;
            Item.noUseGraphic = true;
            Item.useStyle = 13;//?
            Item.channel = true;
        }
		public override bool AltFunctionUse(Player player) => false;
		public override void HoldItem(Player player) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				if (Item.type != ModContent.ItemType<ThornsWeapon>()) {
					player.GetModPlayer<jici2player>().JiCi2 = false;
				}
				if (modPlayer.Skill == 1 && modPlayer.SkillActive&&Item.type == ModContent.ItemType<ThornsWeapon>()) {
					player.GetModPlayer<jici2player>().JiCi2 = true;
				}
				// S1
				if (modPlayer.Skill == 1 && !modPlayer.SkillActive) {
					player.GetModPlayer<jici2player>().JiCi2 = false;
				}
			}
		}
		public override bool CanUseItem(Player player) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				if (ArknightsKeybinds.SkillActivatePressed(player)) {
					if (!modPlayer.SummonMode) {
						// S1
						if (modPlayer.Skill == 0 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
							modPlayer.SkillActive = true;
							modPlayer.SkillTimer = 0;

							modPlayer.DelStockCount();

							Item.UseSound = SkillActive1;
							SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
						//S2
						}
						if (modPlayer.Skill == 1 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
							modPlayer.SkillActive = true;
							modPlayer.SkillTimer = 0;
							player.GetModPlayer<jici2player>().JiCi2 = true;
							modPlayer.DelStockCount();

							Item.UseSound = SkillActive3;
							SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
						}
						//S3
						if (modPlayer.Skill == 2 && modPlayer.StockCount > 0 && !modPlayer.SkillActive&&skillcs>=1) {
							modPlayer.SkillActive = true;
							modPlayer.SkillTimer = 0;
							modPlayer.DelStockCount();
							skillcs = 2;
							Item.UseSound = SkillActive1;
							modPlayer.UpdateActiveSkill2();
							SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
						}
						else if (modPlayer.Skill == 2 && modPlayer.StockCount > 0 && !modPlayer.SkillActive && skillcs < 1) {
							modPlayer.SkillActive = true;
							modPlayer.SkillTimer = 0;

							modPlayer.DelStockCount();
							skillcs = 1;
							Item.UseSound = SkillActive1;
							SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
						}
						else
							return false;
					}
				}
				else {
					if (!modPlayer.SummonMode) {
						Item.UseSound = JiCia;
						SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
						if(modPlayer.CurrentSkill.AutoUpdateActive == false) {
							skillcs = 2;
						}

						// S1
						if (modPlayer.Skill == 0 && modPlayer.SkillActive) {

						}
						else if (modPlayer.Skill == 0 && !modPlayer.SkillActive) {

						}
						// S2
						if (modPlayer.Skill == 1 && modPlayer.SkillActive) {
							player.controlUseItem = false; 
							player.itemAnimation = 0;
							player.itemTime = 0;
						}
						else if (modPlayer.Skill == 1 && !modPlayer.SkillActive) {
							player.GetModPlayer<jici2player>().JiCi2 = false;
						}
						//S31
						if (modPlayer.Skill ==2 && modPlayer.SkillActive&&skillcs>=1) {
							Item.UseSound = JiCi3a;
							SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
						}
						else if (modPlayer.Skill == 2 && !modPlayer.SkillActive) {
							modPlayer.OffensiveRecovery();
							player.GetModPlayer<jici2player>().JiCi2 = false;
						}
						else if (modPlayer.Skill == 2 && modPlayer.SkillActive) {
							Item.UseSound = JiCi3a;
							SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
						}
					}
				}
			}
			return base.CanUseItem(player);
		}
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				if (modPlayer.Skill == 2 && modPlayer.SkillActive == true&& skillcs <= 1) {
					player.GetAttackSpeed(DamageClass.Melee) += 0.25f;
				}
				if (modPlayer.Skill == 2 && modPlayer.SkillActive == true&& skillcs > 1) {
					player.GetAttackSpeed(DamageClass.Melee) += 0.5f;
				}
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();

                //S3至高之术2
             if(modPlayer.Skill == 2 && modPlayer.SkillActive&&skillcs>1)
             {
                    float jc = 2.2f;
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<jiciwd5>(), (int)(damage * jc), knockback, Main.myPlayer);
                    int p = Projectile.NewProjectile(source, position + velocity*3, velocity/2f, ModContent.ProjectileType<jiciwd3>(), (int)(damage * jc), knockback, Main.myPlayer, 1);
                    Main.projectile[p].extraUpdates = 1;
             }

			//S3至高之术1
			 else if (modPlayer.Skill == 2 && modPlayer.SkillActive&& skillcs <= 1)
            {
                float jc = 1.6f;
                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<jiciwd5>(), (int)(damage * jc), knockback, Main.myPlayer);
                int p = Projectile.NewProjectile(source, position + velocity, velocity/2f, ModContent.ProjectileType<jiciwd3>(), (int)(damage * jc), knockback, Main.myPlayer, 1);
                Main.projectile[p].extraUpdates = 1;
            }
			//S1攻击力强化
			else if (modPlayer.Skill == 0 && modPlayer.SkillActive) {
				Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<jiciwd2>(), (int)(damage * 2f), knockback, Main.myPlayer);
				Projectile.NewProjectile(source, position + velocity * 3, velocity, ModContent.ProjectileType<jiciwd3>(), (int)(damage * .8f * 2f), knockback, Main.myPlayer);
			}
			else {
				if(!modPlayer.SkillActive)
				{
					Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<jiciwd2>(), damage, knockback, Main.myPlayer);
					Projectile.NewProjectile(source, position + velocity * 3, velocity, ModContent.ProjectileType<jiciwd3>(), (int)(damage * .8f), knockback, Main.myPlayer);
				}
			}
			return false;
        }
    }
    public class jiciwd2 : ModProjectile
    {
        public override string Texture => "ArknightsMod/Content/Items/Weapons/Guard/Thorns/ThornsSword";
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<sjds>(), 180);
		
		}
        public override void SetDefaults()
        {
            //Projectile.damageclass
            Projectile.extraUpdates = 1;
            Projectile.width = 100;// 弹幕判定体积的宽(碰撞箱)
            Projectile.height = 100;//弹幕判定体积的高
            Projectile.scale = 1f;//放大弹幕
            Projectile.friendly = true;//是否对敌对NPC造成伤害
            Projectile.DamageType = DamageClass.Melee;//伤害类型
            Projectile.ignoreWater = true;//弹幕在水里的时候会不会减速 
            Projectile.tileCollide = false;//弹幕会不会穿墙
            Projectile.timeLeft = 6000;//消散时间60=1秒
            Projectile.penetrate = -1;//弹幕打中几个怪物之后会消失
            Projectile.alpha = 255;//弹幕的透明度
            Projectile.light = 0.5f;//弹幕光照的强度
            Projectile.usesLocalNPCImmunity = true;//NPC是不是按照弹幕ID来获取无敌帧？（如果设定为true，玩家发射8个该弹幕同时击中敌人，则八个都能击中，不骗伤，原版夜明弹的反骗伤就是如此）
            Projectile.localNPCHitCooldown = 30;//上一个设定为true则被调用，NPC按照弹幕ID来获取多少无敌帧
            Projectile.usesIDStaticNPCImmunity = false;//NPC是不是按照弹幕类型来获取无敌帧？（如果设定为true，玩家发射8个该弹幕同时击中敌人，则只能击中一次，其余的会穿透，原版用它来控制喽啰的输出上限）
            Projectile.idStaticNPCHitCooldown = 60;//上一个设定为true则被调用，NPC按照弹幕类型来获取多少无敌帧
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,//这里是用来改弹幕图层的
        List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overWiresUI.Add(index);

        public Vector2[] oldVec = new Vector2[14];
        float t = 0;
        float t2 = 0;
        bool sx = true;//左右
        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];
            //Projectile.ai[1] = ;
            if (player.Center.X - Main.MouseWorld.X > 0) sx = false; else sx = true;
            if (sx)
            {
                Projectile.ai[0] = 3.1415f * 1.75f;//初始位置
                t = -(Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero).ToRotation()
                  + Main.rand.Next(-4, 4 + 1) * .1f;//鼠标位置与随机值
                t2 = +Main.rand.Next(-3, 3 + 1) * .1f;
            }
            else
            {
                Projectile.ai[0] = 3.1415f * 1.15f;
                t = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero).ToRotation()
                  + Main.rand.Next(-4, 4 + 1) * .1f;
                t2 = +Main.rand.Next(-3, 3 + 1) * .1f;
            }
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            float rotaion = (oldVec[0]).ToRotation() - (Main.MouseWorld.X < player.Center.X ? 3.1415f : 0);
            player.itemRotation = rotaion;
            player.direction = Main.MouseWorld.X < player.Center.X ? -1 : 1;
            Projectile.timeLeft = 2;
            //player.itemTime = 2;
            //player.itemAnimation = 2;
            if (sx)
            {
                for (int i = oldVec.Length - 1; i > 0; i--)
                {
                    oldVec[i] = oldVec[i - 1];
                }
                oldVec[0] = new Vector2((float)Math.Sin(Projectile.ai[0] + t + t2), (float)Math.Cos(Projectile.ai[0] + t - t2)) * 60f;
                Projectile.Center = player.Center + new Vector2((float)Math.Sin(Projectile.ai[0] + t + t2), (float)Math.Cos(Projectile.ai[0] + t - t2)) * 60f;
                if (Projectile.ai[0] <= 3.1415f * (2.5f + .5f))//指定位置
                {
                    //这里的30是武器的使用滑动速度，30f/player.itemAnimationMax是计算加成
                    Projectile.ai[0] += 0.12f*30f / player.itemAnimationMax;//旋转
                }
                else
                {
                    Projectile.ai[0] += 0.03f*30f / player.itemAnimationMax;//到位置减速
                    Projectile.ai[2]++;
                    if (Projectile.ai[2] > 11) Projectile.active = false;//消失
                }
            }
            else
            {
                for (int i = oldVec.Length - 1; i > 0; i--)
                {
                    oldVec[i] = oldVec[i - 1];
                }
                oldVec[0] = new Vector2((float)Math.Cos(Projectile.ai[0] + t + t2), (float)Math.Sin(Projectile.ai[0] + t - t2)) * 60f;
                Projectile.Center = player.Center + new Vector2((float)Math.Cos(Projectile.ai[0] + t + t2), (float)Math.Sin(Projectile.ai[0] + t - t2)) * 60f;
                if (Projectile.ai[0] <= 3.1415f * 2.5f)
                {
                    //这里的30是武器的使用滑动速度，30f/player.itemAnimationMax是计算加成
                    Projectile.ai[0] += 0.12f* 30f / player.itemAnimationMax; 
                }
                else
                {
                    Projectile.ai[0] += 0.03f* 30f / player.itemAnimationMax;
                    Projectile.ai[2]++;
                    if (Projectile.ai[2] > 11) Projectile.active = false;
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D 贴图 = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/duaog/wbjex2").Value;
            if (sx) 贴图 = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/duaog/wbjex1").Value;
            StripColorFunction stripColor = (x) => new Color(0, 50, 255, 255);
            StripColorFunction stripColor2 = (x) => new Color(255, 200, 0, 255);
            float Cd = 35;
            VertexStrip strip = new VertexStrip();
            VertexStrip strip2 = new VertexStrip();
            VertexStrip strip3 = new VertexStrip();
            Vector2 wz = Projectile.Center;
            Player player = Main.player[Projectile.owner];

            var rotations = oldVec.Zip(oldVec.Skip(1), (a, b) => a - b).Select((a) => a.ToRotation());
            strip.PrepareStrip(
                oldVec,
                rotations.Prepend(rotations.FirstOrDefault()).ToArray(), stripColor
                ,
                (x) => Cd,
                -Main.screenPosition + player.Center
                );
            strip2.PrepareStrip(
               oldVec,
               rotations.Prepend(rotations.FirstOrDefault()).ToArray(), stripColor2
               ,
               (x) => Cd + 8,
               -Main.screenPosition + player.Center
               );
            // stripColor2 = (x) => new Color(255, 200, 220, 255);
            strip3.PrepareStrip(
              oldVec,
              rotations.Prepend(rotations.FirstOrDefault()).ToArray(), (x) => new Color(255, 200, 140, 255)
              ,
              (x) => 36,
              -Main.screenPosition + player.Center
              );

            BlendState blendStatef = new BlendState()//配置透明度保留状态
            {
                AlphaBlendFunction = BlendState.AlphaBlend.AlphaBlendFunction,
                AlphaDestinationBlend = BlendState.AlphaBlend.AlphaDestinationBlend,
                AlphaSourceBlend = BlendState.AlphaBlend.AlphaSourceBlend,
                ColorBlendFunction = (BlendFunction)0,
                ColorDestinationBlend = (Blend)5,
                ColorSourceBlend = BlendState.Additive.ColorSourceBlend,
                ColorWriteChannels = ColorWriteChannels.All,
                ColorWriteChannels1 = ColorWriteChannels.All,
                ColorWriteChannels2 = ColorWriteChannels.All,
                ColorWriteChannels3 = ColorWriteChannels.All,
                BlendFactor = Color.White,
                MultiSampleMask = -1
            };
            BlendState blendStatef2 = new BlendState()//配置反色混合状态
            {
                AlphaBlendFunction = BlendState.Additive.AlphaBlendFunction,
                AlphaDestinationBlend = BlendState.Additive.AlphaDestinationBlend,
                AlphaSourceBlend = BlendState.Additive.AlphaSourceBlend,
                ColorBlendFunction = BlendFunction.ReverseSubtract,
                ColorDestinationBlend = BlendState.Additive.ColorDestinationBlend,
                ColorSourceBlend = BlendState.Additive.ColorSourceBlend,
                ColorWriteChannels = ColorWriteChannels.All,
                ColorWriteChannels1 = ColorWriteChannels.All,
                ColorWriteChannels2 = ColorWriteChannels.All,
                ColorWriteChannels3 = ColorWriteChannels.All,
                BlendFactor = Color.White,
                MultiSampleMask = -1
            };

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Main.graphics.GraphicsDevice.BlendState = blendStatef2;

            if (oldVec[oldVec.Length - 1] != oldVec[oldVec.Length - 2])
            {
                Main.graphics.GraphicsDevice.Textures[0] = 贴图;
                strip.DrawTrail();
                strip.DrawTrail();
                strip3.DrawTrail();
                Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                strip2.DrawTrail();
                Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/duaog/wbjex4").Value;
                if (sx) Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/duaog/wbjex3").Value;
                strip3.DrawTrail();
            }

            Texture2D 贴图1 = ModContent.Request<Texture2D>("ArknightsMod/Content/Items/Weapons/Guard/Thorns/ThornsSword").Value;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            float jd = (oldVec[1]).SafeNormalize(Vector2.Zero).ToRotation();
            if (sx)
            {
                Main.spriteBatch.Draw(贴图1, player.Center - Main.screenPosition + new Vector2(0, 5),
                    null, Color.AliceBlue, jd + 3.14f / 3.5f, new Vector2(10, 贴图1.Size().Y - 10), Vector2.Distance(new Vector2(0), oldVec[0]) / 60, SpriteEffects.None, 0);//绘制   
            }
            else
            {
                Main.spriteBatch.Draw(贴图1, player.Center - Main.screenPosition + new Vector2(0, 5),
                    null, Color.AliceBlue, jd - 3.14f - 3.14f / 3.5f, 贴图1.Size() + new Vector2(-10), Vector2.Distance(new Vector2(0), oldVec[0]) / 60, SpriteEffects.FlipHorizontally, 0);//绘制
            }
            return false;
        }
    }
    public class jiciwd3 : ModProjectile
    {
        public override string Texture => "ArknightsMod/Content/Items/Weapons/Guard/Thorns/ThornsWeaponProj";
        public override void SetDefaults()
        {

            //Projectile.damageclass
            // Projectile.extraUpdates = 1;
            Projectile.width = 10;// 弹幕判定体积的宽(碰撞箱)
            Projectile.height = 10;//弹幕判定体积的高
            Projectile.scale = 1f;//放大弹幕
            Projectile.friendly = true;//是否对敌对NPC造成伤害
            Projectile.DamageType = DamageClass.Melee;//伤害类型
            Projectile.ignoreWater = true;//弹幕在水里的时候会不会减速
            Projectile.tileCollide = false;//弹幕会不会穿墙
            Projectile.timeLeft = 40;//消散时间60=1秒
            Projectile.penetrate = 1;//弹幕打中几个怪物之后会消失
            Projectile.alpha = 255;//弹幕的透明度
            Projectile.light = 0f;//弹幕光照的强度
            Projectile.usesLocalNPCImmunity = true;//NPC是不是按照弹幕ID来获取无敌帧？（如果设定为true，玩家发射8个该弹幕同时击中敌人，则八个都能击中，不骗伤，原版夜明弹的反骗伤就是如此）
            Projectile.localNPCHitCooldown = 8;//上一个设定为true则被调用，NPC按照弹幕ID来获取多少无敌帧
            Projectile.usesIDStaticNPCImmunity = true;//NPC是不是按照弹幕类型来获取无敌帧？（如果设定为true，玩家发射8个该弹幕同时击中敌人，则只能击中一次，其余的会穿透，原版用它来控制喽啰的输出上限）
            Projectile.idStaticNPCHitCooldown = 60;//上一个设定为true则被调用，NPC按照弹幕类型来获取多少无敌帧
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<sjds2>(), 180);
            for (int i = 0; i < 5; i++)
            {
                Dust v = Dust.NewDustDirect(target.Center, 0, 0, 1, 0f, 0f, 0, new Color(255, 200, 20, 200), 1.7f + Projectile.ai[0] / 2f);
                v.velocity = -(Projectile.velocity + new Vector2(Main.rand.Next(-5, 6), Main.rand.Next(-5, 6))).SafeNormalize(Vector2.Zero) * (2f + Main.rand.Next(3));
                v.noGravity = true;
                //  v.velocity = (-Projectile.velocity / 2f + new Vector2(Main.rand.Next(-5, 6), Main.rand.Next(-5, 6)) / 2f) / 2f;
                Dust s = Dust.NewDustDirect(target.Center, 0, 0, 1, 0f, 0f, 0, new Color(255, 200, 20, 200), 1.7f + Projectile.ai[0] / 2f);
                s.velocity = -(Projectile.velocity + new Vector2(Main.rand.Next(-5, 6), Main.rand.Next(-5, 6))).SafeNormalize(Vector2.Zero) * (2f + Main.rand.Next(3));
                s.noGravity = true;
				//  s.velocity = (-Projectile.velocity / 2f + new Vector2(Main.rand.Next(-5, 6), Main.rand.Next(-5, 6)) / 2f)/2f;
	
			}
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.timeLeft += ((int)Projectile.ai[0]*90);
            base.OnSpawn(source);
        }
        public override void AI()
        {
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height
            , 1, 0f, 0f, 0, new Color(255, 200, 0, 200), 1.7f);
            dust.noGravity = true;
            // 让粒子默认的运动速度归零
            dust.velocity = Projectile.velocity / 2f;
            // 让粒子始终处于弹幕的中心位置
            dust.position = Projectile.Center;
            Projectile.rotation = (Projectile.velocity).ToRotation() + MathHelper.PiOver2 * 2f;
            if (Projectile.ai[0] >= 1)
            {
                NPC target = null;
                // 最大寻敌距离为1000像素
                float distanceMax = 300f;
                foreach (NPC npc in Main.npc)
                {
                    // 如果npc活着且敌对
                    if (npc.active && !npc.friendly&&npc.type != NPCID.TargetDummy&&!npc.dontTakeDamage)
                    {
                        // 计算与玩家的距离
                        float currentDistance = Vector2.Distance(npc.Center, Projectile.Center) + Vector2.Distance(npc.Size, new Vector2(0)) / 2f;
                        // 如果npc距离比当前最大距离小
                        if (currentDistance < distanceMax)
                        {
                            // 就把最大距离设置为npc和玩家的距离
                            // 并且暂时选取这个npc为距离最近npc
                            distanceMax = currentDistance;
                            target = npc;
                        }
                    }
                }

                if (target != null)
                {
                    // 计算朝向目标的向量
                    Vector2 targetVec = target.Center - Projectile.Center;
                    targetVec.Normalize();
                    // 目标向量是朝向目标的大小为20的向量
                    targetVec *= 30f;
                    // 朝向npc的单位向量*20 + 3.33%偏移量
                    Projectile.velocity = (Projectile.velocity * 20f + targetVec) / 21f;
                }
            }
            //Lighting.AddLight(Projectile.Center, 1,1,1);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D 贴图1 = ModContent.Request<Texture2D>("ArknightsMod/Content/Items/Weapons/Guard/Thorns/ThornsWeaponProj").Value;
            Main.spriteBatch.Draw(贴图1, Projectile.Center - Main.screenPosition + new Vector2(0, 0), null, new Color(255, 200, 20, 222),
                Projectile.velocity.ToRotation() + 3.141f, 贴图1.Size() / 2f, 1.3f + Projectile.ai[0] / 2f, SpriteEffects.None, 0);//绘制
            return false;
        }
    }
    public class JiCi4 : ModProjectile
    {
        public override string Texture => "ArknightsMod/Content/Items/Weapons/Guard/Thorns/ThornsSword";
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<sjds2>(), 180);
        }
        public override void SetDefaults()
        {
            //Projectile.damageclass
            Projectile.extraUpdates = 1;
            Projectile.width = 300;// 弹幕判定体积的宽(碰撞箱)
            Projectile.height = 300;//弹幕判定体积的高
            Projectile.scale = 1f;//放大弹幕
            Projectile.friendly = true;//是否对敌对NPC造成伤害
            Projectile.DamageType = DamageClass.Melee;//伤害类型
            Projectile.ignoreWater = true;//弹幕在水里的时候会不会减速 
            Projectile.tileCollide = false;//弹幕会不会穿墙
            Projectile.timeLeft = 600;//消散时间60=1秒
            Projectile.penetrate = -1;//弹幕打中几个怪物之后会消失
            Projectile.alpha = 255;//弹幕的透明度
            Projectile.light = 0.5f;//弹幕光照的强度
            Projectile.usesLocalNPCImmunity = true;//NPC是不是按照弹幕ID来获取无敌帧？（如果设定为true，玩家发射8个该弹幕同时击中敌人，则八个都能击中，不骗伤，原版夜明弹的反骗伤就是如此）
            Projectile.localNPCHitCooldown = 30;//上一个设定为true则被调用，NPC按照弹幕ID来获取多少无敌帧
            Projectile.usesIDStaticNPCImmunity = false;//NPC是不是按照弹幕类型来获取无敌帧？（如果设定为true，玩家发射8个该弹幕同时击中敌人，则只能击中一次，其余的会穿透，原版用它来控制喽啰的输出上限）
            Projectile.idStaticNPCHitCooldown = 60;//上一个设定为true则被调用，NPC按照弹幕类型来获取多少无敌帧
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,//这里是用来改弹幕图层的
        List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overWiresUI.Add(index);

        public Vector2[] oldVec = new Vector2[40];
        float t = 0;
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 4.5f;
            Player player = Main.player[Projectile.owner];
            t = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero).ToRotation();
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            float ndjd = .25f;
            Projectile.ai[1] += 2.5f;
            // if (Projectile.ai[1] > 60)Projectile.friendly = true;//是否对敌对NPC造成伤害
            Projectile.ai[0] = 3.14f / 2f;
            for (int gg = 0; gg < oldVec.Length; gg++)
            {
                for (int i = oldVec.Length - 1; i > 0; i--)
                {
                    oldVec[i] = oldVec[i - 1];
                }
                oldVec[0] = (new Vector2((float)Math.Sin(Projectile.ai[0] + ndjd),
                    (float)Math.Cos(Projectile.ai[0] - ndjd)) * Projectile.ai[1])
                   .RotatedBy(t + 3.14 * .75f);
                //  Projectile.Center = player.Center + new Vector2((float)Math.Sin(Projectile.ai[0] + t), (float)Math.Cos(Projectile.ai[0] + t)) * 60f;
                Projectile.ai[0] += 0.12f;//旋转
            }
            if (Projectile.ai[1] > 100) Projectile.active = false;

        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D 贴图 = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/duaog/wbjex8").Value;
            StripColorFunction stripColor = (x) => new Color(255 - (int)(Projectile.ai[1] * 1.275f * 2), 200 - (int)(Projectile.ai[1] * 2), 0, 255 - (int)(Projectile.ai[1] * 1.275f * 2));
            StripColorFunction stripColor2 = (x) => new Color(0, 55, 255, 255 - (int)(Projectile.ai[1] * 1.275f * 2));
            float Cd = 80;
            VertexStrip strip = new VertexStrip();
            VertexStrip strip2 = new VertexStrip();
            VertexStrip strip3 = new VertexStrip();
            Vector2 wz = Projectile.Center;
            Player player = Main.player[Projectile.owner];

            var rotations = oldVec.Zip(oldVec.Skip(1), (a, b) => a - b).Select((a) => a.ToRotation());
            strip.PrepareStrip(
                oldVec,
                rotations.Prepend(rotations.FirstOrDefault()).ToArray(), stripColor, (x) => Projectile.ai[1] * 2 < 80 ? Projectile.ai[1] * 2 : 80,
                -Main.screenPosition + Projectile.Center - Projectile.velocity * 20);

            strip2.PrepareStrip(
               oldVec,
               rotations.Prepend(rotations.FirstOrDefault()).ToArray(), stripColor2, (x) => Projectile.ai[1] * 2 < 80 ? Projectile.ai[1] * 2 : 80,
               -Main.screenPosition + Projectile.Center - Projectile.velocity * 20);
            BlendState blendStatef = new BlendState()//配置透明度保留状态
            {
                AlphaBlendFunction = BlendState.AlphaBlend.AlphaBlendFunction,
                AlphaDestinationBlend = BlendState.AlphaBlend.AlphaDestinationBlend,
                AlphaSourceBlend = BlendState.AlphaBlend.AlphaSourceBlend,
                ColorBlendFunction = (BlendFunction)0,
                ColorDestinationBlend = (Blend)5,
                ColorSourceBlend = BlendState.Additive.ColorSourceBlend,
                ColorWriteChannels = ColorWriteChannels.All,
                ColorWriteChannels1 = ColorWriteChannels.All,
                ColorWriteChannels2 = ColorWriteChannels.All,
                ColorWriteChannels3 = ColorWriteChannels.All,
                BlendFactor = Color.White,
                MultiSampleMask = -1
            };
            BlendState blendStatef2 = new BlendState()//配置反色混合状态
            {
                AlphaBlendFunction = BlendState.Additive.AlphaBlendFunction,
                AlphaDestinationBlend = BlendState.Additive.AlphaDestinationBlend,
                AlphaSourceBlend = BlendState.Additive.AlphaSourceBlend,
                ColorBlendFunction = BlendFunction.ReverseSubtract,
                ColorDestinationBlend = BlendState.Additive.ColorDestinationBlend,
                ColorSourceBlend = BlendState.Additive.ColorSourceBlend,
                ColorWriteChannels = ColorWriteChannels.All,
                ColorWriteChannels1 = ColorWriteChannels.All,
                ColorWriteChannels2 = ColorWriteChannels.All,
                ColorWriteChannels3 = ColorWriteChannels.All,
                BlendFactor = Color.White,
                MultiSampleMask = -1
            };

            Color color = new Color(255 - (int)(Projectile.ai[1] * 1.275f * 1.4f), 200 - (int)(Projectile.ai[1] * 1.4f), 0, 255 - (int)(Projectile.ai[1] * 1.275f * 1.4f));
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.BlendState = blendStatef2;
            Main.graphics.GraphicsDevice.Textures[0] = 贴图;
            strip2.DrawTrail();
            strip2.DrawTrail();
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            strip.DrawTrail();
            strip.DrawTrail();
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            for (int ff = 0; ff < 5; ff++)
            {
                Texture2D 贴图1 = ModContent.Request<Texture2D>("ArknightsMod/Content/Items/Weapons/Guard/Thorns/ThornsWeaponProj").Value;
                Main.spriteBatch.Draw(贴图1, oldVec[oldVec.Length / 7 * (ff + 2)] * 2 - Main.screenPosition + Projectile.Center - Projectile.velocity * 20
                    , null, color, oldVec[oldVec.Length / 7 * (ff + 2)].ToRotation() + 3.14f
                , 贴图1.Size() / 2f, Projectile.ai[1] / 70f, SpriteEffects.None, 0);//绘制
            }
            return false;
        }
    }
    public class jiciwd5 : ModProjectile
    {
        public override string Texture => "ArknightsMod/Content/Items/Weapons/Guard/Thorns/ThornsSword";
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<sjds>(), 180);
        }
        public override void SetDefaults()
        {
            //Projectile.damageclass
            Projectile.extraUpdates = 1;
            Projectile.width = 140;// 弹幕判定体积的宽(碰撞箱)
            Projectile.height = 140;//弹幕判定体积的高
            Projectile.scale = 1f;//放大弹幕
            Projectile.friendly = true;//是否对敌对NPC造成伤害
            Projectile.DamageType = DamageClass.Melee;//伤害类型
            Projectile.ignoreWater = true;//弹幕在水里的时候会不会减速 
            Projectile.tileCollide = false;//弹幕会不会穿墙
            Projectile.timeLeft = 6000;//消散时间60=1秒
            Projectile.penetrate = -1;//弹幕打中几个怪物之后会消失
            Projectile.alpha = 255;//弹幕的透明度
            Projectile.light = 0.5f;//弹幕光照的强度
            Projectile.usesLocalNPCImmunity = true;//NPC是不是按照弹幕ID来获取无敌帧？（如果设定为true，玩家发射8个该弹幕同时击中敌人，则八个都能击中，不骗伤，原版夜明弹的反骗伤就是如此）
            Projectile.localNPCHitCooldown = 30;//上一个设定为true则被调用，NPC按照弹幕ID来获取多少无敌帧
            Projectile.usesIDStaticNPCImmunity = false;//NPC是不是按照弹幕类型来获取无敌帧？（如果设定为true，玩家发射8个该弹幕同时击中敌人，则只能击中一次，其余的会穿透，原版用它来控制喽啰的输出上限）
            Projectile.idStaticNPCHitCooldown = 60;//上一个设定为true则被调用，NPC按照弹幕类型来获取多少无敌帧
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,//这里是用来改弹幕图层的
        List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overWiresUI.Add(index);

        public Vector2[] oldVec = new Vector2[14];
        float t = 0;
        float t2 = 0;
        bool sx = true;//左右
        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];
            //Projectile.ai[1] = ;
            if (player.Center.X - Main.MouseWorld.X > 0) sx = false; else sx = true;
            if (sx)
            {
                Projectile.ai[0] = 3.1415f * 1.65f;//初始位置
                t = -(Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero).ToRotation()
                  + Main.rand.Next(-4, 4 + 1) * .1f;//鼠标位置与随机值
                t2 = +Main.rand.Next(-3, 3 + 1) * .1f;
            }
            else
            {
                Projectile.ai[0] = 3.1415f * 1.15f;
                t = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero).ToRotation()
                  + Main.rand.Next(-4, 4 + 1) * .1f;
                t2 = +Main.rand.Next(-3, 3 + 1) * .1f;
            }
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            float rotaion = (oldVec[0]).ToRotation() - (Main.MouseWorld.X < player.Center.X ? 3.1415f : 0);
            player.itemRotation = rotaion;
            player.direction = Main.MouseWorld.X < player.Center.X ? -1 : 1;
            Projectile.timeLeft = 2;
            if (sx)
            {
                for (int i = oldVec.Length - 1; i > 0; i--)
                {
                    oldVec[i] = oldVec[i - 1];
                }
                oldVec[0] = new Vector2((float)Math.Sin(Projectile.ai[0] + t + t2), (float)Math.Cos(Projectile.ai[0] + t - t2)) * 60f;
                Projectile.Center = player.Center + new Vector2((float)Math.Sin(Projectile.ai[0] + t + t2), (float)Math.Cos(Projectile.ai[0] + t - t2)) * 60f;
                if (Projectile.ai[0] <= 3.1415f * (2.5f + .5f))//指定位置
                {  
                    //这里的30是武器的使用滑动速度，30f/player.itemAnimationMax是计算加成
                    Projectile.ai[0] += 0.12f * 30f / player.itemAnimationMax;//旋转
                }
                else
                {
                    Projectile.ai[0] += 0.03f * 30f / player.itemAnimationMax;//到位置减速
                    Projectile.ai[2]++;
                    if (Projectile.ai[2] > 11) Projectile.active = false;//消失
                }
            }
            else
            {
                for (int i = oldVec.Length - 1; i > 0; i--)
                {
                    oldVec[i] = oldVec[i - 1];
                }
                oldVec[0] = new Vector2((float)Math.Cos(Projectile.ai[0] + t + t2), (float)Math.Sin(Projectile.ai[0] + t - t2)) * 60f;
                Projectile.Center = player.Center + new Vector2((float)Math.Cos(Projectile.ai[0] + t + t2), (float)Math.Sin(Projectile.ai[0] + t - t2)) * 60f;
                if (Projectile.ai[0] <= 3.1415f * 2.5f)
                {
                    //这里的30是武器的使用滑动速度，30f/player.itemAnimationMax是计算加成
                    Projectile.ai[0] += 0.12f * 30f / player.itemAnimationMax;
                }
                else
                {
                    Projectile.ai[0] += 0.03f * 30f / player.itemAnimationMax;
                    Projectile.ai[2]++;
                    if (Projectile.ai[2] > 11) Projectile.active = false;
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D 贴图 = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/duaog/wbjex2").Value;
            if (sx) 贴图 = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/duaog/wbjex1").Value;
            StripColorFunction stripColor = (x) => new Color(0, 50, 255, 255);
            StripColorFunction stripColor2 = (x) => new Color(255, 200, 0, 255);
            float Cd = 35;
            VertexStrip strip = new VertexStrip();
            VertexStrip strip2 = new VertexStrip();
            VertexStrip strip3 = new VertexStrip();
            VertexStrip strip4 = new VertexStrip();
            VertexStrip strip5 = new VertexStrip();
            Vector2 wz = Projectile.Center;
            Player player = Main.player[Projectile.owner];

            var rotations = oldVec.Zip(oldVec.Skip(1), (a, b) => a - b).Select((a) => a.ToRotation());
            strip.PrepareStrip(
                oldVec,
                rotations.Prepend(rotations.FirstOrDefault()).ToArray(), stripColor
                ,
                (x) => Cd + 18,
                -Main.screenPosition + player.Center
                );
            strip2.PrepareStrip(oldVec,
               rotations.Prepend(rotations.FirstOrDefault()).ToArray(), stripColor2
               , (x) => Cd + 26,
               -Main.screenPosition + player.Center);
            // stripColor2 = (x) => new Color(255, 200, 220, 255);
            strip3.PrepareStrip(oldVec,
            rotations.Prepend(rotations.FirstOrDefault()).ToArray(), (x) => new Color(255, 200, 140, 255)
            , (x) => Cd + 19,
            -Main.screenPosition + player.Center);

            strip4.PrepareStrip(oldVec,
            rotations.Prepend(rotations.FirstOrDefault()).ToArray(), (x) => new Color(175, 255, 220, 200)
            , (x) => Cd + 29,
             -Main.screenPosition + player.Center);

            strip5.PrepareStrip(oldVec,
            rotations.Prepend(rotations.FirstOrDefault()).ToArray(), (x) => new Color(75, 155, 120, 100)
            , (x) => Cd + 22,
            -Main.screenPosition + player.Center);
            BlendState blendStatef = new BlendState()//配置透明度保留状态
            {
                AlphaBlendFunction = BlendState.AlphaBlend.AlphaBlendFunction,
                AlphaDestinationBlend = BlendState.AlphaBlend.AlphaDestinationBlend,
                AlphaSourceBlend = BlendState.AlphaBlend.AlphaSourceBlend,
                ColorBlendFunction = (BlendFunction)0,
                ColorDestinationBlend = (Blend)5,
                ColorSourceBlend = BlendState.Additive.ColorSourceBlend,
                ColorWriteChannels = ColorWriteChannels.All,
                ColorWriteChannels1 = ColorWriteChannels.All,
                ColorWriteChannels2 = ColorWriteChannels.All,
                ColorWriteChannels3 = ColorWriteChannels.All,
                BlendFactor = Color.White,
                MultiSampleMask = -1
            };
            BlendState blendStatef2 = new BlendState()//配置反色混合状态
            {
                AlphaBlendFunction = BlendState.Additive.AlphaBlendFunction,
                AlphaDestinationBlend = BlendState.Additive.AlphaDestinationBlend,
                AlphaSourceBlend = BlendState.Additive.AlphaSourceBlend,
                ColorBlendFunction = BlendFunction.ReverseSubtract,
                ColorDestinationBlend = BlendState.Additive.ColorDestinationBlend,
                ColorSourceBlend = BlendState.Additive.ColorSourceBlend,
                ColorWriteChannels = ColorWriteChannels.All,
                ColorWriteChannels1 = ColorWriteChannels.All,
                ColorWriteChannels2 = ColorWriteChannels.All,
                ColorWriteChannels3 = ColorWriteChannels.All,
                BlendFactor = Color.White,
                MultiSampleMask = -1
            };
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Main.graphics.GraphicsDevice.BlendState = blendStatef2;

            if (oldVec[oldVec.Length - 1] != oldVec[oldVec.Length - 2])
            {
                Main.graphics.GraphicsDevice.Textures[0] = 贴图;
                strip.DrawTrail();
                strip.DrawTrail();
                strip3.DrawTrail();
                Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                strip2.DrawTrail();
                Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/duaog/wbjex4").Value;
                if (sx) Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/duaog/wbjex3").Value;
                strip3.DrawTrail();
                Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/duaog/wbjex6").Value;
                if (sx) Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/duaog/wbjex5").Value;
                strip4.DrawTrail();
                strip5.DrawTrail();
                Texture2D 贴图1 = ModContent.Request<Texture2D>("ArknightsMod/Content/Items/Weapons/Guard/Thorns/ThornsSword").Value;
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                float jd = (oldVec[0]).ToRotation();
                if (sx)
                {
                    Main.spriteBatch.Draw(贴图1, player.Center - Main.screenPosition + new Vector2(0, 5),
                        null, Color.AliceBlue, jd + 3.14f / 3.5f, new Vector2(10, 贴图1.Size().Y - 10), Vector2.Distance(new Vector2(0), oldVec[0]) / 55, SpriteEffects.None, 0);//绘制   
                }
                else
                {
                    Main.spriteBatch.Draw(贴图1, player.Center - Main.screenPosition + new Vector2(0, 5),
                        null, Color.AliceBlue, jd - 3.14f - 3.14f / 3.5f, 贴图1.Size() + new Vector2(-10), Vector2.Distance(new Vector2(0), oldVec[0]) / 55, SpriteEffects.FlipHorizontally, 0);//绘制
                }
            }
            return false;
        }
    }
}