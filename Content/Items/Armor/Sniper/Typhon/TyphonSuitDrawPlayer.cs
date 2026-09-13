using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.Typhon
{
	public class TyphonSuitDrawPlayer : ModPlayer
	{
		public override void TransformDrawData(ref PlayerDrawSet drawInfo)
		{
			Player p = drawInfo.drawPlayer;
			if (!TyphonVanityAnim.BodyFrameMatchesHornsLongStrip(p))
				return;
			if (!IsTyphonBodyEquipped(p))
				return;

			/*Texture2D bodyArmorTex = TextureAssets.ArmorBody[p.body].Value;
			var cache = drawInfo.DrawDataCache;
			for (int i = 0; i < cache.Count; i++) {
				DrawData d = cache[i];
				if (d.texture != bodyArmorTex)
					continue;
				Vector2 pos = d.position;
				pos.Y -= p.gravDir;
				d.position = pos;
				cache[i] = d;
			}*/
		}

		// 迁移改动：原先手写"比对 armor[1] / armor[11] 的 ItemID"，那样只认时装、
		// 漏掉套装形态（套装件是另一个独立 ItemID）。统一走 IsPartVisible，两者都认，
		// 而且它已经处理了"时装栏覆盖盔甲栏外观"的原版规则。
		private static bool IsTyphonBodyEquipped(Player p)
			=> NeoArmorReforgeSetLoader.IsPartVisible<TyphonBody>(p, EquipType.Body);
	}
}
