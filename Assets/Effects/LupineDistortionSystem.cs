using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Assets.Effects
{
    /// <summary>
    /// 仅作为 LupineScarlet 刀光扭曲所用 ScreenSnapshot 与捕获 SpriteBatch 的持有者。
    /// 和 SchwarzDistortionSystem 是同样的安全模式（不挂任何渲染管线 hook，
    /// 抓屏由武器自身 PreDraw 按需进行），单独建一份而不是共用 Schwarz 那份，
    /// 避免两把武器同帧绘制时互相抢占同一张 RenderTarget。
    /// </summary>
    public class LupineDistortionSystem : ModSystem
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
