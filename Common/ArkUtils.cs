using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ArknightsMod.Common
{
	/// <summary>
	/// 用来写拓展方法的类
	/// </summary>
	public static class ArkUtils
	{
		/// <summary>
		/// 获得一个贴图的长宽的拓展方法
		/// </summary>
		/// <param name="texture">贴图</param>
		/// <returns>返回一个 Vector2 ，其 X 为长，Y 为宽（如果贴图为 null 则返回 Vector2.Zero）</returns>
		public static Vector2 GetTextureSize(this Texture2D texture) {
			return texture == null ? Vector2.Zero : new Vector2(texture.Width, texture.Height);
		}
	}
}
