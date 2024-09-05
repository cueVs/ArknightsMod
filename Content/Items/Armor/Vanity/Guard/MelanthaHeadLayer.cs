using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Vanity.Guard
{
    internal class MelanthaHeadLayer : PlayerDrawLayer
    {
		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			// The layer will be visible only if the player is holding an ExampleItem in their hands. Or if another modder forces this layer to be visible.
			return drawInfo.drawPlayer.HeldItem?.type == ModContent.ItemType<MelanthaHead>();

			// If you'd like to reference another PlayerDrawLayer's visibility,
			// you can do so by getting its instance via ModContent.GetInstance<OtherDrawLayer>(), and calling GetDefaultVisibility on it
		}

		public override Position GetDefaultPosition()
        {
			return new AfterParent(PlayerDrawLayers.Head);

		}

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            var drawPlayer = drawInfo.drawPlayer;
            var texture = ModContent.Request<Texture2D>("ArknightsMod/Content/Items/Armor/Vanity/Guard/MelanthaHead_Back", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            for (int i = 0; i < drawInfo.DrawDataCache.Count; i++)
            {
                DrawData d = drawInfo.DrawDataCache[i];
                if (d.texture == texture)
                {
                    if (d.sourceRect.HasValue)
                    {
                        int frame = drawPlayer.bodyFrame.Y / drawPlayer.bodyFrame.Height;

                        Rectangle rectangle = d.sourceRect.Value;
                        rectangle.Width = 55;
                        rectangle.Height = 68;
                        rectangle.Y = rectangle.Height * frame;
                        d.sourceRect = rectangle;
                        if (drawPlayer.dead)
                            d.position = Vector2.Zero;
                        else
                            d.position = drawPlayer.position - Main.screenPosition +
                                (drawPlayer.direction == 1 ? new Vector2(-8f, -1.5f) : new Vector2(12f, -1.5f));
                        drawInfo.DrawDataCache[i] = d;
                        break;
                    }
                }
            }
        }
    }
}