using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using ArknightsMod.Content.Tiles;

namespace ArknightsMod.Content.Players
{
	internal sealed class ElevatorInputLockPlayer : ModPlayer
	{
		private const string ElevatorScrollLockKey = "ArknightsMod/Elevator";

		public override void PreUpdate()
		{
			// 仅本地玩家需要锁定原版滚轮切栏位逻辑。
			if (Main.myPlayer != Player.whoAmI)
				return;

			if (TEElevator.TryFindNearbyElevatorForPlayer(Player, out _, out _, out _))
				PlayerInput.LockVanillaMouseScroll(ElevatorScrollLockKey);
		}
	}
}
