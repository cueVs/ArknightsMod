using ArknightsMod.Content.Projectiles.Caster.Goldenglow;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs
{
	// 浮游信标：标记玩家当前部署了多少个澄闪的浮游单元，右键可一键取消全部召唤
	public class GoldenglowBeaconBuff : ModBuff
	{
		public override void SetStaticDefaults() {
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			player.buffTime[buffIndex] = 18000;
		}

		public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare) {
			int count = player_BeaconCount;
			tip = Language.GetTextValue("Mods.ArknightsMod.Buffs.GoldenglowBeaconBuff.Description", count);
		}

		private int player_BeaconCount => Main.LocalPlayer.ownedProjectileCounts[ModContent.ProjectileType<GoldenglowBeacon>()];

		public override bool RightClick(int buffIndex) {
			Player player = Main.LocalPlayer;
			foreach (Projectile proj in Main.ActiveProjectiles) {
				if (proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<GoldenglowBeacon>()) {
					proj.Kill();
				}
			}
			return true;
		}
	}
}
