using ArknightsMod.Content.Projectiles.Specialist.Scene;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs.Specialist.Scene
{
	public class CameraTruckBuff : ModBuff
	{
		// 暂时复用武器贴图作为 buff 图标。
		public override string Texture => "ArknightsMod/Content/Items/Weapons/Specialist/Scene/SceneCamera";

		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			if (player.ownedProjectileCounts[ModContent.ProjectileType<CameraTruck>()] > 0)
				player.buffTime[buffIndex] = 18000;
			else {
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}
