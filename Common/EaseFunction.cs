using Microsoft.Xna.Framework;
using System;

namespace ArknightsMod.Common
{
    public static class EaseFunction
    {
		public static float Ease(int value, int inMin, int inMax, float outMin, float outMax) {
			return (float)(value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
		}
		public static float EaseOutQuint(float x, float xMin, float xMax, float yMin, float yMax)
        {
            float normalizedX = (x - xMin) / (xMax - xMin);
            float easedValue = 1 - (float)Math.Pow(1 - normalizedX, 5);
            return easedValue * (yMax - yMin) + yMin;
        }
        public static float EaseOutQuintClamped(float x, float xMin, float xMax, float yMin, float yMax)
        {
            float normalizedX = (x - xMin) / (xMax - xMin);
            float easedValue = 1 - (float)Math.Pow(1 - normalizedX, 5);
            return Math.Clamp(easedValue * (yMax - yMin) + yMin, yMin, yMax);
        }
        public static float EaseInQuint(float x, float xMin, float xMax, float yMin, float yMax)
        {
            float normalizedX = (x - xMin) / (xMax - xMin);
            float easedValue = (float)Math.Pow(normalizedX, 5);
            return easedValue * (yMax - yMin) + yMin;
        }
        public static float EaseInQuint(float x, float xMin, float xMax, float yMin, float yMax, float intensity=5)
        {
            float normalizedX = (x - xMin) / (xMax - xMin);
            float easedValue = (float)Math.Pow(normalizedX, intensity);
            return easedValue * (yMax - yMin) + yMin;
        }
        public static float EaseInQuintClamped(float x, float xMin, float xMax, float yMin, float yMax)
        {
            float normalizedX = (x - xMin) / (xMax - xMin);
            float easedValue = (float)Math.Pow(normalizedX, 5);
            return Math.Clamp(easedValue * (yMax - yMin) + yMin, yMin, yMax);
        }

        public static float EaseOutElastic(float x, float xMin, float xMax, float yMin, float yMax)
        {
            float c4 = (2 * (float)Math.PI) / 3;
            float t = (x - xMin) / (xMax - xMin);
            float elastic = t == 0 ? 0 : t == 1 ? 1 : (float)(Math.Pow(2, -10 * t) * Math.Sin((t * 10 - 0.75) * c4) + 1);

            return yMin + elastic * (yMax - yMin);
        }
        public static float EaseInBack(float x, float xMin, float xMax, float outputMin, float outputMax)
        {
            x = Math.Clamp(x, xMin, xMax);
            float t = (x - xMin) / (xMax - xMin);
            float s = 1.70158f;
            float easeInBackValue = t * t * ((s + 1) * t - s);
            float outputRange = outputMax - outputMin;
            float mappedOutput = outputMin + easeInBackValue * outputRange;
            return mappedOutput;
        }
        public static float EaseInOutBack(float x, float xMin, float xMax, float outputMin, float outputMax)
        {
            x = Math.Clamp(x, xMin, xMax);
            float t = (x - xMin) / (xMax - xMin);
            float s = 1.70158f * 1.525f;
            float easeInOutBackValue;

            if (t < 0.5f)
            {
                easeInOutBackValue = 0.5f * (t * 2) * (t * 2) * ((s + 1) * (t * 2) - s);
            }
            else
            {
                float f = (t * 2) - 2;
                easeInOutBackValue = 0.5f * (f * f * ((s + 1) * f + s) + 2);
            }

            float outputRange = outputMax - outputMin;
            float mappedOutput = outputMin + easeInOutBackValue * outputRange;
            return mappedOutput;
        }
		/// <summary>
		/// 弹性缓动
		/// </summary>
		/// <param name="x">自变量</param>
		/// <param name="xMin">定义域最小值</param>
		/// <param name="xMax">定义域最大值</param>
		/// <param name="yMin">值域最小值</param>
		/// <param name="yMax">值域最大值</param>
		/// <param name="firstOvershoot">第一次波谷相对于最大值域的比例。值为正数时，第一次震荡会超过目标值（向上震荡）。值为负数时，第一次震荡会低于起始值（向下震荡）</param>
		/// <returns></returns>
		public static float EaseOutElastic(float x, float xMin, float xMax, float yMin, float yMax, float firstOvershoot = 0f)
		{
			float c4 = (2 * (float)Math.PI) / 3;
			float t = (x - xMin) / (xMax - xMin);

			// 计算弹性部分
			float elastic;
			if (t == 0) {
				elastic = 0;
			}
			else if (t == 1) {
				elastic = 1;
			}
			else {
				// 调整参数以控制第一次震荡的最低点
				float amplitudeFactor = 1.0f + firstOvershoot;
				float decayFactor = -10.0f / amplitudeFactor;

				elastic = (float)(amplitudeFactor * Math.Pow(2, decayFactor * t) *
								  Math.Sin((t * 10 - 0.75) * c4) + 1);
			}

			return yMin + elastic * (yMax - yMin);
		}
		/// <summary>
		/// 二次缓动
		/// </summary>
		/// <param name="x">当前 x 值</param>
		/// <param name="xMin">x 的最小值</param>
		/// <param name="xMax">x 的最大值</param>
		/// <param name="yMin">y 的最小值</param>
		/// <param name="yMax">y 的最大值</param>
		/// <param name="peakPercentage">峰值或谷值在 x 区间的位置百分比 (0-1)</param>
		/// <param name="isConcave">是否是凹形状 (false 为凹形，true 为凸形)</param>
		/// <returns>对应的 y 值</returns>
		public static float QuadraticEase(float x, float xMin, float xMax, float yMin, float yMax, bool isConcave, float peakPercentage = 0.5f)
        {
            peakPercentage = Math.Clamp(peakPercentage, 0f, 1f);

            //计算峰值对应的x值
            float xPeak = xMin + (xMax - xMin) * peakPercentage;

            x = Math.Clamp(x, xMin, xMax);

            //y的变化幅度
            float yRange = yMax - yMin;
            float t;

            //根据x的位置计算缓动效果
            if (x <= xPeak)
            {
                t = (x - xPeak) / (xMin - xPeak); //归一化到[0, 1]
            }
            else
            {
                t = (x - xPeak) / (xMax - xPeak); //归一化到[0, 1]
            }

            //根据isConcave决定公式
            if (isConcave)
            {
                //凹形状 (最大值位于峰值)
                return yMin + yRange * (1 - t * t);
            }
            else
            {
                //凸形状 (最小值位于峰值)
                return yMax - yRange * (1 - t * t);
            }
        }

        /// <summary>
        /// 二次贝塞尔曲线
        /// </summary>
        /// <param name="P0"></param>
        /// <param name="P1"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static Vector2 LinearBezier(Vector2 P0, Vector2 P1, float progress)
        {
            progress = Math.Clamp(progress, 0f, 1f);

            float x = (1 - progress) * P0.X + progress * P1.X;
            float y = (1 - progress) * P0.Y + progress * P1.Y;

            return new Vector2(x, y);
        }

        /// <summary>
        /// 三次贝塞尔曲线
        /// </summary>
        /// <param name="Start"></param>
        /// <param name="ControlPoint1"></param>
        /// <param name="End"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static Vector2 QuadraticBezier(Vector2 Start, Vector2 ControlPoint1, Vector2 End, float progress)
        {
            progress = Math.Clamp(progress, 0, 1);

            float x = (1 - progress) * (1 - progress) * Start.X + 2 * (1 - progress) * progress * ControlPoint1.X + progress * progress * End.X;
            float y = (1 - progress) * (1 - progress) * Start.Y + 2 * (1 - progress) * progress * ControlPoint1.Y + progress * progress * End.Y;

            return new Vector2(x, y);
        }
        /// <summary>
        /// 四次贝塞尔曲线
        /// </summary>
        /// <param name="Start"></param>
        /// <param name="ControlPoint1"></param>
        /// <param name="ControlPoint2"></param>
        /// <param name="End"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public static Vector2 CubicBezier(Vector2 Start, Vector2 ControlPoint1, Vector2 ControlPoint2, Vector2 End, float progress)
        {
            //确保progress在[0, 1]范围内
            progress = Math.Clamp(progress, 0, 1);

            //三次贝塞尔曲线公式
            float x = (float)(
                Math.Pow(1 - progress, 3) * Start.X +
                3 * Math.Pow(1 - progress, 2) * progress * ControlPoint1.X +
                3 * (1 - progress) * Math.Pow(progress, 2) * ControlPoint2.X +
                Math.Pow(progress, 3) * End.X
            );

            float y = (float)(
                Math.Pow(1 - progress, 3) * Start.Y +
                3 * Math.Pow(1 - progress, 2) * progress * ControlPoint1.Y +
                3 * (1 - progress) * Math.Pow(progress, 2) * ControlPoint2.Y +
                Math.Pow(progress, 3) * End.Y
            );

            return new Vector2(x, y);
        }

        /// <summary>
        /// CatmullRom曲线，返回Vector2
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="value3"></param>
        /// <param name="value4"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static Vector2 CatmullRom(Vector2 value1, Vector2 value2, Vector2 value3, Vector2 value4, float amount)
        {
            return new Vector2(
                MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount),
                MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount)
            );
        }

