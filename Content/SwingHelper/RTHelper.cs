using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.SwingHelper
{
	/// <summary>
	/// 全屏 RenderTarget 工具。它只负责 RT 的创建、切换和恢复，
	/// 具体使用什么着色器、传什么参数、如何合成，全部交给外部委托。
	/// 该类由 tModLoader 创建并管理，使用 ModContent.GetInstance&lt;RTHelper&gt;() 获取实例。
	/// </summary>
	public class RTHelper : ModSystem
	{
		private RenderTarget2D normalTarget;
		private RenderTarget2D screenBackup;
		private RenderTarget2D pingTargetA;
		private RenderTarget2D pingTargetB;
		private bool resizePending;
		private bool isDrawing;

		/// <summary>
		/// 注册分辨率变化事件。分辨率改变后的下一次绘制会重新创建 RT。
		/// </summary>
		public override void Load() {
			if (!Main.dedServ)
				Main.OnResolutionChanged += OnResolutionChanged;
		}

		/// <summary>
		/// 卸载时释放普通 RT 与 Ping-Pong 使用的两个 RT。
		/// </summary>
		public override void Unload() {
			if (!Main.dedServ)
				Main.OnResolutionChanged -= OnResolutionChanged;

			DisposeTarget(ref normalTarget);
			DisposeTarget(ref screenBackup);
			DisposeTarget(ref pingTargetA);
			DisposeTarget(ref pingTargetB);
			resizePending = false;
		}

		/// <summary>
		/// 标记 RT 需要重建。实际重建延迟到下一次绘制时进行，避免在分辨率事件中直接操作显存。
		/// </summary>
		private void OnResolutionChanged(Vector2 _) => resizePending = true;

		/// <summary>
		/// 普通单次 RT。
		/// sourceDraw 在透明 RT 中绘制原始内容；compositeDraw 接收这个 RT，负责将它绘制回屏幕。
		/// compositeDraw 必须自行开始和结束 SpriteBatch，因此可以自由设置着色器、参数、混合模式和最终透明度。
		/// </summary>
		public void Draw(SpriteBatch spriteBatch, Action sourceDraw, Action<Texture2D> compositeDraw) {
			if (isDrawing || !CanDraw(spriteBatch, sourceDraw, out GraphicsDevice graphicsDevice) || compositeDraw == null)
				return;
			if (!EnsureTarget(graphicsDevice, ref normalTarget) || !EnsureTarget(graphicsDevice, ref screenBackup))
				return;

			RenderState state = new RenderState(graphicsDevice);
			Texture2D screenSource;
			if (!TryFindScreenSource(state, out screenSource))
				return;
			bool screenCaptured = false;
			bool outputRestored = false;
			isDrawing = true;
			try {
				// 切换 RT 会导致原目标内容被丢弃，先把当前画面复制到备份 RT。
				CaptureScreen(spriteBatch, graphicsDevice, screenSource);
				screenCaptured = true;
				BeginTarget(spriteBatch, graphicsDevice, normalTarget);
				sourceDraw();
				spriteBatch.End();

				Restore(graphicsDevice, state);
				outputRestored = true;
				// 原目标可能已经被驱动清空，先完整恢复原画面，再叠加残影。
				RestoreScreen(spriteBatch);
				compositeDraw(normalTarget);
			}
			catch {
				TryEnd(spriteBatch);
				if (screenCaptured) {
					Restore(graphicsDevice, state);
					outputRestored = true;
					try { RestoreScreen(spriteBatch); } catch { }
				}
			}
			finally {
				// 成功路径已经恢复并写回输出 RT，重复绑定会再次丢弃刚写入的画面。
				if (!outputRestored)
					Restore(graphicsDevice, state);
				isDrawing = false;
				BeginDefault(spriteBatch);
			}
		}

		/// <summary>
		/// 双缓冲 Ping-Pong RT。
		/// sourceDraw 绘制初始内容。process 每执行一次，current 是上一次结果，next 是当前要写入的 RT。
		/// process 返回 true 时继续下一次交换，返回 false 时停止处理。最后的 RT 会传给 compositeDraw。
		/// process 与 compositeDraw 内部必须自行开始和结束 SpriteBatch；process 需要将 current 绘制到已绑定的 next。
		/// </summary>
		public void DrawPingPong(SpriteBatch spriteBatch, Action sourceDraw,
			Func<Texture2D, RenderTarget2D, int, bool> process, Action<Texture2D> compositeDraw) {
			if (isDrawing || !CanDraw(spriteBatch, sourceDraw, out GraphicsDevice graphicsDevice) || process == null || compositeDraw == null)
				return;
			if (!EnsureTarget(graphicsDevice, ref pingTargetA) || !EnsureTarget(graphicsDevice, ref pingTargetB))
				return;

			RenderState state = new RenderState(graphicsDevice);
			Texture2D screenSource;
			if (!TryFindScreenSource(state, out screenSource))
				return;
			if (!EnsureTarget(graphicsDevice, ref screenBackup))
				return;
			RenderTarget2D current = pingTargetA;
			RenderTarget2D next = pingTargetB;
			bool screenCaptured = false;
			bool outputRestored = false;
			isDrawing = true;
			try {
				// Ping-Pong 处理同样必须保护进入工具前的屏幕内容。
				CaptureScreen(spriteBatch, graphicsDevice, screenSource);
				screenCaptured = true;
				BeginTarget(spriteBatch, graphicsDevice, current);
				sourceDraw();
				spriteBatch.End();

				for (int passIndex = 0; passIndex < 64; passIndex++) {
					graphicsDevice.SetRenderTarget(next);
					graphicsDevice.Clear(Color.Transparent);
					if (!process(current, next, passIndex))
						break;

					RenderTarget2D swap = current;
					current = next;
					next = swap;
				}

				Restore(graphicsDevice, state);
				outputRestored = true;
				RestoreScreen(spriteBatch);
				compositeDraw(current);
			}
			catch {
				TryEnd(spriteBatch);
				if (screenCaptured) {
					Restore(graphicsDevice, state);
					outputRestored = true;
					try { RestoreScreen(spriteBatch); } catch { }
				}
			}
			finally {
				// 成功路径已经恢复并写回输出 RT，重复绑定会再次丢弃刚写入的画面。
				if (!outputRestored)
					Restore(graphicsDevice, state);
				isDrawing = false;
				BeginDefault(spriteBatch);
			}
		}

		/// <summary>
		/// 检查当前环境是否可以操作 RT。专用服务器没有图形设备，不能执行任何绘制。
		/// </summary>
		private bool CanDraw(SpriteBatch spriteBatch, Action sourceDraw, out GraphicsDevice graphicsDevice) {
			graphicsDevice = Main.instance?.GraphicsDevice;
			return !Main.dedServ && spriteBatch != null && sourceDraw != null && graphicsDevice != null;
		}

		/// <summary>
		/// 结束当前 SpriteBatch，绑定目标 RT，清成透明画布后以世界坐标矩阵重新开始绘制。
		/// </summary>
		private void BeginTarget(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D target) {
			// 调用方的 SpriteBatch 可能已经在抓屏步骤中结束，重复 End 会直接抛异常。
			graphicsDevice.SetRenderTarget(target);
			graphicsDevice.Clear(Color.Transparent);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		}

		/// <summary>
		/// 找到进入工具前真正包含当前画面的纹理。后备缓冲没有可采样纹理时使用 Terraria 的屏幕 RT。
		/// </summary>
		private bool TryFindScreenSource(RenderState state, out Texture2D source) {
			source = Main.screenTarget as Texture2D;
			if (state.Targets != null && state.Targets.Length > 0 && state.Targets[0].RenderTarget != null) {
				source = state.Targets[0].RenderTarget as Texture2D;
				if (source == null)
					return false;
				return true;
			}
			Texture2D screenTarget = Main.screenTarget as Texture2D;
			if (screenTarget == null || screenTarget.IsDisposed)
				return false;
			source = screenTarget;
			return true;
		}

		/// <summary>
		/// 将当前屏幕复制到独立 RT，避免绑定新 RT 时丢失原画面。
		/// </summary>
		private void CaptureScreen(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Texture2D source) {
			TryEnd(spriteBatch);
			graphicsDevice.SetRenderTarget(screenBackup);
			graphicsDevice.Clear(Color.Transparent);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
			spriteBatch.Draw(source, Vector2.Zero, Color.White);
			spriteBatch.End();
		}

		/// <summary>
		/// 把屏幕备份回填到当前恢复后的目标。
		/// </summary>
		private void RestoreScreen(SpriteBatch spriteBatch) {
			if (screenBackup == null || screenBackup.IsDisposed)
				return;
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp,
				DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
			spriteBatch.Draw(screenBackup, Vector2.Zero, Color.White);
			spriteBatch.End();
		}

		/// <summary>
		/// 根据当前屏幕大小创建或复用 RT。RT 的尺寸始终与游戏屏幕完全一致。
		/// </summary>
		private bool EnsureTarget(GraphicsDevice graphicsDevice, ref RenderTarget2D target) {
			int width = graphicsDevice.PresentationParameters.BackBufferWidth;
			int height = graphicsDevice.PresentationParameters.BackBufferHeight;
			if (width <= 0 || height <= 0)
				return false;

			if (!resizePending && target != null && !target.IsDisposed && target.Width == width && target.Height == height)
				return true;

			DisposeTarget(ref target);
			try {
				// RT 需要 Alpha 通道；后备缓冲格式可能没有 Alpha，透明清屏会变成不透明黑色。
				target = new RenderTarget2D(graphicsDevice, width, height, false,
					SurfaceFormat.Color, DepthFormat.None);
				resizePending = false;
				return true;
			}
			catch {
				target = null;
				return false;
			}
		}

		/// <summary>
		/// 恢复进入 RT 工具之前的渲染目标、视口与 GPU 状态，避免影响后续绘制。
		/// </summary>
		private void Restore(GraphicsDevice graphicsDevice, RenderState state) {
			graphicsDevice.SetRenderTargets(state.Targets);
			graphicsDevice.Viewport = state.Viewport;
			graphicsDevice.ScissorRectangle = state.ScissorRectangle;
			graphicsDevice.RasterizerState = state.RasterizerState;
			graphicsDevice.BlendState = state.BlendState;
			graphicsDevice.DepthStencilState = state.DepthStencilState;
			graphicsDevice.SamplerStates[0] = state.SamplerState;
		}

		/// <summary>
		/// 释放 RT 占用的显存。
		/// </summary>
		private void DisposeTarget(ref RenderTarget2D target) {
			try {
				target?.Dispose();
			}
			catch {
			}
			target = null;
		}

		/// <summary>
		/// 尝试结束 SpriteBatch。异常时忽略，确保管线恢复流程仍能继续。
		/// </summary>
		private void TryEnd(SpriteBatch spriteBatch) {
			try {
				spriteBatch.End();
			}
			catch {
			}
		}

		/// <summary>
		/// 工具结束后重启 Terraria 默认 SpriteBatch，保证后续普通绘制能继续进行。
		/// </summary>
		private void BeginDefault(SpriteBatch spriteBatch) {
			try {
				spriteBatch.Begin();
			}
			catch {
			}
		}

		/// <summary>
		/// 记录进入 RT 工具前的 GPU 状态。RenderTarget 与 SpriteBatch 状态无法自动恢复，
		/// 因此必须在切换 RT 前手动保存这些值。
		/// </summary>
		private readonly struct RenderState
		{
			public readonly RenderTargetBinding[] Targets;
			public readonly Viewport Viewport;
			public readonly Rectangle ScissorRectangle;
			public readonly RasterizerState RasterizerState;
			public readonly BlendState BlendState;
			public readonly DepthStencilState DepthStencilState;
			public readonly SamplerState SamplerState;

			public RenderState(GraphicsDevice graphicsDevice) {
				Targets = graphicsDevice.GetRenderTargets();
				Viewport = graphicsDevice.Viewport;
				ScissorRectangle = graphicsDevice.ScissorRectangle;
				RasterizerState = graphicsDevice.RasterizerState;
				BlendState = graphicsDevice.BlendState;
				DepthStencilState = graphicsDevice.DepthStencilState;
				SamplerState = graphicsDevice.SamplerStates[0];
			}
		}
	}
}
