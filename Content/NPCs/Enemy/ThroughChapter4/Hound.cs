using ArknightsMod.Common.Damageclasses;
using ArknightsMod.Content.Items.Material;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Biomes.Desert;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;


namespace ArknightsMod.Content.NPCs.Enemy.ThroughChapter4 //记得把BakaMod改成ArknightsMod
{
	public class Hound : ModNPC
	{
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 5;
		}
		public override void SetDefaults() {
			NPC.aiStyle = -1; //战士ai类型 //蛇蜥怪ai
			NPC.damage = 20; //伤害
			NPC.width = 46; //宽度，我不会填，照着灾厄的血犬填的
			NPC.height = 30; //高度，我不会填，照着灾厄的血犬填的
			NPC.defense = 5; //防御力
			NPC.lifeMax = 55; //最大生命
			NPC.knockBackResist = 0.8f; //击退抗性
			NPC.value = Item.buyPrice(0, 0, 1, 0); //掉的钱
			NPC.HitSound = SoundID.NPCHit1; //受击音效
			Banner = NPC.type;
			NPC.DeathSound = SoundID.NPCDeath5;
			//BannerItem = ModContent.ItemType<旗帜>(); 此处是每击杀50个该NPC掉的旗帜
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Rotdog")
			});
		}
		public override void FindFrame(int frameHeight) {

			if (NPC.velocity.X != 0) {
				if (NPC.frame.Y < 0 * frameHeight) {
					NPC.frame.Y = 0 * frameHeight;
				}
				NPC.frameCounter++;
				NPC.frameCounter += NPC.velocity.Length(); // Make the counter go faster with more movement speed
				if (NPC.frameCounter > 12) {
					NPC.frameCounter = 0;
					NPC.frame.Y += frameHeight;
					if (NPC.frame.Y > 3 * frameHeight) {
						NPC.frame.Y = 0 * frameHeight;
					}
				}
			}
			else {
				NPC.frame.Y = 4 * frameHeight;
			}
		}
		private int fadeTimer;
		public override void OnSpawn(IEntitySource source) {
			fadeTimer = 60; // 持续60帧
			NPC.color = Color.Black; // 初始为纯黑
			NPC.alpha = 240;
		}

		public override void AI() {
			if (fadeTimer > 0) {
				fadeTimer--;
				NPC.alpha = (int)4f * fadeTimer;
				NPC.color = Color.Lerp(Color.Black, Color.White, 1f - fadeTimer / 60f);
			}
			if (NPC.velocity.X != 0) {
				NPC.spriteDirection = -Math.Sign(NPC.velocity.X);
				NPC.rotation = 0; // 如果需要取消旋转效果
			}
			NPC.ai[3]++;
			NPC.ai[2]++;
			NPC.ai[1]++;
			NPC.TargetClosest(true);
			Player p = Main.player[NPC.target];
			float acceleration = 0.04f;
			float maxSpeed = 5f;
			if (NPC.ai[3] <= 200) {

				if (NPC.Center.X > (p.Center.X + 10)) {
					NPC.velocity.X -= acceleration;
					if (NPC.velocity.X > 0)
						NPC.velocity.X -= acceleration;
					if (NPC.velocity.X < -maxSpeed)
						NPC.velocity.X = -maxSpeed;
				}
				if (NPC.Center.X <= (p.Center.X - 10)) {
					NPC.velocity.X += acceleration;
					if (NPC.velocity.X < 0)
						NPC.velocity.X += acceleration;
					if (NPC.velocity.X > maxSpeed)
						NPC.velocity.X = maxSpeed;
				}
				if (NPC.collideX == true && NPC.ai[2] > 100) {
					NPC.velocity.Y = -7f;
					NPC.ai[2] = 0;
				}
				if ((NPC.position.Y - p.position.Y > 80 || HoleBelow()) && NPC.ai[2] > 100) {
					NPC.velocity.Y = -7f;
					NPC.ai[2] = 0;
				}
			}

			if (NPC.ai[3] == 200) {
				NPC.ai[3] = 0;
			}
			if (NPC.ai[3] < 300 && NPC.ai[3] > 200) {

				NPC.velocity.X = 0;

			}
			if (NPC.ai[3] >= 300) {
				NPC.ai[3] = 0;
				NPC.velocity = 5 * (p.Center - NPC.Center).SafeNormalize(Vector2.Zero);
			}
			NPC.direction = NPC.Center.X > p.Center.X ? 0 : 1;
			NPC.spriteDirection = NPC.direction;
		}
		private bool HoleBelow() {
			//width of npc in tiles
			int tileWidth = 4;
			int tileX = (int)(NPC.Center.X / 16f) - tileWidth;
			if (NPC.velocity.X > 0) //if moving right
			{
				tileX += tileWidth;
			}
			int tileY = (int)((NPC.position.Y + NPC.height) / 16f);
			for (int y = tileY; y < tileY + 2; y++) {
				for (int x = tileX; x < tileX + tileWidth; x++) {
					if (Main.tile[x, y].HasTile) {
						return false;
					}
				}
			}
			return true;

		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			//此处写刷怪率，这里写的是正常夜间怪的刷怪率
			return SpawnCondition.OverworldNightMonster.Chance; //此处可以乘上一个因数来调整刷怪率
		}
		public override void HitEffect(NPC.HitInfo hit) {
			//此处写该NPC被击中时产生的效果
			//例子：
			for (int k = 0; k < 5; k++) {
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1f);
			}
			//让该NPC被击中时冒血
			//尸块也要在这里写
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			//这里写掉落物 例子： npcLoot.Add(ItemDropRule.Common(ItemID.LunarBar, d, min, max)); 1/d几率掉落min~max个夜明锭
		}
		public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) {
			if (SpellDamageConfig.SpellProjectiles.Contains(projectile.type)) {
				// 法术伤害无视护甲
				modifiers.ScalingArmorPenetration += 1f;
				// 0.95倍伤害减免
				modifiers.FinalDamage *= 0.75f;

			}
		}
	}
}
