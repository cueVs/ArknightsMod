using Microsoft.Xna.Framework;
using System;

namespace ArknightsMod.Content.SwingHelper
{
	public static class Vector2AddPart
	{
		public static Vector2 Abs(this Vector2 p) {
			Vector2 a = new Vector2();
			a.X =MathF.Abs(p.X);
			a.Y =MathF.Abs(p.Y);
			return a;
		}
	}
}

