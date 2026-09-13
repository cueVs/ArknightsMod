using System;
using ArknightsMod.Content.NPCs.Enemy.Chapter6.FrostNova;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace ArknightsMod.Systems
{
	/// <summary>
	/// 冰晶祭坛冲击波环带的屏幕扭曲后处理。
	/// </summary>
	public sealed class IceAltarWarpPostProcess : ModSystem
	{
		private static SpriteBatch _warpSpriteBatch;
		private static RenderTarget2D _warpTarget;
		private static RenderTarget2D _screenCopy;
		internal static Effect IceAltarKScreen0Effect;

		internal static bool EnableIceAltarWarpThisFrame;
		internal static bool IceAltarWarpDrawnThisFrame;
		internal static float IceAltarWarpIntensity = 0.06f;

		public override void Load() {
			if (Main.dedServ)
				return;
			// 在 TransferAllAssets 完成前 ImmediateLoad 会触发资源解析；若 .fx 未成功编译为 .xnb，HasAsset 为 false，必须避免 Request 以免 MissingResourceException 禁用整个模组。
			const string iceAltarEffectPath = "ArknightsMod/Assets/Effects/IceAltarKScreen0";
			try {
				if (!ModContent.HasAsset(iceAltarEffectPath)) {
					Mod.Logger.Warn($"未找到着色器资源 {iceAltarEffectPath}（请确认 Assets/Effects/IceAltarKScreen0.fx 已参与构建并生成 .xnb）。冰晶祭坛屏幕扭曲已禁用。");
					IceAltarKScreen0Effect = null;
					return;
				}
				if (!ModContent.RequestIfExists<Effect>(iceAltarEffectPath, out Asset<Effect> iceFx, AssetRequestMode.ImmediateLoad)) {
					IceAltarKScreen0Effect = null;
					return;
				}
				IceAltarKScreen0Effect = iceFx.Value;
			}
			catch (Exception ex) {
				Mod.Logger.Error($"加载 IceAltarKScreen0 失败。冰晶祭坛扭曲将不可用。{ex.Message}");
				IceAltarKScreen0Effect = null;
			}
			if (IceAltarKScreen0Effect != null) {
				Main.OnResolutionChanged += OnResolutionChanged;
				On_FilterManager.EndCapture += FilterManager_EndCapture;
				Main.RunOnMainThread(() => {
					CreateWarpTarget();
					try {
						_warpSpriteBatch?.Dispose();
					}
					catch { }
					try {
						if (Main.instance?.GraphicsDevice != null)
							_warpSpriteBatch = new SpriteBatch(Main.instance.GraphicsDevice);
					}
					catch { }
				});
			}
		}

		public override void Unload() {
			IceAltarKScreen0Effect = null;
			try { _warpSpriteBatch?.Dispose(); } catch { }
			_warpSpriteBatch = null;
			RenderTarget2D warp = _warpTarget;
			RenderTarget2D copy = _screenCopy;
			_warpTarget = null;
			_screenCopy = null;
			if (!Main.dedServ && (warp != null || copy != null)) {
				try {
					Main.RunOnMainThread(() => {
						warp?.Dispose();
						copy?.Dispose();
					});
				}
				catch { }
			}
			try { Main.OnResolutionChanged -= OnResolutionChanged; } catch { }
			try { On_FilterManager.EndCapture -= FilterManager_EndCapture; } catch { }
		}

		private static void OnResolutionChanged(Vector2 _) {
			if (Main.dedServ)
				return;
			Main.RunOnMainThread(CreateWarpTarget);
		}

		private static void CreateWarpTarget() {
			if (Main.dedServ || Main.instance == null)
				return;
			GraphicsDevice gd = Main.instance.GraphicsDevice;
			if (gd == null)
				return;
			int w = gd.PresentationParameters.BackBufferWidth;
			int h = gd.PresentationParameters.BackBufferHeight;
			SurfaceFormat format = gd.PresentationParameters.BackBufferFormat;
			if (_warpTarget != null)
				_warpTarget.Dispose();
			_warpTarget = new RenderTarget2D(gd, w, h, false, format, DepthFormat.None);
			if (_screenCopy != null)
				_screenCopy.Dispose();
			_screenCopy = new RenderTarget2D(gd, w, h, false, format, DepthFormat.None);
		}

		private static bool HasIceAltarWarpDraw() {
			foreach (Projectile proj in Main.projectile) {
				if (proj != null && proj.active && proj.ModProjectile is IIceAltarWarpDraw w && w.IceAltarWarpMaskActive)
					return true;
			}
			return false;
		}

		private static void FilterManager_EndCapture(On_FilterManager.orig_EndCapture orig, FilterManager self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor) {
			orig(self, finalTexture, screenTarget1, screenTarget2, clearColor);
			if (Main.dedServ)
				return;
			if (finalTexture == null) {
				EnableIceAltarWarpThisFrame = false;
				return;
			}
			bool enableThisFrame = EnableIceAltarWarpThisFrame;
			bool hasWarp = HasIceAltarWarpDraw();
			if (!enableThisFrame && !hasWarp)
				return;

			GraphicsDevice gd = null;
			SpriteBatch sb = null;
			RenderTarget2D screenCopy = null;
			RenderTarget2D warpTarget = null;
			RenderTargetBinding[] previousRenderTargets = null;
			Viewport previousViewport = default;
			Rectangle previousScissorRectangle = default;
			RasterizerState previousRasterizerState = null;
			BlendState previousBlendState = null;
			DepthStencilState previousDepthStencilState = null;
			SamplerState previousSampler0 = null;

			void SafeEndMainSpriteBatch() {
				try { Main.spriteBatch?.End(); } catch { }
			}

			void RestoreScreenFromCopy() {
				if (gd == null || screenCopy == null || screenCopy.IsDisposed || finalTexture == null || sb == null)
					return;
				try {
					gd.SetRenderTarget(finalTexture);
					sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
					sb.Draw(screenCopy, Vector2.Zero, Color.White);
					sb.End();
				}
				catch {
					try { sb.End(); } catch { }
					try { gd?.SetRenderTarget(finalTexture); } catch { }
				}
			}

			try {
				if (Main.instance == null)
					return;
				screenCopy = _screenCopy;
				warpTarget = _warpTarget;
				if (IceAltarKScreen0Effect == null || warpTarget == null || screenCopy == null)
					return;
				if (screenCopy.IsDisposed || warpTarget.IsDisposed)
					return;
				gd = Main.instance.GraphicsDevice;
				if (gd == null)
					return;
				sb = _warpSpriteBatch;
				if (sb == null)
					return;

				previousRenderTargets = gd.GetRenderTargets();
				previousViewport = gd.Viewport;
				previousScissorRectangle = gd.ScissorRectangle;
				previousRasterizerState = gd.RasterizerState;
				previousBlendState = gd.BlendState;
				previousDepthStencilState = gd.DepthStencilState;
				previousSampler0 = gd.SamplerStates[0];

				IceAltarWarpDrawnThisFrame = false;

				gd.SetRenderTarget(screenCopy);
				gd.Clear(Color.Black);
				sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
				sb.Draw(finalTexture, Vector2.Zero, Color.White);
				sb.End();
				gd.SetRenderTarget(finalTexture);

				gd.SetRenderTarget(warpTarget);
				gd.Clear(Color.Transparent);
				foreach (Projectile proj in Main.projectile) {
					if (proj == null || !proj.active || proj.ModProjectile is not IIceAltarWarpDraw w || !w.IceAltarWarpMaskActive)
						continue;
					try {
						SafeEndMainSpriteBatch();
						w.DrawIceAltarWarp();
						SafeEndMainSpriteBatch();
						IceAltarWarpDrawnThisFrame = true;
					}
					catch {
						SafeEndMainSpriteBatch();
					}
				}

				gd.SetRenderTarget(finalTexture);

				if (!IceAltarWarpDrawnThisFrame) {
					RestoreScreenFromCopy();
					return;
				}

				sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
				IceAltarKScreen0Effect.Parameters["tex0"].SetValue(warpTarget);
				IceAltarKScreen0Effect.Parameters["i"].SetValue(IceAltarWarpIntensity);
				EffectPass warpPass = IceAltarKScreen0Effect.CurrentTechnique.Passes[0];
				foreach (EffectPass p in IceAltarKScreen0Effect.CurrentTechnique.Passes) {
					if (p.Name == "IceAltarKScreen0") {
						warpPass = p;
						break;
					}
				}
				warpPass.Apply();
				sb.Draw(screenCopy, Vector2.Zero, Color.White);
				sb.End();
			}
			catch {
				SafeEndMainSpriteBatch();
				try { sb?.End(); } catch { }
				try { gd?.SetRenderTarget(finalTexture); } catch { }
				RestoreScreenFromCopy();
			}
			finally {
				EnableIceAltarWarpThisFrame = false;
				IceAltarWarpIntensity = 0.06f;
				try {
					if (gd != null) {
						if (previousRenderTargets != null)
							gd.SetRenderTargets(previousRenderTargets);
						gd.Viewport = previousViewport;
						gd.ScissorRectangle = previousScissorRectangle;
						if (previousRasterizerState != null)
							gd.RasterizerState = previousRasterizerState;
						if (previousBlendState != null)
							gd.BlendState = previousBlendState;
						if (previousDepthStencilState != null)
							gd.DepthStencilState = previousDepthStencilState;
						if (previousSampler0 != null)
							gd.SamplerStates[0] = previousSampler0;
					}
				}
				catch { }
				SafeEndMainSpriteBatch();
			}
		}
	}
}
