using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Guard.Beehunter
{
	// 盾牌背部图层：在玩家躯干图层之前渲染，使盾牌显示在玩家背后
	public class BeehunterShieldLayer : PlayerDrawLayer
	{
		public override Position GetDefaultPosition() =>
			new BeforeParent(PlayerDrawLayers.Torso);

		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Player player = drawInfo.drawPlayer;

			// 仅在手持蛇屠箱盾牌球棒时显示
			if (player.HeldItem.ModItem is not BeehunterBag) return;
			if (player.dead || !player.active) return;

			Texture2D tex = ModContent.Request<Texture2D>(
				"ArknightsMod/Content/Items/Weapons/Guard/Beehunter/BeehunterShield").Value;

			Vector2 origin = tex.Size() / 2f;
			// 盾牌居中于玩家，向背部（反方向）偏移，露出更多
			Vector2 pos = drawInfo.Center - Main.screenPosition
				+ new Vector2(-player.direction * 16f, 2f);

			DrawData data = new DrawData(
				tex,
				pos,
				null,
				Lighting.GetColor((int)(player.Center.X / 16), (int)(player.Center.Y / 16)),
				0f,
				origin,
				1f,
				player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
				0);

			drawInfo.DrawDataCache.Add(data);
		}
	}
}
