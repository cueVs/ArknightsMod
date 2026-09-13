using ArknightsMod.Content.Buffs.Supporter.Pramanix;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ArknightsMod.Common.GlobalNPCs
{
	public class PramanixDebuffGlobalNPC : GlobalNPC
	{
		public override void AI(NPC npc) {
			if (npc.HasBuff(ModContent.BuffType<PramanixSlowDebuff>())
			    && !npc.boss
			    && !Content.Items.Weapons.Supporter.Pramanix.PramanixColdAttachment.HasKnockbackGrace(npc))
				npc.velocity *= 0.65f;
		}

		public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (!npc.HasBuff(ModContent.BuffType<PramanixFreezeDebuff>()) || npc.alpha >= 255)
				return;

			Texture2D ice = TextureAssets.Frozen.Value;
			Color color = drawColor;
			color.R = (byte)(color.R * 0.55);
			color.G = (byte)(color.G * 0.55);
			color.B = (byte)(color.B * 0.55);
			color.A = (byte)(color.A * 0.55);

			float scale = MathHelper.Clamp((npc.width + npc.height) * 0.5f / ice.Height * 1.1f, 0.45f, 2.5f);
			Vector2 drawPos = npc.Center - screenPos;
			SpriteEffects effects = npc.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Main.EntitySpriteDraw(ice, drawPos, null, color, npc.rotation, ice.Size() / 2f, scale, effects, 0);
		}

		public override void OnKill(NPC npc) {
			Content.Items.Weapons.Supporter.Pramanix.PramanixColdAttachment.Clear(npc);
		}
	}
}
