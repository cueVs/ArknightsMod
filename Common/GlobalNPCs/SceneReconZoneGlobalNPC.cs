using ArknightsMod.Content.Items.Weapons.Specialist.Scene;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Common.GlobalNPCs
{
	// 稀音技能二「全景过载摄像」：处于摄影车侦查圈内的敌人，受到该玩家造成的伤害 +150%。
	public class SceneReconZoneGlobalNPC : GlobalNPC
	{
		public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers) {
			if (SceneCameraSkills.IsEnemyInReconZone(player, npc))
				modifiers.SourceDamage *= 1f + SceneCameraSkills.ReconDamageBonus;
		}

		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers) {
			if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
				return;
			Player player = Main.player[projectile.owner];
			if (player.active && SceneCameraSkills.IsEnemyInReconZone(player, npc))
				modifiers.SourceDamage *= 1f + SceneCameraSkills.ReconDamageBonus;
		}
	}
}
