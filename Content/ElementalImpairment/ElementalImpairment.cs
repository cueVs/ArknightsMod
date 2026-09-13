using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.ElementalImpairment.Effect
{
	public enum AfflictionState { Accumulating, Burst, Cooldown, Idle }
	public enum UpdateResult { None, Burst }

	public abstract class ElementalAffliction
	{
		public abstract int MaxValue { get; }
		public abstract int BurstDamage { get; }
		public abstract int CooldownTicks { get; }
		public abstract void OnBurstEffects(NPC npc);
		public virtual string IconMaskTexture => "ArknightsMod/Content/ElementalImpairment/Effect/IconMask";
		public virtual string FeatherMaskTexture => "ArknightsMod/Content/ElementalImpairment/Effect/FeatherMask";
		public virtual Color IconColor => Color.White;
		public virtual Color FeatherColor => new Color(255, 255, 255, 30);
		public virtual float FeatherScale => 0.139f;
		public virtual float MainScale => 0.215f;
		public abstract Color BurstDamageColor { get; }
		public virtual string BurstFlashMainMask => "ArknightsMod/Content/ElementalImpairment/Effect/BurstMainMask";
		public virtual string BurstFlashFeatherMask => "ArknightsMod/Content/ElementalImpairment/Effect/BurstFeatherMask";
		public virtual Color BurstFlashMainColor => Color.White;
		public virtual Color BurstFlashFeatherColor => new Color(255, 255, 255, 150);

		public int CurrentValue;
		public int CooldownTimer;
		private int cachedCooldownTicks;
		public AfflictionState State { get; private set; } = AfflictionState.Idle;
		public bool IsSuppressed { get; set; }

		public virtual void ApplyDefenseReduction(NPC npc, int amount) {
			if (amount <= 0)
				return;
			npc.defense -= amount;
			npc.defDefense = npc.defense;
			if (npc.defense < 0)
				npc.defense = 0;
		}

		public virtual void ApplyBurstDamage(NPC npc) {
			int damage = BurstDamage;
			if (damage < 0)
				return;
			npc.life -= damage;
			CombatText.NewText(npc.Hitbox, BurstDamageColor, damage, true);
			OnBurstEffects(npc);
			if (npc.life <= 0) {
				npc.life = 0;
				npc.checkDead();
				npc.active = false;
			}
		}

		public virtual Vector2 GetFlashPosition(NPC npc) {
			Vector2 pos = npc.Center;
			pos.Y += npc.height * 0.5f + 5f;
			return pos;
		}

		public virtual UpdateResult Update() {
			if (IsSuppressed)
				return UpdateResult.None;

			switch (State) {
				case AfflictionState.Accumulating:
					if (CurrentValue >= MaxValue) {
						CurrentValue = MaxValue;
						State = AfflictionState.Burst;
						return UpdateResult.Burst;
					}
					break;
				case AfflictionState.Burst:
					State = AfflictionState.Cooldown;
					cachedCooldownTicks = CooldownTicks;
					CooldownTimer = cachedCooldownTicks;
					CurrentValue = 0;
					break;
				case AfflictionState.Cooldown:
					if (CooldownTimer > 0) {
						CooldownTimer--;
						CurrentValue = (int)((1f - (float)CooldownTimer / cachedCooldownTicks) * MaxValue);
					}
					else {
						CurrentValue = 0;
						State = AfflictionState.Idle;
					}
					break;
			}
			return UpdateResult.None;
		}

		public void AddValue(int amount) {
			if (State == AfflictionState.Cooldown)
				return;
			CurrentValue += amount;
			if (State == AfflictionState.Idle)
				State = AfflictionState.Accumulating;
		}

		public void ClearAccumulation() {
			CurrentValue = 0;
			State = AfflictionState.Idle;
		}
	}

	public class AfflictionContainer
	{
		public NPC Owner { get; }
		public List<ElementalAffliction> Afflictions = new();

		public AfflictionContainer(NPC owner) => Owner = owner;

		public T GetOrAdd<T>() where T : ElementalAffliction, new() {
			var existing = Afflictions.Find(a => a is T);
			if (existing != null)
				return (T)existing;
			var aff = new T();
			Afflictions.Add(aff);
			return aff;
		}

		private ElementalAffliction GetDominantAffliction() {
			ElementalAffliction dominant = null;
			int maxValue = -1;
			foreach (var aff in Afflictions) {
				if (aff.State == AfflictionState.Accumulating || aff.State == AfflictionState.Idle) {
					if (aff.CurrentValue > maxValue) {
						maxValue = aff.CurrentValue;
						dominant = aff;
					}
				}
			}
			return dominant;
		}

		private void UpdateSuppression() {
			var dominant = GetDominantAffliction();
			foreach (var aff in Afflictions) {
				if (aff.State == AfflictionState.Cooldown || aff.State == AfflictionState.Burst) {
					aff.IsSuppressed = false;
					continue;
				}
				aff.IsSuppressed = (aff != dominant && dominant != null && dominant.CurrentValue > 0);
			}
		}

		public void AddAfflictionValue<T>(int amount) where T : ElementalAffliction, new() {
			// 有任何损伤在冷却中，阻止施加新的异常值
			foreach (var aff in Afflictions) {
				if (aff.State == AfflictionState.Cooldown)
					return;
			}

			var affToAdd = GetOrAdd<T>();
			affToAdd.AddValue(amount);
			UpdateSuppression();
		}

		public void Update() {
			if (Owner == null || !Owner.active)
				return;

			UpdateSuppression();

			foreach (var aff in Afflictions) {
				var result = aff.Update();
				if (result == UpdateResult.Burst) {
					string mainTex = aff.BurstFlashMainMask;
					string featherTex = aff.BurstFlashFeatherMask;
					Vector2 flashPos = aff.GetFlashPosition(Owner);
					Color mainCol = aff.BurstFlashMainColor;
					Color featherCol = aff.BurstFlashFeatherColor;
					aff.ApplyBurstDamage(Owner);
					BurstFlashEffect.Play(Owner, flashPos, mainTex, featherTex, mainCol, featherCol);

					foreach (var otherAff in Afflictions) {
						if (otherAff != aff)
							otherAff.ClearAccumulation();
					}

					UpdateSuppression();
					break;
				}
			}
		}
	}

	public class AfflictionGlobalNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		public AfflictionContainer Container { get; private set; }

		public override void SetDefaults(NPC npc) {
			Container = new AfflictionContainer(npc);
		}

		public override void PostAI(NPC npc) {
			Container?.Update();
		}

		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor) {
			if (Container == null || Container.Afflictions.Count == 0)
				return;

			if (npc.Center.X < Main.screenPosition.X - 100 || npc.Center.X > Main.screenPosition.X + Main.screenWidth + 100 ||
				npc.Center.Y < Main.screenPosition.Y - 100 || npc.Center.Y > Main.screenPosition.Y + Main.screenHeight + 100)
				return;

			bool anyVisible = false;
			foreach (var aff in Container.Afflictions)
				if (!aff.IsSuppressed && aff.State != AfflictionState.Idle) { anyVisible = true; break; }
			if (!anyVisible)
				return;

			float scale = Main.GameViewMatrix.Zoom.X;
			Vector2 baseScreenPos = npc.Center - Main.screenPosition;
			float baseYOffset = npc.height * 0.5f;

			Vector2 iconWorldPos = baseScreenPos + new Vector2(0, baseYOffset + 20f);
			Vector2 iconPos = Vector2.Transform(iconWorldPos, Main.GameViewMatrix.TransformationMatrix);

			Vector2 ringWorldPos = baseScreenPos + new Vector2(0, baseYOffset + 5f);
			Vector2 ringPos = Vector2.Transform(ringWorldPos, Main.GameViewMatrix.TransformationMatrix);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearClamp,
				DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Matrix.Identity);

			foreach (var aff in Container.Afflictions) {
				if (aff.IsSuppressed || aff.State == AfflictionState.Idle)
					continue;
				Vector2 drawPos = iconPos - new Vector2(0, 15f * scale);
				float featherScale = aff.FeatherScale * scale;
				float mainScale = aff.MainScale * scale;
				Texture2D featherTex = ModContent.Request<Texture2D>(aff.FeatherMaskTexture).Value;
				spriteBatch.Draw(featherTex, drawPos, null, aff.FeatherColor, 0f, featherTex.Size() * 0.5f, featherScale, SpriteEffects.None, 0);
				Texture2D iconTex = ModContent.Request<Texture2D>(aff.IconMaskTexture).Value;
				spriteBatch.Draw(iconTex, drawPos, null, aff.IconColor, 0f, iconTex.Size() * 0.5f, mainScale, SpriteEffects.None, 0);
			}
			spriteBatch.End();

			foreach (var aff in Container.Afflictions) {
				if (aff.IsSuppressed || aff.State == AfflictionState.Idle)
					continue;
				float rawProgress = (float)aff.CurrentValue / aff.MaxValue;
				float visualProgress = (aff.State == AfflictionState.Cooldown) ? rawProgress : 1f - rawProgress;
				Color ringColor = (aff.State == AfflictionState.Cooldown) ? new Color(165, 165, 165, 180) : Color.White;
				RingDrawer.DrawRing(ringPos, 5f * scale, 2.5f * scale, visualProgress, ringColor, 70);
			}

			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
				DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
		}

		public static class RingDrawer
		{
			private static DynamicVertexBuffer vertexBuffer;
			private static VertexPositionColor[] vertices = new VertexPositionColor[0];
			private static BasicEffect cachedEffect;

			public static void DrawRing(Vector2 center, float radius, float thickness, float progress, Color color, int segments = 60) {
				if (progress <= 0 || segments < 3)
					return;
				GraphicsDevice device = Main.graphics.GraphicsDevice;
				int maxSegments = (int)(segments * progress) + 1;
				if (maxSegments < 2)
					return;
				int vertCount = maxSegments * 6;
				if (vertices.Length < vertCount)
					vertices = new VertexPositionColor[vertCount];
				if (vertexBuffer == null || vertexBuffer.VertexCount < vertCount)
					vertexBuffer = new DynamicVertexBuffer(device, typeof(VertexPositionColor), vertCount, BufferUsage.WriteOnly);

				float outer = radius + thickness * 0.5f;
				float inner = radius - thickness * 0.5f;
				float angleStep = MathHelper.TwoPi / segments;
				float startAngle = -MathHelper.PiOver2;
				int vi = 0;

				for (int i = 0; i < maxSegments; i++) {
					float angle0 = startAngle + angleStep * i;
					float angle1 = startAngle + angleStep * (i + 1);
					if (i == maxSegments - 1)
						angle1 = startAngle + MathHelper.TwoPi * progress;

					Vector2 dir0 = new((float)Math.Cos(angle0), (float)Math.Sin(angle0));
					Vector2 dir1 = new((float)Math.Cos(angle1), (float)Math.Sin(angle1));
					Vector3 pOut0 = new(center + dir0 * outer, 0);
					Vector3 pIn0 = new(center + dir0 * inner, 0);
					Vector3 pOut1 = new(center + dir1 * outer, 0);
					Vector3 pIn1 = new(center + dir1 * inner, 0);

					vertices[vi++] = new VertexPositionColor(pOut0, color);
					vertices[vi++] = new VertexPositionColor(pOut1, color);
					vertices[vi++] = new VertexPositionColor(pIn0, color);
					vertices[vi++] = new VertexPositionColor(pOut1, color);
					vertices[vi++] = new VertexPositionColor(pIn1, color);
					vertices[vi++] = new VertexPositionColor(pIn0, color);
				}

				vertexBuffer.SetData(vertices, 0, vi, SetDataOptions.Discard);
				device.SetVertexBuffer(vertexBuffer);
				device.RasterizerState = RasterizerState.CullNone;
				device.DepthStencilState = DepthStencilState.Default;

				if (cachedEffect == null) {
					cachedEffect = new BasicEffect(device) { VertexColorEnabled = true, View = Matrix.Identity };
				}
				cachedEffect.World = Matrix.Identity;
				cachedEffect.View = Matrix.Identity;
				cachedEffect.Projection = Matrix.CreateOrthographicOffCenter(0f, device.Viewport.Width, device.Viewport.Height, 0f, -1f, 1f);

				foreach (var pass in cachedEffect.CurrentTechnique.Passes) {
					pass.Apply();
					device.DrawPrimitives(PrimitiveType.TriangleList, 0, vi / 3);
				}
			}
		}
	}
}