using ArknightsMod.Content.Tiles.Infrastructure;
using ArknightsMod.Content.Projectiles;
using ArknightsMod.Players;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using ArknightsMod.Content;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;

using Terraria.Graphics;

using Terraria.ID;

using Terraria.ModLoader;
using static Terraria.Graphics.VertexStrip;
using Color = Microsoft.Xna.Framework.Color;
using ArknightsMod.Content.Rarities;


namespace ArknightsMod.Content.Items.Weapons.Guard.SilverAsh
{
    public class SilverAshWeapon : UpgradeWeaponBase
	{
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<Material.D32Steel>(4);
			recipe.AddIngredient<Material.WhiteHorseKohl>(6);
			recipe.AddTile(ModContent.TileType<FactoryTile>());
			recipe.Register();
		}
		public override bool MeleePrefix() => true;
        public override void SetDefaults()
        {
            Item.damage = 142;//攻击力
            Item.DamageType = DamageClass.Melee;
            Item.width = 52;//丢出体积
            Item.height = 48;//丢出体积
            Item.scale = 1;//图片缩放
            Item.useTime = 39;//使用一次时间 
            Item.useAnimation = 39;//动画显示时间
			Item.knockBack = 2f;//击退
            Item.value = 200000;//大概是价格吧 
            Item.rare = ModContent.RarityType<ArknightsRarities>();//稀有度
            Item.autoReuse = true;//是否可以连续使用
            Item.noMelee = true;//贴图是否造成伤害
            Item.shoot = 87;
            Item.shootSpeed = 4;//弹幕射速
            Item.useTurn = false;
            Item.noUseGraphic = true;
			Item.UseSound = NoSound;
            Item.useStyle = 13;//?
            Item.channel = true;
            //SoundStyle zji = new SoundStyle("Slashsoul/bgms/yc");
            //Item.UseSound = zji;
        }
        /// <summary>
        /// 技能切换
        /// </summary>
        bool GJXT = false;
		private bool skcl = true;
		private int skill2cd=0;
		private static SoundStyle SkillActive1;
		private static SoundStyle SkillActive3;
		private static SoundStyle yinhui2A;
		private static SoundStyle yinhuiA;
		private static SoundStyle NoSound;

