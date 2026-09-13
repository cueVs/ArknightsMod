using ArknightsMod.Content.Items.Weapons.Defender.Beagle;
using ArknightsMod.Content.Items.Weapons.Defender.Durnar;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
namespace RuneSKill.Content.NeedTool
{
    public static class RotationHelper
    {   
        public enum SwingDir{plus =1,minus =-1}
        /// <summary>
        /// 计算当前挥舞角度
        /// </summary>
        /// <param name="startRad">起始角度</param>
        /// <param name="endRad">结束角度</param>
        /// <param name="timer">当前时间</param>
        /// <param name="totalTime">总时间</param>
        /// <param name="dir">挥舞方向</param>
        /// <returns>当前角度</returns>
        public static float GetSwingRotation(float startRad, float endRad, float timer, float totalTime,int playerDir,SwingDir dir = SwingDir.plus)
        {
            if (totalTime <= 0f)
                return startRad;
            float t = MathHelper.Clamp(timer / totalTime, 0f, 1f);
            float easedT = EaseOutCubic(t);
            float rot =   startRad * playerDir - (endRad - startRad) *easedT  * (int)dir;
            return rot;
        }
        /// <summary>
        /// 更自然的挥舞缓动，前快后慢
        /// </summary>
        public static float EaseOutCubic(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            return 1f - MathF.Pow(1f - t, 3f);
        }
        /// <summary>
        /// 前后柔和
        /// </summary>
        public static float EaseInOutSine(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            return -(MathF.Cos(MathF.PI * t) - 1f) / 2f;
        }
    }
}