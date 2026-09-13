namespace ArknightsMod.Content.NPCs.Enemy.Chapter6.FrostNova
{
	/// <summary>
	/// 在 <see cref="Systems.IceAltarWarpPostProcess"/> 捕获的扭曲 RT 上绘制扭曲遮罩
	/// </summary>
	public interface IIceAltarWarpDraw
	{
		/// <summary>
		/// 为 false 时（例如喷发阶段）不在扭曲 RT 上绘制遮罩；<see cref="Systems.IceAltarWarpPostProcess"/> 也不得为此弹幕提前 End <c>Main.spriteBatch</c>。
		/// </summary>
		bool IceAltarWarpMaskActive { get; }

		void DrawIceAltarWarp();
	}
}
