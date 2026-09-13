using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace ArknightsMod.Content.SwingHelper
{
    public enum ScaleAxis
    {
        X,
        Y
    }
    /// <summary>
    /// 变换辅助类
    /// </summary>
    public static class TransformHelper
    {
        /// <summary>
        /// 计算为保持视觉比例不变所需的倾斜角度
        /// </summary>
        /// <param name="player">弹幕的主人</param>
        /// <param name="scale">缩放比例</param>
        /// <param name="axis">缩放的轴</param>
        /// <returns>需要倾斜的角度（弧度）</returns>
        public static float CalculateTiltAngle(Player player, float scale)
        {
            float tanTheta = 0f;
            Texture2D WeaponTexture = TextureAssets.Item[player.HeldItem.type].Value;

            float aspectRatio = (float)WeaponTexture.Width / WeaponTexture.Height;
            tanTheta = aspectRatio * (1.0f - scale) * 0.5f;

            return (float)Math.Atan(tanTheta);
        }
        public static float CalculateTiltAngle(Texture2D weaponTex, float scale)
        {
            float tanTheta = 0f;

            float aspectRatio = (float)weaponTex.Width / weaponTex.Height;
            tanTheta = aspectRatio * (1.0f - scale) * 0.5f;

            return (float)Math.Atan(tanTheta);
        }
        /// <summary>
        /// 获取弹幕绘制缩放矩阵
        /// </summary>
        /// <param name="projRotaioin">弹幕当前旋转值</param>
        /// <param name="projScale">弹幕缩放比值</param>
        /// <param name="minScale">最小缩放值</param>
        /// <param name="maxScale">最大缩放值</param>
        /// <param name="ward">方向 默认true正向 即90°为最大 flase 即270°为最大</param>
        /// <returns></returns>
        public static Matrix GetProjDrawScaleMatrix(float projRotation,Vector2 projScale,float minScale = 0.8f,float maxScale = 1.5f,bool ward = true)
        {
            float progress = MathF.Abs(projScale.X - 1 + projScale.Y - 1) * 2;
            if(progress < 0.01f)
                return Matrix.Identity;
            float projDeg = MathHelper.ToDegrees(projRotation);
            float degProgress;
            degProgress = (1 + MathF.Sin(projRotation)) / 2;
            if(!ward)
                degProgress = 1-degProgress;
            float scaleValue = MathHelper.Lerp(minScale,maxScale,degProgress);
            return Matrix.CreateScale(scaleValue, scaleValue, 1f);
        }
    }
}