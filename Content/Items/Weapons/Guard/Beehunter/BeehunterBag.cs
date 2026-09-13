using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Guard.Beehunter
{
	public class BeehunterBag : ExpansionWeaponBase
	{
		// 手持/挥砍时显示棒球棍贴图；库存图标用 PreDrawInInventory 画盾牌
		public override string Texture =>
			"ArknightsMod/Content/Items/Weapons/Guard/Beehunter/BeehunterBat";

		protected override int[] EliteDamage => [23, 28, 34];

		private static SoundStyle SkillActiveSfx;

		public override void Load() {
			SkillActiveSfx = new SoundStyle("ArknightsMod/Sounds/SkillActive1") { Volume = 0.5f, MaxInstances = 2 };
		}

		public override void SetDefaults() {
			Item.damage = EliteDamage[0];
			Item.DamageType = DamageClass.Melee;
			Item.width = 49;
			Item.height = 49;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.knockBack = 4f;
			Item.value = Item.sellPrice(silver: 20);
			Item.rare = ItemRarityID.Green;
			Item.autoReuse = true;
			Item.useStyle = ItemUseStyleID.Swing; // 原版挥砍，引擎自动处理两个方向
			Item.crit = 4;
		}

		// 库存格子里画盾牌贴图
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
				Color drawColor, Color itemColor, Vector2 origin, float scale) {
			Texture2D shield = ModContent.Request<Texture2D>(
				"ArknightsMod/Content/Items/Weapons/Guard/Beehunter/BeehunterShield").Value;
			spriteBatch.Draw(shield, position, null, drawColor, 0f, shield.Size() / 2f, scale, SpriteEffects.None, 0f);
			return false;
		}

		public override bool AltFunctionUse(Player player) => false;

		public override bool CanUseItem(Player player) {
			var mp = player.GetModPlayer<WeaponPlayer>();

			if (ArknightsKeybinds.SkillActivatePressed(player)) {
				if (mp.StockCount > 0 && !mp.SkillActive) {
					mp.SkillActive = true;
					mp.SkillTimer = 0;
					mp.DelStockCount();
					SoundEngine.PlaySound(SkillActiveSfx, player.Center);
				}
				return false;
			}

			// S2 壳状防御：停止攻击
			var mpCheck = player.GetModPlayer<WeaponPlayer>();
			if (mpCheck.SkillActive && mpCheck.Skill == 1) return false;

			return base.CanUseItem(player);
		}

		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			base.ModifyWeaponDamage(player, ref damage);
			var mp = player.GetModPlayer<WeaponPlayer>();
			if (mp.SkillActive && mp.Skill == 0)
				damage *= 1.8f; // S1 防御力强化·B：攻击 +80%
		}

		public override void HoldItem(Player player) {
			base.HoldItem(player);
			var mp = player.GetModPlayer<WeaponPlayer>();
			if (!mp.SkillActive) return;

			if (mp.Skill == 0) {
				// S1 防御力强化·B：防御 +80%
				player.statDefense += (int)((int)player.statDefense * 0.8f);
			} else {
				// S2 壳状防御：防御 +130%，缓慢回血 3%/s
				player.statDefense += (int)((int)player.statDefense * 1.3f);
				if (player.statLife < player.statLifeMax) {
					float heal = player.statLifeMax * 0.03f / 60f;
					if (Main.rand.NextFloat() < heal - (int)heal) player.statLife++;
					player.statLife = System.Math.Min(player.statLife + (int)heal, player.statLifeMax);
				}
			}
		}
	}
}
