using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs
{
	public class SurtrLaevatainS3Player : ModPlayer
	{
		public const int MaxLifeBonus = 1200;

		// 持续掉血用的累加器
		public float S3DrainAccumulator;

		// 本次激活是否已经回过满血；buff 消失时重置，下次激活才能再触发
		public bool S3HealDone;
		public bool S3HealPending;
		public int S3LastFinalMaxLife;

		private bool wasS3Active;

		public override void UpdateDead()
		{
			// 死亡时移除 S3 buff，防止残留到复活后
			int s3BuffType = ModContent.BuffType<SurtrLaevatainS3Buff>();
			for (int i = 0; i < Player.MaxBuffs; i++)
			{
				if (Player.buffType[i] == s3BuffType)
				{
					Player.DelBuff(i);
					// 死亡移除也是 buff 消失的一种，走同一套清理
					OnS3BuffRemoved();
					wasS3Active = false;
					break;
				}
			}
		}

		public override void PostUpdate()
		{
			bool nowActive = Player.HasBuff<SurtrLaevatainS3Buff>();
			// 活着的时候 buff 消失（比如玩家手动右键取消 buff）在这里兜底清理
			if (wasS3Active && !nowActive)
				OnS3BuffRemoved();
			if (S3HealPending && !S3HealDone && nowActive)
			{
				S3HealPending = false;
				S3HealDone = true;
				Player.statLife = Player.statLifeMax2;
			}

			// 记录本帧最终生命上限，供 S3 掉血用（掉血在 UpdateBuffs 里算时，
			// 其他加成的上限还没叠上来）。PostUpdate 是帧内最早能拿到完整值的时机。
			S3LastFinalMaxLife = Player.statLifeMax2;

			wasS3Active = nowActive;
		}

		private void OnS3BuffRemoved()
		{
			// 回满血标记和掉血累加器都跟着 buff 走：buff 没了就清掉，下次挂上重新来
			S3HealDone = false;
			S3HealPending = false;
			S3DrainAccumulator = 0f;

			// 同步清掉 WeaponPlayer 里的技能激活状态。不然死亡复活后 buff 已经没了，
			// 技能条却还亮着、攻击还保持 S3 强化形态。
			// 只清 S3 的状态：如果玩家已经切了武器或切了技能位，那是别的技能的状态，不能动。
			var wp = Player.GetModPlayer<WeaponPlayer>();
			if (wp.HoldSurtrLaevatain && wp.Skill == 2)
			{
				wp.SkillActive = false;
				wp.SkillTimer = 0;
			}
		}
	}

	/// <summary>
	/// S3「黄昏」的持续效果：生命上限+1200 并回满血（都在下面 Update 里做，原因见 Update 内注释），
	/// 并按持续时间线性爬坡扣血，90 秒爬满后稳定在每秒扣除最大生命 10%，扣到 0 会真正触发死亡。
	/// 用 buffTime 自己记录经过了多久，不依赖 WeaponPlayer.Skill/SkillActive——那两个字段切换武器就会被重置，
	/// 而这个 buff 一旦挂上就只能靠死亡结束，换武器免疫不了。
	/// </summary>
	public class SurtrLaevatainS3Buff : ModBuff
	{
		public override string Texture => "ArknightsMod/Content/Buffs/SurtrLaevatain_3_buff";

		public const int InitialDuration = int.MaxValue;
		private const int RampSeconds = 90; // 90 秒内扣血速率线性爬满
		private const float MaxDrainPerSecond = 0.10f; // 爬满后每秒扣除的最大生命值比例

		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = true;
			Main.debuff[Type] = false; // 主动技能带来的效果，不是敌对负面状态
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.dead)
			{
				player.DelBuff(buffIndex);
				buffIndex--;
				return;
			}

			var mp = player.GetModPlayer<SurtrLaevatainS3Player>();
			player.statLifeMax2 += SurtrLaevatainS3Player.MaxLifeBonus;
			if (!mp.S3HealDone)
				mp.S3HealPending = true;

			int elapsedTicks = InitialDuration - player.buffTime[buffIndex];
			float rampProgress = MathHelper.Clamp(elapsedTicks / 60f / RampSeconds, 0f, 1f);
			int maxLifeForDrain = mp.S3LastFinalMaxLife > 0 ? mp.S3LastFinalMaxLife : player.statLifeMax2;
			float drainPerTick = rampProgress * MaxDrainPerSecond * maxLifeForDrain / 60f;

			mp.S3DrainAccumulator += drainPerTick;
			if (mp.S3DrainAccumulator < 1f)
				return;

			int damage = (int)mp.S3DrainAccumulator;
			mp.S3DrainAccumulator -= damage;

			if (player.statLife - damage <= 0)
			{
				player.statLife = 0;
				player.KillMe(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral(player.name + " 被黄昏的力量吞噬了")), damage, 0);
			}
			else
			{
				player.statLife -= damage;
			}
		}
	}
}
