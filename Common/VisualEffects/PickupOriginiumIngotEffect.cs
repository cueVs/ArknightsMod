using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Common.VisualEffects
{
	public class PickupOriginiumIngotEffect : PlayerDrawLayer
	{
		public static Asset<Texture2D> EffectTexture;

		public override void Load() => EffectTexture = ModContent.Request<Texture2D>((GetType().Namespace + "." + Name).Replace('.', '/'));

		public override Position GetDefaultPosition() => PlayerDrawLayers.AfterLastVanillaLayer;

		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Player player = drawInfo.drawPlayer;
			if (!player.TryGetModPlayer<InventoryPlayer>(out var inventoryPlayer) || inventoryPlayer.PickupOriginiumIngotEffectCount.Count <= 0)
				return;

			var count = inventoryPlayer.PickupOriginiumIngotEffectCount;
			for (int i = 0; i < count.Count; i++) {
				Texture2D texture = EffectTexture.Value;
				Vector2 drawOffset = new(6, 28 - (28 + 48 - count[i] * 2) * player.gravDir);
				Vector2 vector = drawInfo.bodyVect;

				int frameY = 4 - count[i] / 6;
				Rectangle frame = texture.Frame(1, 4, 0, frameY);

				DrawData drawData =
					new(texture,
					vector - Main.screenPosition + drawOffset + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2) - new Vector2(player.width, player.height),
					frame,
					Color.White * (0.8f * (count[i] / 25f) + 0.2f),
					0f,
					vector - Main.screenPosition,
					1f,
					player.gravDir > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically);
				drawInfo.DrawDataCache.Add(drawData);
			}
		}
	}
}
