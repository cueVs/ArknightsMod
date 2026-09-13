using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.NPCs.Enemy.OF.Pmp
{
	/// <summary>
	/// 庞贝穿墙跳跃路径上留下的短暂火焰，可对玩家造成伤害和灼烧
	/// </summary>
	public class PmpTrailFlame : ModProjectile
	{
		public override string Texture => "ArknightsMod/Content/NPCs/Enemy/OF/Pmp/PmpFireBall";

		public override void SetDefaults()
		{
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 40;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.aiStyle = 0;
			Projectile.damage = 15;
			Projectile.alpha = 80;
		}

		public override void AI()
		{
			Projectile.velocity *= 0f;
			Projectile.Opacity = (float)Projectile.timeLeft / 40f;

			for (int i = 0; i < 2; i++)
			{
				int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
					DustID.FlameBurst, Main.rand.NextFloatDirection() * 1.5f, -Main.rand.NextFloat(0.5f, 2f), 80, default, 1.2f);
				Main.dust[d].noGravity = true;
			}
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
		{
			target.buffImmune[BuffID.OnFire] = false;
			int existingTime = 0;
			for (int i = 0; i < Player.MaxBuffs; i++)
			{
				if (target.buffType[i] == BuffID.OnFire)
				{
					existingTime = target.buffTime[i];
					break;
				}
			}
			target.AddBuff(BuffID.OnFire, existingTime + 180); // 叠加3秒灼烧
		}
	}
}
