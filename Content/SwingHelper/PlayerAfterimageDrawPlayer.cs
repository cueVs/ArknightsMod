using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace ArknightsMod.Content.SwingHelper
{
	public sealed class PlayerAfterimageDrawPlayer : ModPlayer
	{
		private static bool isDrawingAfterimages;
		private Vector2[] positions = Array.Empty<Vector2>();
		private int positionCount;
		private float shadow;
		private bool drawQueued;
		private bool isAfterimageClone;
		private float afterimageOpacity;

		public void QueueAfterimages(Vector2[] sourcePositions, int count, float shadow) {
			if (sourcePositions == null || count <= 0) {
				drawQueued = false;
				return;
			}

			count = Math.Min(count, sourcePositions.Length);
			if (positions.Length < count)
				Array.Resize(ref positions, count);

			Array.Copy(sourcePositions, positions, count);
			positionCount = count;
			this.shadow = shadow;
			drawQueued = true;
		}

		public override void TransformDrawData(ref PlayerDrawSet drawInfo) {
			if (!isAfterimageClone)
				return;

			// 完整玩家层已经生成后，再统一降低每一层的颜色和 Alpha。
			// 不能把透明度交给 DrawPlayer 的 shadow 参数，那个参数会主动跳过裸头等层。
			for (int i = 0; i < drawInfo.DrawDataCache.Count; i++) {
				DrawData drawData = drawInfo.DrawDataCache[i];
				drawData.color *= afterimageOpacity;
				drawInfo.DrawDataCache[i] = drawData;
			}
		}

		public override void DrawPlayer(Camera camera) {
			if (isDrawingAfterimages || !drawQueued || positionCount <= 0)
				return;

			drawQueued = false;
			isDrawingAfterimages = true;
			try {
				Player clone = Player.clientClone();
				clone.CopyVisuals(Player);
				for (int i = 0; i < clone.armor.Length; i++)
					clone.armor[i] = Player.armor[i].Clone();
				for (int i = 0; i < clone.dye.Length; i++)
					clone.dye[i] = Player.dye[i].Clone();

				clone.ResetEffects();
				clone.ResetVisibleAccessories();
				clone.invis = false;
				clone.UpdateDyes();
				clone.DisplayDollUpdate();
				clone.skipAnimatingValuesInPlayerFrame = true;
				clone.PlayerFrame();
				clone.skipAnimatingValuesInPlayerFrame = false;
				clone.immuneAlpha = 0;
				clone.shimmerTransparency = 0f;
				clone.heldProj = -1;

				var cloneAfterimage = clone.GetModPlayer<PlayerAfterimageDrawPlayer>();
				cloneAfterimage.isAfterimageClone = true;
				cloneAfterimage.afterimageOpacity = MathHelper.Clamp(1f - shadow, 0f, 1f);

				for (int i = 0; i < positionCount; i++) {
					if (positions[i] == Vector2.Zero)
						continue;

					Vector2 offset = positions[i] - (Player.position + new Vector2(0f, Player.gfxOffY));
					clone.position = Player.position + offset;
					clone.itemLocation = Player.itemLocation + offset;
					Main.PlayerRenderer.DrawPlayer(camera, clone, positions[i], 0f,
						new Vector2(Player.width * 0.5f, Player.height), 0f);
				}
			}
			finally {
				isDrawingAfterimages = false;
			}
		}
	}
}