        /// <summary>
        /// 计算三角函数缓动值
        /// </summary>
        /// <param name="x">输入值</param>
        /// <param name="yMin">y的最小值</param>
        /// <param name="yMax">y的最大值</param>
        /// <param name="frequency">频率（默认为1）</param>
        /// <returns>缓动后的y值</returns>
        public static double SineEase(double x, double yMin, double yMax, double frequency = 1)
        {
            yMin = Math.Min(yMin, yMax);
            yMax = Math.Max(yMin, yMax);
            frequency *= 0.01;
            double amplitude = (yMax - yMin) / 2;
            double offset = yMin + amplitude;
            return amplitude * Math.Sin(2 * Math.PI * frequency * x) + offset;
        }
        public static float SineEase(float x, float yMin, float yMax, float frequency = 1)
        {
            yMin = Math.Min(yMin, yMax);
            yMax = Math.Max(yMin, yMax);
            frequency *= 0.01f;
            float amplitude = (yMax - yMin) / 2;
            float offset = yMin + amplitude;
            return amplitude * (float)Math.Sin(2 * Math.PI * frequency * x) + offset;
        }
        public static Vector2 GetQuadraticPoint(float x, float maxValue, Vector2 startPoint, Vector2 endPoint, float StandardY)
        {
            //确定顶点 (h, k)
            float h = (endPoint.X + startPoint.X) * 0.5f; //顶点的 X 坐标
            float k = StandardY + maxValue;        //顶点的 Y 坐标

            //计算系数a
            float x1 = startPoint.X; //起点的X坐标
            float y1 = StandardY; //起点的Y坐标
            float a = (y1 - k) / ((x1 - h) * (x1 - h));

            //根据输入的x计算对应的Y值
            float interpolatedX = MathHelper.Lerp(startPoint.X, endPoint.X, x); //线性插值X坐标
            float interpolatedY = a * (interpolatedX - h) * (interpolatedX - h) + k; //抛物线计算Y坐标

            //返回计算出的点
            return new Vector2(interpolatedX, interpolatedY);
        }
    }
}
