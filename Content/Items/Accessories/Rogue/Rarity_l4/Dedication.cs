using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using ArknightsMod.Content.Projectiles.Rogue.Dedication;
using ArknightsMod.Content.Projectiles.Rogue.Dedication.ShootProj;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l4
{
	public class Dedication : ModItem
	{


		public override void SetDefaults() {
			Item.width = 28;
			Item.height = 30;
			Item.accessory = true;
			Item.value = Item.sellPrice(0, 16, 0, 0);
			Item.rare = ItemRarityID.Master;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
	
			player.GetModPlayer<DedicationPlayer>().hasDedication = true;
		}

		
	}


	public class DedicationPlayer : ModPlayer
	{
		public bool hasDedication = false;

		public override void ResetEffects() {
			hasDedication = false;
		}


		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
		
			if (hasDedication && proj.friendly && proj.DamageType == DamageClass.Ranged && !proj.minion && !proj.sentry) {
				Vector2 hitPosition = target.Center;

			if (proj.type == ModContent.ProjectileType<DedicationTrackingProj>()) return;
				
				float range = 650f;
				List<NPC> nearbyNPCs = new List<NPC>();

				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					if (npc.active && !npc.friendly && npc.lifeMax > 5 && Vector2.Distance(hitPosition, npc.Center) <= range) {
						nearbyNPCs.Add(npc);
					}
				}

		
				if (nearbyNPCs.Count > 0) {
					NPC targetNPC = nearbyNPCs[Main.rand.Next(nearbyNPCs.Count)];
					Vector2 spawnPosition = targetNPC.Center;

					var source = Player.GetSource_FromThis();
					Projectile.NewProjectile(source, spawnPosition, Vector2.Zero,
						ModContent.ProjectileType<DedicationTrackingProj>(), 150, 5f, Player.whoAmI);
				}
			}
		}
	}


}