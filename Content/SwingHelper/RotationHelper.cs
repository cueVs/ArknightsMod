
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
namespace ArknightsMod.Content.SwingHelper
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
        public static float GetSwingRotation(float startRad, float endRad, float timer, float totalTime,int playerDir,SwingDir dir = SwingDir.plus) {
	        float setoff = dir == SwingDir.plus ? 0 :MathF.PI;
	        if (totalTime <= 0f)
                return startRad;
            float t = MathHelper.Clamp(timer / totalTime, 0f, 1f);
            float easedT = EaseOutCubic(t);
            float rot = playerDir == 1? startRad + (endRad - startRad) * easedT : endRad + (startRad - endRad) * easedT;
            rot += setoff;
            return rot;
        }
        /// <summary>
        /// 朝向剑方向的缩放旋转
        /// </summary>
        /// <param name="startRad"></param>
        /// <param name="endRad"></param>
        /// <param name="timer"></param>
        /// <param name="totalTime"></param>
        /// <param name="playerDir"></param>
        /// <param name="scale"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static float GetSwingRotation(float startRad, float endRad, float timer, float totalTime, int playerDir,
            Vector2 scale,Vector2 Length,Vector2 handleLen,Vector2 swordLen,out float length,out float handlelen,out float swordlen
            ,out float swordDir,SwingDir dir = SwingDir.plus)
        {
	        float setoff = dir == SwingDir.plus ? 0 :MathF.PI;
	        swordDir = dir == SwingDir.plus ? 1 : -1;
            if (totalTime <= 0f)
            {
                length = Length.Length();
                handlelen = handleLen.Length();
                swordlen = swordLen.Length();
                return startRad;
            }
            Vector2 pos1 = Length;
            Vector2 pos2 = handleLen;
            Vector2 pos3 = swordLen;
            float t = MathHelper.Clamp(timer / totalTime, 0f, 1f);
            float easedT = EaseOutCubic(t);

            float rot = playerDir == 1? startRad + (endRad - startRad) * easedT * (int)dir : endRad + (startRad - endRad) * easedT* (int)dir;
            rot += setoff;
            pos1 = pos1.RotatedBy(rot) * scale;
            pos2 = pos2.RotatedBy(rot) * scale;
            pos3 = pos3.RotatedBy(rot) * scale;
            length = pos1.Length();
            handlelen = pos2.Length();
            swordlen = pos3.Length();
            return pos1.ToRotation();
        }
        /// <summary>
        /// 根据传入的角度来做缩放
        /// </summary>
        /// <param name="startRad"></param>
        /// <param name="endRad"></param>
        /// <param name="timer"></param>
        /// <param name="totalTime"></param>
        /// <param name="playerDir"></param>
        /// <param name="scale"></param>
        /// <param name="stretchAngle"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static float GetSwingRotation(float startRad, float endRad, float timer, float totalTime,
            int playerDir, Vector2 scale, float stretchAngle, SwingDir dir = SwingDir.plus)
        {
            float t = MathHelper.Clamp(timer / totalTime, 0f, 1f);
            float rot = startRad + (endRad - startRad) * EaseOutCubic(t) * (int)dir;

            // 伸缩轴：沿 stretchAngle 方向
            Vector2 axis = new Vector2(1, 0).RotatedBy(stretchAngle);
            Vector2 perp = new Vector2(-axis.Y, axis.X);

            // 把当前剑方向 (cos, sin) 拆到伸缩轴的分量上
            Vector2 sword = new Vector2(1, 0).RotatedBy(rot);
            float ax = sword.X * axis.X + sword.Y * axis.Y;
            float ay = sword.X * perp.X + sword.Y * perp.Y;

            Vector2 result = axis * ax * scale.X + perp * ay * scale.Y;
            return result.ToRotation();
        }
        /// <summary>
        /// 更自然的挥舞缓动 前快后慢
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