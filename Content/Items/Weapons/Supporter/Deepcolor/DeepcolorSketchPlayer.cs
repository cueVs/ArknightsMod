using ArknightsMod.Content;
using ArknightsMod.Content.Projectiles.Supporter.Deepcolor;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Supporter.Deepcolor
{
	public class DeepcolorSketchPlayer : ModPlayer
	{
		public const int RedeployCooldownMax = 300;

		public int RedeployCooldown;
		public bool CanRedeploy => RedeployCooldown <= 0;
		public bool IsLogoChargeActive { get; private set; }

		// 首次右键 LOGO 时锁定，供打击弹幕使用（弹幕 ai 仅 3 槽）
		public Vector2 LogoStrikeWorld;

		private static int _logoChargeType = -1;

		private static int LogoChargeType =>
			_logoChargeType >= 0 ? _logoChargeType : _logoChargeType = ModContent.ProjectileType<DeepcolorSketchLogoAttack>();

		public void StartRedeployCooldown() {
			RedeployCooldown = RedeployCooldownMax;
		}

		public override void PostUpdate() {
			if (RedeployCooldown > 0)
				RedeployCooldown--;

			UpdateLogoChargeActive();

			if (!IsLocalHoldingSketch())
				return;

			// 技能开启键：释放选中的技能。原来是"按住 Down 时，右键松开"这个组合手势，
			// 现在统一挪到独立热键，不再需要额外按住方向键——TryActivateSkill 内部本来就有
			// !SkillActive 的守卫，按住热键连续调用也不会重复触发，不需要另外做按下/松开的边沿检测。
			if (ArknightsKeybinds.SkillActivatePressed(Player) && !BlocksLogoInput())
				DeepcolorSketch.TryActivateSkill(Player);
		}

		private void UpdateLogoChargeActive() {
			IsLogoChargeActive = false;
			if (Player.HeldItem.ModItem is not DeepcolorSketch)
				return;

			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile proj = Main.projectile[i];
				if (proj.active && proj.owner == Player.whoAmI && proj.type == LogoChargeType) {
					IsLogoChargeActive = true;
					return;
				}
			}
		}

		private bool IsLocalHoldingSketch() =>
			Player.whoAmI == Main.myPlayer && Player.HeldItem.ModItem is DeepcolorSketch;

		private bool BlocksLogoInput() =>
			Player.mouseInterface
			|| Main.playerInventory
			|| Main.LocalPlayer.talkNPC >= 0
			|| Main.LocalPlayer.chest != -1;

		public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
			if (!DeepcolorSketchSkills.IsOwnerInAnyTentacleAttackRange(Player))
				return;

			if (Main.rand.NextFloat() >= DeepcolorSketchSkills.VisualTrapDodgeChance)
				return;

			modifiers.Cancel();
		}
	}
}
