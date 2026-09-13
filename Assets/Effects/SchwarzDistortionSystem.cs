using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Assets.Effects
{
    /// <summary>
    /// 仅作为 SchwarzArrow 扭曲所用 ScreenSnapshot 与捕获 SpriteBatch 的持有者。
    /// 不挂任何渲染管线 hook —— 在 EndCapture / PostDrawTiles 等位置切 RT 风险太大。
    /// 抓屏由 SchwarzArrow.PreDraw 在自身渲染时按需进行。
    /// </summary>
    public class SchwarzDistortionSystem : ModSystem
    {
        public static RenderTarget2D ScreenSnapshot;
        public static SpriteBatch CaptureBatch;

        public override void Unload()
        {
            Main.QueueMainThreadAction(() =>
            {
                ScreenSnapshot?.Dispose();
                ScreenSnapshot = null;
                CaptureBatch?.Dispose();
                CaptureBatch = null;
            });
        }
    }
}