		public override void Load() {
			SkillActive1 = new SoundStyle("ArknightsMod/Sounds/SkillActive1") {
				Volume = 0.4f,
				MaxInstances = 4,
			};
			SkillActive3 = new SoundStyle("ArknightsMod/Sounds/SkillActive3") {
				Volume = 1f,
				MaxInstances = 4,
			};
			yinhuiA = new SoundStyle("ArknightsMod/Sounds/yinhuiA") {
				Volume = 0.4f,
				MaxInstances = 4,
			};
			yinhui2A = new SoundStyle("ArknightsMod/Sounds/yinhui2A") {
				Volume = 0.5f,
				MaxInstances = 4,
			};
			NoSound = new SoundStyle("ArknightsMod/Sounds/NoSound") {
				Volume = 0f,
				MaxInstances = 4,
			};
		}
		public override bool AltFunctionUse(Player player) => false;
		public override bool CanUseItem(Player player) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				if (ArknightsKeybinds.SkillActivatePressed(player)) {
					if (!modPlayer.SummonMode) {
						// S2
						if (modPlayer.Skill == 1 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
							modPlayer.SkillActive = true;
							modPlayer.SkillTimer = 0;

							modPlayer.DelStockCount();
							player.GetModPlayer<yinhui2player>().yinhui2 = true;
							Item.UseSound = SkillActive3;
							SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
						}
						else if (modPlayer.Skill == 1 && modPlayer.SkillActive&&skill2cd>=300) {
							modPlayer.SkillActive = false;
							modPlayer.StockCount = 0;
							player.GetModPlayer<yinhui2player>().yinhui2 = false;
							Item.UseSound = SkillActive1;
							SoundEngine.PlaySound(Item.UseSound.Value, player.Center);

						}
						else if (modPlayer.Skill == 2 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
							modPlayer.SkillActive = true;
							modPlayer.SkillTimer = 0;
							player.GetModPlayer<yinhui3player>().yinhui3 = true;
							modPlayer.DelStockCount();

							Item.UseSound = SkillActive1;
							SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
						}
						return false;
					}
				}
				else {
					if (!modPlayer.SummonMode) {
						if (modPlayer.Skill == 0) {
							if (modPlayer.StockCount == 0) {
								modPlayer.OffensiveRecovery();
								Item.UseSound = NoSound;
								SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
							}
							else if (modPlayer.StockCount > 0) {
								modPlayer.SkillActive = true;
								modPlayer.SkillTimer = 0;
								modPlayer.DelStockCount();
								Item.UseSound = NoSound;
								SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
							}
						}
						if (modPlayer.Skill == 1&&modPlayer.SkillActive) {
							Item.UseSound = NoSound;
							SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
						}
						else if (modPlayer.Skill == 1 && !modPlayer.SkillActive) {
							Item.UseSound = NoSound;
							SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
						}
						if (modPlayer.Skill == 2 && modPlayer.SkillActive) {
							Item.UseSound = NoSound;
							SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
						}
						else if (modPlayer.Skill == 2 && !modPlayer.SkillActive) {
							Item.UseSound = NoSound;
							SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
						}
					}
				}
			}
			return base.CanUseItem(player);
		}
		public override void HoldItem(Player player) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				if (modPlayer.Skill == 1 && modPlayer.SkillActive && Item.type == ModContent.ItemType<SilverAshWeapon>()) {
					player.GetModPlayer<yinhui2player>().yinhui2 = true;
					skill2cd++;
				}
				// S1
				if (modPlayer.Skill == 1 && !modPlayer.SkillActive) {
					player.GetModPlayer<yinhui2player>().yinhui2 = false;
					skill2cd=0;
				}
				if (modPlayer.Skill != 1) {
					player.GetModPlayer<yinhui2player>().yinhui2 = false;
					skill2cd = 0;
				}
				if (modPlayer.Skill == 2 && modPlayer.SkillActive && Item.type == ModContent.ItemType<SilverAshWeapon>()) {
					player.GetModPlayer<yinhui3player>().yinhui3 = true;
				}
				if (modPlayer.Skill == 2 && !modPlayer.SkillActive) {
					player.GetModPlayer<yinhui3player>().yinhui3 = false;
				}
				if (modPlayer.Skill != 2) {
					player.GetModPlayer<yinhui3player>().yinhui3 = false;
				}
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			SoundStyle zji2 = new SoundStyle("ArknightsMod/Sounds/yinh1")
            {
                MaxInstances = 4
            };
            SoundStyle zji3 = new SoundStyle("ArknightsMod/Sounds/yinh2")
            {
                MaxInstances = 4
            };
			SoundStyle yh2A = new SoundStyle("ArknightsMod/Sounds/yinhui2A") {
				MaxInstances = 4
			};
			SoundStyle yhA = new SoundStyle("ArknightsMod/Sounds/yinhuiA") {
				MaxInstances = 4
			};
			int bzd = Main.rand.Next(-7, 7 + 1);
            //天赋
            int jinsh = (int)(damage * 1.12f);
            //强力击类技能单独写
            if (modPlayer.Skill == 0)
            {
                if (!modPlayer.SkillActive)//普攻
                {
					SoundEngine.PlaySound(yhA, player.position);
					Projectile.NewProjectile(source, position
                    , (velocity * 1.4f).RotatedBy(bzd / 45f), ModContent.ProjectileType<yinhdsz2>(), jinsh, knockback, Main.myPlayer);
                    int mm1 = Main.rand.Next(100, 401) * player.direction;
                    Vector2 velocity1 = (new Vector2(mm1, 655)).SafeNormalize(Vector2.Zero) * (22f);
                    Projectile.NewProjectile(source, Main.MouseWorld - new Vector2(mm1, 655)
                    , velocity1, ModContent.ProjectileType<yinhdsz4>(), (int)(jinsh * .8f), knockback, Main.myPlayer);
                }
                else //技能1
                {
                    SoundEngine.PlaySound(zji2, player.position);
					SoundEngine.PlaySound(yhA, player.position);
					Projectile.NewProjectile(source, position
                  , (velocity * 1.4f).RotatedBy(bzd / 45f), ModContent.ProjectileType<yinhdsz2>(), (int)(jinsh * 2.9f), knockback, Main.myPlayer, 0, 1);
                    int mm1 = Main.rand.Next(100, 401) * player.direction;
                    Vector2 velocity1 = (new Vector2(mm1, 655)).SafeNormalize(Vector2.Zero) * (22f);
                    Projectile.NewProjectile(source, Main.MouseWorld - new Vector2(mm1, 655)
                    , velocity1, ModContent.ProjectileType<yinhdsz4>(), (int)(jinsh * 2.9f * .8f), knockback, Main.myPlayer, 1);
                }
            }
            else if (modPlayer.Skill == 1)
            {
                if (!modPlayer.SkillActive)
                {
					//普通攻击
					SoundEngine.PlaySound(yhA, player.position);
					Projectile.NewProjectile(source, position
                 , (velocity * 1.4f).RotatedBy(bzd / 45f), ModContent.ProjectileType<yinhdsz2>(), jinsh, knockback, Main.myPlayer);
                    int mm1 = Main.rand.Next(100, 401) * player.direction;
                    Vector2 velocity1 = (new Vector2(mm1, 655)).SafeNormalize(Vector2.Zero) * (22f);
                    Projectile.NewProjectile(source, Main.MouseWorld - new Vector2(mm1, 655)
                    , velocity1, ModContent.ProjectileType<yinhdsz4>(), (int)(jinsh * .8f), knockback, Main.myPlayer);
                }
                else
                {
					//2技能
					SoundEngine.PlaySound(yh2A, player.position);
					Projectile.NewProjectile(source, position
                  , (velocity * 1.4f).RotatedBy(bzd / 45f), ModContent.ProjectileType<yinhdsz2>(), jinsh, knockback, Main.myPlayer, 0, 1);
                }
            }
            else if (modPlayer.Skill == 2)
            {
                if (!modPlayer.SkillActive)
                {
					//普通攻击
					SoundEngine.PlaySound(yhA, player.position);
					Projectile.NewProjectile(source, position
                    , (velocity * 1.4f).RotatedBy(bzd / 45f), ModContent.ProjectileType<yinhdsz2>(), jinsh, knockback, Main.myPlayer);
                    int mm1 = Main.rand.Next(100, 401) * player.direction;
                    Vector2 velocity1 = (new Vector2(mm1, 655)).SafeNormalize(Vector2.Zero) * (22f);
                    Projectile.NewProjectile(source, Main.MouseWorld - new Vector2(mm1, 655)
                    , velocity1, ModContent.ProjectileType<yinhdsz4>(), (int)(jinsh * .8f), knockback, Main.myPlayer);
                }
                else
                {
                    //3技能
                    SoundEngine.PlaySound(zji3, player.position);
                    Projectile.NewProjectile(source, position
                    , (velocity * 1.4f), ModContent.ProjectileType<yinhdsz5>(), jinsh * 3, knockback, Main.myPlayer);
                    Projectile.NewProjectile(source, position - velocity.SafeNormalize(Vector2.Zero) * 60
                         , (velocity * 1.4f), ModContent.ProjectileType<yinhdsz6>(), jinsh * 3, knockback, Main.myPlayer);
                }
            }
            return false;
        }
    }
    public class yinhdsz2 : ModProjectile
    {
        public override string Texture => "ArknightsMod/Content/Items/Weapons/Guard/SilverAsh/SilverAshWeapon2";
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(49);
            AIType = 49;
            Projectile.width = 20;// 弹幕判定体积的宽(碰撞箱)
            Projectile.height = 20;//弹幕判定体积的高
            Projectile.DamageType = DamageClass.Melee;//伤害类型
            Projectile.scale = 1f;
            Projectile.usesLocalNPCImmunity = true;//NPC是不是按照弹幕ID来获取无敌帧？（如果设定为true，玩家发射8个该弹幕同时击中敌人，则八个都能击中，不骗伤，原版夜明弹的反骗伤就是如此）
            Projectile.localNPCHitCooldown = 15;//上一个设定为true则被调用，NPC按照弹幕ID来获取多少无敌帧
            Projectile.usesIDStaticNPCImmunity = false;//NPC是不是按照弹幕类型来获取无敌帧？（如果设定为true，玩家发射8个该弹幕同时击中敌人，则只能击中一次，其余的会穿透，原版用它来控制喽啰的输出上限）
            Projectile.idStaticNPCHitCooldown = 60;//上一个设定为true则被调用，NPC按照弹幕类型来获取多少无敌帧
        }
        bool jjj = false;
        int max = 0;
        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];
            max = player.itemTime;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (jjj == false)
            {
                jjj = true;
                Player player = Main.player[Projectile.owner];
                Projectile.NewProjectile(player.GetSource_Death(), Projectile.Center
                  , Projectile.velocity.SafeNormalize(Vector2.Zero), ModContent.ProjectileType<TYTX>(), 0, 0, Main.myPlayer);
            }

        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            // Main.NewText($"{player.itemTime}");

            if (jjj == false && Projectile.ai[0] >= player.itemTime / 3.5f && Projectile.ai[1] == 1)
            {
                jjj = true;
                Projectile.NewProjectile(player.GetSource_Death(), Projectile.Center
                  , Projectile.velocity.SafeNormalize(Vector2.Zero), ModContent.ProjectileType<yinhdsz3>(), 0, 0, Main.myPlayer);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return true;
        }
    }
    public class yinhdsz3 : ModProjectile
    {
        public override string Texture => "ArknightsMod/Content/Items/Weapons/Guard/SilverAsh/SilverAshWeapon2";
        public override void SetDefaults()
        {
            Projectile.extraUpdates = 1;
            Projectile.width = 40;// 弹幕判定体积的宽(碰撞箱)
            Projectile.height = 40;//弹幕判定体积的高
            Projectile.scale = 1f;//放大弹幕
            Projectile.friendly = false;//是否对敌对NPC造成伤害
            Projectile.DamageType = DamageClass.Melee;//伤害类型
            Projectile.ignoreWater = true;//弹幕在水里的时候会不会减速 
            Projectile.tileCollide = false;//弹幕会不会穿墙
            Projectile.timeLeft = 20;//消散时间60=1秒
            Projectile.penetrate = -1;//弹幕打中几个怪物之后会消失
            Projectile.alpha = 255;//弹幕的透明度
            Projectile.light = 0.5f;//弹幕光照的强度
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.ai[0] = Projectile.timeLeft;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + 3.14f / 2f;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,//这里是用来改弹幕图层的
        List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overWiresUI.Add(index);
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D 贴图 = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/ex24").Value;
            int d = (int)(200f / Projectile.ai[0] * Projectile.timeLeft);
            Main.spriteBatch.Draw(贴图, Projectile.Center - Main.screenPosition - Projectile.velocity.SafeNormalize(Vector2.Zero) * 2f,
        null, new Color(d + 30, d + 30, d + 55, d / 4 + 200), Projectile.rotation, 贴图.Size() / 2f,
        new Vector2(1.5f / Projectile.ai[0] * Projectile.timeLeft, 2.5f) / 1f * (Projectile.ai[1] + 1)
        , SpriteEffects.None, 0);//绘制

            Main.spriteBatch.Draw(贴图, Projectile.Center - Main.screenPosition - Projectile.velocity.SafeNormalize(Vector2.Zero) * 2f,
             null, new Color(d + 30, d + 30, d + 55, 180), Projectile.rotation, 贴图.Size() / 2f,
             new Vector2(1.5f / Projectile.ai[0] * Projectile.timeLeft, 2.5f) / 1.5f* (Projectile.ai[1]+1)
             , SpriteEffects.None, 0);//绘制
            return true;
        }
    }
    public class yinhdsz4 : ModProjectile
    {
        public override string Texture => "ArknightsMod/Content/Items/Weapons/Guard/SilverAsh/SilverAshWeapon2";
        public override void SetDefaults()
        {
            Projectile.width = 20;// 弹幕判定体积的宽(碰撞箱)
            Projectile.height = 20;//弹幕判定体积的高
            Projectile.extraUpdates = 1;
            Projectile.friendly = true;//是否对敌对NPC造成伤害
            Projectile.DamageType = DamageClass.Melee;//伤害类型
            Projectile.ignoreWater = true;//弹幕在水里的时候会不会减速
            Projectile.tileCollide = false;//弹幕会不会穿墙
            Projectile.timeLeft = 160;//消散时间60=1秒
            Projectile.penetrate = 1;//弹幕打中几个怪物之后会消失
            Projectile.alpha = 255;//弹幕的透明度
            Projectile.light = 0.5f;//弹幕光照的强度
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
        }
        int gg = 0;
        bool zz = false;
        bool sss = true;
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            Projectile.NewProjectile(player.GetSource_Death(), Projectile.Center
                 , Projectile.velocity.SafeNormalize(Vector2.Zero), ModContent.ProjectileType<yinhdsz3>(), 0, 0, Main.myPlayer);
        }
        public override bool PreDraw(ref Color lightColor)
        {

            Vector2 vector = Projectile.Center - Projectile.oldPos[0] + Projectile.velocity * 3f;
            Player player = Main.player[Projectile.owner];
            Texture2D 贴图 = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/ex24").Value;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);


            Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/ex26").Value;
            VertexStrip strip = new VertexStrip();
            var rotations = Projectile.oldPos.Zip(Projectile.oldPos.Skip(1), (a, b) => a - b).Select((a) => a.ToRotation());
            strip.PrepareStrip(
                Projectile.oldPos,
                rotations.Prepend(rotations.FirstOrDefault()).ToArray(),
                (x) => new Color(151, 151, 201, 255),
                (x) => 6 * (Projectile.ai[0] + 1),
                -Main.screenPosition + new Vector2(Projectile.width, Projectile.height) / 2);
            strip.DrawTrail();
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            // int d = (int)(200f / Projectile.ai[0] * Projectile.timeLeft);
            Main.spriteBatch.Draw(贴图, Projectile.Center - Main.screenPosition - Projectile.velocity.SafeNormalize(Vector2.Zero) * 2f,
        null, new Color(155, 155, 205, 255), Projectile.rotation, 贴图.Size() / 2f,
        new Vector2(1.5f, 3.5f) / 1.7f * (Projectile.ai[0] + 1)
        , SpriteEffects.None, 0);//绘制

            Main.spriteBatch.Draw(贴图, Projectile.Center - Main.screenPosition - Projectile.velocity.SafeNormalize(Vector2.Zero) * 2f,
             null, new Color(205, 205, 255, 205), Projectile.rotation, 贴图.Size() / 2f,
             new Vector2(1.5f, 3.5f) / 2f * (Projectile.ai[0] + 1)
             , SpriteEffects.None, 0);//绘制

            return false;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + 3.14f / 2f;

            NPC target = null;
            // 最大寻敌距离为1000像素
            float distanceMax = 300f;
            foreach (NPC npc in Main.npc)
            {
                // 如果npc活着且敌对
                if (npc.active && !npc.friendly)
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
                targetVec *= 18f;
                // 朝向npc的单位向量*20 + 3.33%偏移量
                Projectile.velocity = (Projectile.velocity * 10f + targetVec) / 11f;
            }
        }
        public override void OnKill(int timeLeft)
        {

        }
    }
    public class yinhdsz5 : ModProjectile
    {
        public override string Texture => "ArknightsMod/Content/Items/Weapons/Guard/SilverAsh/SilverAshWeapon2";
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            Projectile.NewProjectile(player.GetSource_Death(), target.Center
                 , Projectile.velocity.SafeNormalize(Vector2.Zero), ModContent.ProjectileType<yinhdsz3>(), 0, 0, Main.myPlayer);
        }
        public override void SetDefaults()
        {
            //Projectile.damageclass
            Projectile.extraUpdates = 1;
            Projectile.width = 10;// 弹幕判定体积的宽(碰撞箱)
            Projectile.height = 10;//弹幕判定体积的高
            Projectile.scale = 1f;//放大弹幕
            Projectile.friendly = false;//是否对敌对NPC造成伤害
            Projectile.DamageType = DamageClass.Melee;//伤害类型
            Projectile.ignoreWater = true;//弹幕在水里的时候会不会减速 
            Projectile.tileCollide = false;//弹幕会不会穿墙
            Projectile.timeLeft = 900;//消散时间60=1秒
            Projectile.penetrate = -1;//弹幕打中几个怪物之后会消失
            Projectile.alpha = 255;//弹幕的透明度
            Projectile.light = 0.5f;//弹幕光照的强度
            Projectile.usesLocalNPCImmunity = true;//NPC是不是按照弹幕ID来获取无敌帧？（如果设定为true，玩家发射8个该弹幕同时击中敌人，则八个都能击中，不骗伤，原版夜明弹的反骗伤就是如此）
            Projectile.localNPCHitCooldown = 60;//上一个设定为true则被调用，NPC按照弹幕ID来获取多少无敌帧
            Projectile.usesIDStaticNPCImmunity = false;//NPC是不是按照弹幕类型来获取无敌帧？（如果设定为true，玩家发射8个该弹幕同时击中敌人，则只能击中一次，其余的会穿透，原版用它来控制喽啰的输出上限）
            Projectile.idStaticNPCHitCooldown = 60;//上一个设定为true则被调用，NPC按照弹幕类型来获取多少无敌帧
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,//这里是用来改弹幕图层的
        List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overWiresUI.Add(index);

        public Vector2[] oldVec = new Vector2[30];
        float t = 0;
        Vector2 csd = new Vector2(0);
        public override void OnSpawn(IEntitySource source)
        {
            csd = Projectile.Center;
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 7.5f;
            Player player = Main.player[Projectile.owner];
            t = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero).ToRotation();
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            player.itemTime = 2;
            player.itemAnimation = 2;
            float ndjd = 0;
            csd += Projectile.velocity;
            Projectile.Center = csd;
            Projectile.ai[1] += 2.8f;
            // if (Projectile.ai[1] > 60)Projectile.friendly = true;//是否对敌对NPC造成伤害
            Projectile.ai[0] = 3.14f / 2f + 3.14f / 4f;
            if (Projectile.width < 800)
                Projectile.width =
                Projectile.height += 20;

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
            if (Projectile.ai[1] > 40) Projectile.friendly = true;
            if (Projectile.ai[1] > 140) Projectile.active = false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D 贴图 = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/duaog/wbjex8").Value;
            StripColorFunction stripColor = (x) =>
            new Color(205 - (int)(Projectile.ai[1] * Projectile.ai[1]) / 100
            , 205 - (int)(Projectile.ai[1] * Projectile.ai[1]) / 100
            , 255 - (int)((Projectile.ai[1] + 10) * (Projectile.ai[1] + 10)) / 100
            , 170 - (int)(Projectile.ai[1] * Projectile.ai[1]) / 100);

            StripColorFunction stripColor2 = (x) =>
            new Color(205 - (int)((Projectile.ai[1] + 10) * (Projectile.ai[1] + 10)) / 100
            , 205 - (int)(Projectile.ai[1] * Projectile.ai[1]) / 100
            , 255 - (int)(Projectile.ai[1] * Projectile.ai[1]) / 100
          , 170 - (int)(Projectile.ai[1] * Projectile.ai[1]) / 100);

            float Cd = 80;// new Color(151, 151, 201, 255)
            VertexStrip strip = new VertexStrip();
            VertexStrip strip2 = new VertexStrip();
            VertexStrip strip3 = new VertexStrip();
            Vector2 wz = Projectile.Center;
            Player player = Main.player[Projectile.owner];

            var rotations = oldVec.Zip(oldVec.Skip(1), (a, b) => a - b).Select((a) => a.ToRotation());
            strip.PrepareStrip(
                oldVec,
                rotations.Prepend(rotations.FirstOrDefault()).ToArray(), stripColor, (x) => Projectile.ai[1] * 2,
                -Main.screenPosition + Projectile.Center - Projectile.velocity * 10);

            strip2.PrepareStrip(
               oldVec,
               rotations.Prepend(rotations.FirstOrDefault()).ToArray(), stripColor2, (x) => Projectile.ai[1] * 1.8f,
               -Main.screenPosition + Projectile.Center - Projectile.velocity * 10);
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
            //GameShaders.Armor.Apply(GameShaders.Armor.GetShaderIdFromItemId(3556), Projectile);
            Color color = new Color(255 - (int)(Projectile.ai[1] * 1.275f * 1.4f), 200 - (int)(Projectile.ai[1] * 1.4f), 0, 255 - (int)(Projectile.ai[1] * 1.275f * 1.4f));
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            Main.graphics.GraphicsDevice.Textures[0] = 贴图;
            Main.graphics.GraphicsDevice.BlendState = blendStatef2;
            strip2.DrawTrail();
            strip2.DrawTrail();
            Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            // strip2.DrawTrail();
            strip.DrawTrail();
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
    public class yinhdsz6 : ModProjectile
    {
        public override string Texture => "ArknightsMod/Content/Items/Weapons/Guard/SilverAsh/SilverAshWeapon2";
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            Projectile.NewProjectile(player.GetSource_Death(), target.Center
                 , Projectile.velocity.SafeNormalize(Vector2.Zero), ModContent.ProjectileType<yinhdsz3>(), 0, 0, Main.myPlayer);
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
            if (player.Center.X - Main.MouseWorld.X < 0) sx = false; else sx = true;
            if (sx)
            {
                Projectile.ai[0] = 3.1415f * 1.75f;//初始位置
                t = -(Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero).ToRotation()
                  + Main.rand.Next(-4, 4 + 1) * .1f;//鼠标位置与随机值
                t2 = +Main.rand.Next(-3, 3 + 1) * .1f;
            }
            else
            {
                Projectile.ai[0] = 3.1415f * 1.25f;
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
            player.itemTime = 2;
            player.itemAnimation = 2;
            if (sx)
            {
                for (int i = oldVec.Length - 1; i > 0; i--)
                {
                    oldVec[i] = oldVec[i - 1];
                }
                oldVec[0] = new Vector2((float)Math.Sin(Projectile.ai[0] + t + t2), (float)Math.Cos(Projectile.ai[0] + t - t2)) * 20f;
                Projectile.Center = player.Center + new Vector2((float)Math.Sin(Projectile.ai[0] + t + t2), (float)Math.Cos(Projectile.ai[0] + t - t2)) * 20f;
                if (Projectile.ai[0] <= 3.1415f * (2.5f + .5f))//指定位置
                {
                    Projectile.ai[0] += 0.12f;//旋转
                }
                else
                {
                    Projectile.ai[0] += 0.03f;//到位置减速
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
                oldVec[0] = new Vector2((float)Math.Cos(Projectile.ai[0] + t + t2), (float)Math.Sin(Projectile.ai[0] + t - t2)) * 20f;
                Projectile.Center = player.Center + new Vector2((float)Math.Cos(Projectile.ai[0] + t + t2), (float)Math.Sin(Projectile.ai[0] + t - t2)) * 20f;
                if (Projectile.ai[0] <= 3.1415f * 2.5f)
                {
                    Projectile.ai[0] += 0.12f;
                }
                else
                {
                    Projectile.ai[0] += 0.03f;
                    Projectile.ai[2]++;
                    if (Projectile.ai[2] > 11) Projectile.active = false;
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D 贴图 = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/duaog/wbjexb").Value;
            if (sx) 贴图 = ModContent.Request<Texture2D>("ArknightsMod/Content/Textures/duaog/wbjexa").Value;
            StripColorFunction stripColor = (x) => new Color(151, 151, 151, 105);
            StripColorFunction stripColor2 = (x) => new Color(151, 151, 151, 125);
            float Cd = 15;
            VertexStrip strip = new VertexStrip();
            VertexStrip strip2 = new VertexStrip();
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


            if (oldVec[oldVec.Length - 1] != oldVec[oldVec.Length - 2])
            {            
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

                Main.graphics.GraphicsDevice.BlendState = blendStatef2;

                Main.graphics.GraphicsDevice.Textures[0] = 贴图;
                strip.DrawTrail();
                strip.DrawTrail();
                // strip3.DrawTrail();
                Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                strip2.DrawTrail();
                Texture2D 贴图1 = ModContent.Request<Texture2D>("ArknightsMod/Content/Items/Weapons/Guard/SilverAsh/SilverAshWeapon3").Value;
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                float jd = (oldVec[0]).ToRotation();
                if (sx)
                {
                    Main.spriteBatch.Draw(贴图1, player.Center - Main.screenPosition + new Vector2(0, 5),
                        null, Color.AliceBlue, jd + 3.14f / 3.5f, new Vector2(3, 贴图1.Size().Y - 3), 1f, SpriteEffects.None, 0);//绘制   
                }
                else
                {
                    Main.spriteBatch.Draw(贴图1, player.Center - Main.screenPosition + new Vector2(0, 5),
                        null, Color.AliceBlue, jd - 3.14f - 3.14f / 3.5f, 贴图1.Size() + new Vector2(-3), 1f, SpriteEffects.FlipHorizontally, 0);//绘制
                }
            }
            return false;
        }
    }
}