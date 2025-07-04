using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader.Utilities;
using Terraria.Localization;
using System;
using ArknightsMod.Content.Items;
using ArknightsMod.Common.VisualEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using static Terraria.ModLoader.ModContent;
using ArknightsMod.Common.Damageclasses;
using ArknightsMod.Content.Projectiles;

namespace ArknightsMod.Content.NPCs.Enemy.ThroughChapter4
{
	public class Crossbowman : ModNPC
	{
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 19;

			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Velocity = 1f
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
		}

		public override void SetDefaults() {
			NPC.lifeMax = 45;
			NPC.damage = 4;
			NPC.defense = 5;
			NPC.knockBackResist = 0.5f;//���˿��ԣ�0fΪ��ߣ�1fΪ���
			NPC.width = 32;
			NPC.height = 56;
			NPC.aiStyle = 0;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.value = 60f;
			NPC.friendly = false;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {

			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Material.Device>(), 8, 1, 1));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Material.Polyketon>(), 8, 1, 1));

		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return SpawnCondition.OverworldNightMonster.Chance * 0.1f;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.ArknightsMod.Bestiary.Crossbowman")),
			});
		}

		public override bool? CanBeHitByItem(Player player, Item item)//�޵�֡
		{
			return null;
		}

		public override bool? CanBeHitByProjectile(Projectile Projectile)//�����з���Ļ������Դ��Ļ����&����
		{
			if (Projectile.hostile == true) {
				return false;
			}
			else {
				return Projectile.friendly == true ? null : false;
			}
		}

		public override void HitEffect(NPC.HitInfo hit) {
			for (int i = 0; i < 10; i++) {
				int dustType = DustID.RedMoss;
				var dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType);
				dust.velocity.X += Main.rand.NextFloat(-0.05f, 0.05f);
				dust.velocity.Y += Main.rand.NextFloat(-0.05f, 0.05f);
				dust.scale *= 1f + Main.rand.NextFloat(-0.03f, 0.03f);
			}
		}

		private float timer = 180;
		private int atkloop = 180;
		private int atkrange = 320;
		private int escrange = 160;
		private bool iswalk = false;
		private bool isatk = false;
		private bool isescape = false;
		private bool isstuck = false;
		private int walkframe;
		private int atkframe;
		private int framecount;
		private int jumpstage;
		private float jumpspeed;
		private int jumptimes;
		private float distance;
		private float diffX;
		private float diffY;
		private float ax;
		private float vx;
		private float escapetimer = 0;
		private float escapetime;
		private int attacktimer;
		private float angle;
		private int directionchoose;

		public override void AI() {
			Player Player = Main.player[NPC.target];
			diffX = Player.Center.X - NPC.Center.X;
			diffY = Player.Center.Y - NPC.Center.Y;
			ax = 0.3f;
			distance = (float)Math.Sqrt(Math.Pow(diffX / 16, 2) + Math.Pow(diffY / 16, 2));//����ҵľ��루������
			if (NPC.velocity.X != 0) {
				NPC.spriteDirection = Math.Sign(NPC.velocity.X);
				NPC.rotation = 0; 
			}
			if (Main.masterMode) {
				atkloop = 90;
				atkrange = 30;
				escrange = 15;
				jumpspeed = 6f;
				escapetime = 300;
				vx = 3f;
			}
			else if (Main.expertMode) {
				atkloop = 120;
				atkrange = 20;
				escrange = 10;
				jumpspeed = 5f;
				escapetime = 240;
				vx = 2.5f;
			}
			else {
				atkloop = 180;
				atkrange = 15;
				escrange = 10;
				jumpspeed = 4f;
				escapetime = 180;
				vx = 2f;
			}
			//�������
			if (!Player.active || Player.dead) {
				NPC.TargetClosest(false);
				if (Player.dead) {
					Escape();
					return;
				}
			}
			
			//ģʽѡ��
			if (distance >= atkrange) {//������Χ֮��
				Walk();
			}
			else if (distance >= escrange + 2) {//���ܷ�Χ֮��
				timer++;
				if (timer >= atkloop) {
					Attack();
					//timer = 0;
				}
				else {
					Walk();
					//attacktimer = 0;
				}
			}
			else if (distance >= escrange) {//�м䷶Χ��ֻ�������ƶ�
				timer++;
				NPC.velocity.X = float.Lerp(NPC.velocity.X, 0, 0.1f);
				if (timer >= atkloop) {
					Attack();
					//timer = 0;
				}
				else {
					//attacktimer = 0;
				}
			}
			else {//���ܷ�Χ֮��
				timer++;
				if (timer >= atkloop) {
					Attack();
					//timer = 0;
				}
				else {
					Escape();
					//attacktimer = 0;
				}
			}
		}

		private void Walk() {//��������������ٶ�Ϊ0ʱ�����ٶ�ҲΪ0��������Ծ����Ծ���κ󷴷�������
			Player Player = Main.player[NPC.target];
			iswalk = true;
			isatk = false;
			isescape = false;
			if (NPC.velocity.X == 0) {
				isstuck = true;
				Vector2 velDiff = NPC.velocity - Player.velocity;
				int haltDirectionX = velDiff.X > 0 ? 1 : -1;
				float haltPointX = NPC.Center.X + haltDirectionX * (velDiff.X * velDiff.X) / (2 * ax);
				if (Player.Center.X > haltPointX) {
					NPC.velocity.X += ax;
				}
				else {
					NPC.velocity.X -= ax;
				}
				NPC.velocity.X = Math.Min(vx, Math.Max(-vx, NPC.velocity.X));
				if (NPC.velocity.Y == 0) {
					jumpstage += 1;
					if (jumpstage >= 1) {
						NPC.velocity.Y -= jumpspeed;
						jumpstage = 0;
						jumptimes += 1;
						if (jumptimes >= 3) {
							jumptimes = 0;
							//escapetimer = 0;
							Escape();
							return;
						}
					}
				}
			}
			else {
				isstuck = false;
				jumptimes = 0;
				jumpstage = 0;
				Vector2 velDiff = NPC.velocity - Player.velocity;
				int haltDirectionX = velDiff.X > 0 ? 1 : -1;
				float haltPointX = NPC.Center.X + haltDirectionX * (velDiff.X * velDiff.X) / (2 * ax);
				if (Player.Center.X > haltPointX) {
					NPC.velocity.X += ax;
				}
				else {
					NPC.velocity.X -= ax;
				}
				NPC.velocity.X = Math.Min(vx, Math.Max(-vx, NPC.velocity.X));
				return;
			}
		}
		private int jumpCD;
		private void Escape() {//��Զ����ҷ����ߣ���ѡ�񹥻��򲻹���
			escapetimer++;
			isescape = true;
			iswalk = false;
			isatk = false;
			isstuck = false;
			if (escapetimer <= escapetime) {
				jumpCD++;
				Player Player = Main.player[NPC.target];
				Vector2 velDiff = NPC.velocity - Player.velocity;
				int haltDirectionX = velDiff.X > 0 ? 1 : -1;
				float haltPointX = NPC.Center.X + haltDirectionX * (velDiff.X * velDiff.X) / (2 * ax);
				if (Player.Center.X > haltPointX) {
					NPC.velocity.X -= ax;
				}
				else {
					NPC.velocity.X += ax;
				}
				NPC.velocity.X = Math.Min(vx, Math.Max(-vx, NPC.velocity.X));
				if (NPC.velocity.Y == 0) {
					jumpstage += 1;

					if (jumpstage >= 1&&jumpCD>=60) {
						NPC.velocity.Y -= jumpspeed;
						jumpstage = 0;
						jumpCD = 0;
						jumptimes += 1;
						if (jumptimes >= 3) {
							jumptimes = 0;
							Walk();
							return;
						}
					}
				}
			}
			else {
				escapetimer = 0;
				return;
			}
		}

		private void Attack() {
			var newSource = NPC.GetSource_FromThis();
			Player Player = Main.player[NPC.target];
			attacktimer++;
			if (attacktimer <= 30) {
				NPC.velocity.X = float.Lerp(NPC.velocity.X, 0, 0.1f);
			}
			else if (attacktimer <= 70) {
				//NPC.velocity = Vector2.Zero;
				isatk = true;
				iswalk = false;
				isescape = false;
				isstuck = false;
				if (attacktimer == 50) {
					directionchoose = Player.Center.X - NPC.Center.X >= 0 ? 1 : -1;
					angle = (float)Math.Atan((Player.Center.Y - NPC.Center.Y) / (Player.Center.X - NPC.Center.X));
					Projectile.NewProjectile(newSource, NPC.Center, new Vector2(directionchoose * 8f, 0).RotatedBy(angle), ModContent.ProjectileType<CrossbowmanBolt>(), 12, 0.8f, 0, 0);
					SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/Crossbow") with { Volume = 1f, Pitch = 0f }, NPC.Center);
				}
			}
			else {
				isatk = false;
				isstuck = true;
				timer = 0;
				attacktimer = 0;
				return;
			}
		}

		public override void FindFrame(int frameHeight) {
			frameHeight = 56;
			if (iswalk == true || isescape == true) {
				walkframe++;
				framecount = walkframe / 20;
				if (framecount > 13) {
					walkframe = 0;
				}
				NPC.frame.Y = framecount * frameHeight;
			}
			if (isstuck == true) {
				NPC.frame.Y = 14 * frameHeight;
			}
			if (isatk == true) {
				atkframe++;
				framecount = atkframe / 10 + 15;
				NPC.frame.Y = framecount * frameHeight;
				if (framecount > 18) {
					NPC.frame.Y = 15 * frameHeight;
					atkframe = 0;
				}
			}
		}
		public override void OnKill() {
			SoundStyle ghostSound = SoundID.NPCDeath6 with {
				Pitch = -0.4f, // ��Χ[-1.0, 1.0]��-0.5��ʾ���Ͱ˶�
				Volume = 0.6f  // ��ѡ��������
			};
			SoundEngine.PlaySound(ghostSound, NPC.Center);
			for (int i = 0; i < 25; i++) // ��������
	{
				// 70%�������ɺ�ɫ��30%�������ɳ�ɫ
				bool isBlack = Main.rand.NextFloat() < 0.7f;

				Dust dust = Dust.NewDustPerfect(
					NPC.Center,
					isBlack ? DustID.Asphalt : DustID.FireworksRGB, // ��ɫ���ɫ
					Main.rand.NextVector2Circular(5, 5),
					Alpha: 150,
					Scale: Main.rand.NextFloat(1.2f, 2f)
				);

				// ͳһ�������
				dust.noGravity = true;
				dust.fadeIn = 1.5f;
				dust.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
			}
		}
		public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) {
			if (SpellDamageConfig.SpellProjectiles.Contains(projectile.type)) {
				// �����˺����ӻ���
				modifiers.ScalingArmorPenetration += 1f;
				// 0.95���˺�����
				modifiers.FinalDamage *= 1f;

				for (int i = 0; i < 3; i++) {
					Dust.NewDust(NPC.position, NPC.width, NPC.height,
						DustID.MagicMirror, 0, 0, 150, Color.LightBlue, 0.7f);
				}
			}
		}
	}
	public class CrossbowmanBolt : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 1;
		}
		public override void SetDefaults() {
			Projectile.width = 26;
			Projectile.height = 6;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 1;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = false;
			Projectile.timeLeft = 640;
			Projectile.alpha = 0;
			Projectile.damage = 12;
			Projectile.light = 0.6f;
			Projectile.friendly = false;
			Projectile.hostile = true;
		}
		//public override bool PreDraw(ref Color lightColor) {
		//	Color A = new Color(255, 255, 255);
		//	Color B = new Color(235, 235, 235);
		//	Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/FlameTrail").Value;
		//	TrailProjectileMaker.ProjectileDrawTail(Projectile, trailtexture, new Vector2(0, 0), A, B, 15f, true);
		//	return true;
		//}

		public override void PostDraw(Color lightColor) {
			Texture2D lightsTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/ThroughChapter4/CrossbowmanBolt").Value;
			Main.EntitySpriteDraw(lightsTexture, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, lightsTexture.Width, lightsTexture.Height), Color.White, Projectile.rotation, new Vector2(lightsTexture.Width / 2, lightsTexture.Height / 2), 1f, SpriteEffects.None, 0);
		}

		public override void AI() {
			Projectile.velocity = Vector2.Lerp(Projectile.velocity, 0.833f * Projectile.velocity, 0.01f);
			Projectile.rotation = Projectile.velocity.ToRotation();
			Dust dust;
			Vector2 position = Projectile.Center + new Vector2(0, 3);
			dust = Terraria.Dust.NewDustPerfect(position, 279, new Vector2(0f, 0f), 0, new Color(255, 255, 255), 1f);
		}
	}
}