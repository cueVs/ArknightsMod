using ArknightsMod.Common;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using WisdelItem = ArknightsMod.Content.Items.Weapons.Sniper.Wisadel.WisadelCannon;

namespace ArknightsMod.Content.Projectiles.Sniper.Wisadel
{
	public static class WisdelProbeHelper
	{
		public static WisdelProbePlayer wisdel(this Player player)
		{
			return player.GetModPlayer<WisdelProbePlayer>();
		}
	}
	public class WisdelProbePlayer : ModPlayer
	{
		/// <summary>
		/// 当前正在使用的浮游炮
		/// </summary>
		public int currentUse;

		/// <summary>
		/// 攻击冷却
		/// </summary>
		public int coolDown;

		/// <summary>
		/// 模式
		/// </summary>
		public int mode;

		/// <summary>
		/// 模式切换冷却
		/// </summary>
		public int modeSwitchCooldown;

		/// <summary>
		/// 蓄力时间
		/// </summary>
		public int channelTimer;

		public int combineAnimationTimer;
		public override void PostUpdate()
		{
			if (coolDown > 0)
				coolDown--;

			if (modeSwitchCooldown > 0)
				modeSwitchCooldown--;

			if (combineAnimationTimer > 0) {
				combineAnimationTimer--;
			}
		}

		public float ShakeTime;
		public float ShakeIntensity = 1;
		private int screenshakeTimer = 0;
		public override void ModifyScreenPosition() {
			if (!Main.gameMenu) {
				screenshakeTimer++;
				if (ShakeTime >= 0 && screenshakeTimer >= 20)
				{
					ShakeTime -= 0.5f;
				}
				if (ShakeTime <= 0) {
					ShakeTime = 0;
					ShakeIntensity = 1;
				}
				Main.screenPosition += new Vector2(ShakeTime * Main.rand.NextFloat() * ShakeIntensity, ShakeTime * Main.rand.NextFloat() * ShakeIntensity); //NextFloat creates a random value between 0 and 1, multiply screenshake amount for a bit of variety
			}
			else
			{
				ShakeTime = 0;
				ShakeIntensity = 1;
				screenshakeTimer = 0;
			}
		}
		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
		{
			if (Player.HeldItem.type == ModContent.ItemType<WisdelItem>() && mode == 1)
			{
				float offset = EaseFunction.Ease(channelTimer, 0, 20, 0, 2);
				offset = MathHelper.Clamp(offset, 0, 2);
				if (Player.gravDir < 0) {
					drawInfo.Position.Y += offset;
				}
				Player.headPosition.Y += offset * Player.gravDir;
				Player.bodyPosition.Y += offset * Player.gravDir;
			}
		}
	}
}
