using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Systems.Gameplay.Elemental
{
	public partial class ElementalSystem : ModSystem
	{
		public interface IBurstHandler {
			void OnBurst(int entityWhoAmI, EntityType entityType);
		}

		public static readonly IBurstHandler[] _burstHandlers = new IBurstHandler[] {
			new NeuronBurstHandler(),
			//new CorrosiveBurstHandler(),
			//new BurnBurstHandler()

			//0, 1, 2, 3
		};

		private struct NeuronBurstHandler : IBurstHandler {
			public void OnBurst(int playerWhoAmI, EntityType entityType) {
				if (entityType != 0)
					return;
				Main.npc[playerWhoAmI].AddBuff(31, 90);
				Main.npc[playerWhoAmI].AddBuff(23, 240);
				//Player.Hurt(PlayerDeathReason.ByCustomReason("精神崩溃"), 200, 1, false, false, 0, true, 1000, 1000, 0);
				
				//SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/Madness") with { Volume = 1f, Pitch = 0f }, Player.Center);
				
				//Projectile.NewProjectile(newSource, Player.Center + new Vector2(100, 180), new Vector2(0, 0), ModContent.ProjectileType<SanCrash>(), 0, 0);
			}

		}
		// apply npcs & enemies burst buff

		public static void RegenerateElemental(int WhoAmI) //后续移动到elementallogic 完成
		{
			//can use bm1
			ElementalData elemData = elementalRecords[WhoAmI].elementalData;
			byte mask = elemData.Status;
			//不能治疗满血或元素爆发中的单位
			if ((mask & 0x20) == 0 && (mask & 0x20) != 0 && (mask & 0x20) != 0) {
				return;
			}
			elemData.ApplyHealingAll(1);
		}
	}
}