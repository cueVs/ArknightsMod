using ArknightsMod.Content.Projectiles.Caster.Lava;
using ArknightsMod.Content.Tiles.Infrastructure;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Caster.Lava
{
	public class Lava_Dagger : UpgradeWeaponBase
	{
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<Material.Oriron>(2);
			recipe.AddTile(ModContent.TileType<FactoryTile>());
			recipe.Register();
		}
		private static SoundStyle SkillActive1;
		private static SoundStyle NoSound;
		public override void Load() {
			SkillActive1 = new SoundStyle("ArknightsMod/Sounds/SkillActive1") {
				Volume = 0.4f,
				MaxInstances = 4,
			};
			NoSound = new SoundStyle("ArknightsMod/Sounds/NoSound") {
				Volume = 0f,
				MaxInstances = 4,
			};
		}
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			Texture2D tex = ModContent.Request<Texture2D>("ArknightsMod/Content/Items/Weapons/Caster/Lava/LavaWeapon").Value;
			spriteBatch.Draw(tex, position, null, drawColor, 0f, tex.Size() / 2, scale, SpriteEffects.None, 0f);
			return false;
		}
		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			Texture2D tex = ModContent.Request<Texture2D>("ArknightsMod/Content/Items/Weapons/Caster/Lava/LavaWeapon").Value;
			spriteBatch.Draw(tex, Item.position - tex.Size() / 2 - Main.screenPosition, null,
				lightColor, rotation, tex.Size() / 2, scale, SpriteEffects.None, 0f);
			return false;
		}
		public override void SetDefaults()
		{
			Item.damage = 40;
			Item.knockBack = 3f;
			Item.useAnimation = 87;
			Item.useTime = 87;
			Item.shootSpeed = 8f;

			Item.shoot = ModContent.ProjectileType<Lava_Dagger_Explode>();
			Item.DamageType = DamageClass.Magic;
			Item.useStyle = ItemUseStyleID.Swing;
			//Item.UseSound = SoundID.Item1;
			//Item.rare = ItemRarityID.Green;
			//Item.value = Item.sellPrice(silver: 5);

			Item.noMelee = true;
			Item.useTurn = true;
		}
		public override void HoldItem(Player player)
		{
			// 检查法书效果是否已经存在
			bool exist = false;
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<LavaSpellbookHoldout>() && Main.projectile[i].owner == player.whoAmI)
				{
					exist = true;
					break;
				}
			}

			if (!exist)
			{
				Projectile.NewProjectile(
					player.GetSource_FromThis(),
					player.Center,
					Vector2.Zero,
					ModContent.ProjectileType<LavaSpellbookHoldout>(),
					0,
					0f,
					player.whoAmI
				);
			}
		}
		public override bool AltFunctionUse(Player player) => false;
		public override bool CanUseItem(Player player) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				if (ArknightsKeybinds.SkillActivatePressed(player)) {
					if (!modPlayer.SummonMode) {
						if (modPlayer.Skill == 0 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
							modPlayer.SkillActive = true;
							modPlayer.SkillTimer = 0;

							modPlayer.DelStockCount();
							Item.UseSound = SkillActive1;
							SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
						}
						else
							return false;
					}
				}
				else {
					if (!modPlayer.SummonMode) {
						Item.UseSound = NoSound;
						if (modPlayer.Skill == 0 && modPlayer.SkillActive) {
							player.GetAttackSpeed(DamageClass.Magic) += 0.5f;
							Item.UseSound = NoSound;
						}
					}
				}
			}
			return base.CanUseItem(player);
		}
	}

	public class LavaSpellbookHoldout : ModProjectile
	{
		private Vector2 pos;

		public override string Texture => "ArknightsMod/Content/Projectiles/Caster/Lava/Lava_Spellbook_Tex";

		public override void SetDefaults()
		{
			Projectile.width = Projectile.height = 32;
			Projectile.timeLeft = 2;
			Projectile.tileCollide = false;
			Projectile.hide = true;
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];

			if (player.dead || !player.active || player.HeldItem?.type != ModContent.ItemType<Lava_Dagger>())
			{
				Projectile.Kill();
				return;
			}

			Vector2 target = player.MountedCenter + new Vector2(player.direction * 24, 0);

			if (Projectile.timeLeft == 2)
			{
				Projectile.Center = target;
				pos = target;
			}
			else
			{
				pos = Vector2.Lerp(pos, target, 0.7f);
				Projectile.Center = pos;
			}

			Projectile.rotation = 0f;
			Projectile.spriteDirection = player.direction;

			Projectile.timeLeft = 2;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			overPlayers.Add(index);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

			Vector2 position = Projectile.Center - Main.screenPosition;

			SpriteEffects spriteEffects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			Main.EntitySpriteDraw(
				texture,
				position,
				null,
				lightColor,
				0f,
				texture.Size() * 0.5f,
				1f,
				spriteEffects,
				0
			);

			return false;
		}

		public override bool? CanDamage() => false;
	}
}
