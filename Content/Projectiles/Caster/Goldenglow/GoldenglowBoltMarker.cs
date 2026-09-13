using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Goldenglow
{
	// 标记某个原版魔法导弹弹幕是否由澄闪发射，用于统计弹幕堆叠数量上限
	public class GoldenglowBoltMarker : GlobalProjectile
	{
		public override bool InstancePerEntity => true;

		public bool IsGoldenglowBolt;

		// 弹幕离玩家超过此距离（像素，约 1125px≈70 格）即自行销毁，避免无目标时无限游荡
		private const float MaxDistanceFromOwner = 1125f;

		public override void AI(Projectile projectile) {
			if (!IsGoldenglowBolt)
				return;

			Player owner = Main.player[projectile.owner];
			if (!owner.active)
				return;

			if (Vector2.Distance(projectile.Center, owner.Center) > MaxDistanceFromOwner) {
				projectile.Kill();
			}
		}
	}
}
