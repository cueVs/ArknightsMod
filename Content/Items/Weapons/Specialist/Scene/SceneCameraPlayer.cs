using ArknightsMod.Content.Projectiles.Specialist.Scene;
using ArknightsMod.Players;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Specialist.Scene
{
	// 稀音的摄像机：右键部署冷却（5 秒）+ 两个技能的激活状态/时长管理。
	// 技能用「Down(S)+右键」释放当前在技能栏选中的技能；技力充能/消耗复用 WeaponPlayer。
	public class SceneCameraPlayer : ModPlayer
	{
		public const int RedeployCooldownMax = 300; // 5 秒 @60fps
		public const int Skill2DurationTicks = 20 * 60;
		public const int Skill2StunTicks = 5 * 60;

		public int RedeployCooldown;
		public bool CanRedeploy => RedeployCooldown <= 0;

		// 技能状态（latch 在玩家身上，效果生效仍以「手持摄像机」为门槛）
		public bool Skill1Active;
		public int Skill2Timer;

		private static readonly SoundStyle SkillActiveSound = new("ArknightsMod/Sounds/SkillActive1") {
			Volume = 0.4f,
			MaxInstances = 4,
		};

		public void StartRedeployCooldown() => RedeployCooldown = RedeployCooldownMax;

		public override void PostUpdate() {
			if (RedeployCooldown > 0)
				RedeployCooldown--;

			// 死亡时清空技能状态
			if (Player.dead) {
				Skill1Active = false;
				Skill2Timer = 0;
				if (Player.whoAmI == Main.myPlayer) {
					var deadWp = Player.GetModPlayer<WeaponPlayer>();
					deadWp.SkillActive = false; // 死亡时清空技力条持续状态
					deadWp.SkillTimer = 0;
				}
				return;
			}

			bool holding = Player.whoAmI == Main.myPlayer && Player.HeldItem.ModItem is SceneCamera;

			if (Skill2Timer > 0) {
				var wp = Player.GetModPlayer<WeaponPlayer>();
				if (Player.whoAmI == Main.myPlayer) {
					if (holding && wp.Skill == 1) {
						wp.SkillActive = true;
						wp.SkillTimer = Skill2DurationTicks - Skill2Timer; // 同步技力条倒退
					}
					else if (!Skill1Active) {
						wp.SkillActive = false; // 非二技能槽时清除遗留状态
						wp.SkillTimer = 0;
					}
				}

				if (holding)
					Skill2Timer--; // 仅手持时推进

				if (Skill2Timer == 0) {
					if (Player.whoAmI == Main.myPlayer && !Skill1Active) {
						wp.SkillActive = false; // 结束时重置技力条
						wp.SkillTimer = 0;
					}
					if (holding)
						CameraTruck.StunAllForPlayer(Player, Skill2StunTicks);
				}
			}

			// 输入：手持 + Down(S) 按住 + 右键刚按下 → 释放选中技能
			if (holding && !BlocksInput() && Player.controlDown && PlayerInput.Triggers.JustPressed.MouseRight)
				TryActivateSelectedSkill();

			if (holding && Skill1Active)
				SceneCameraSkills.MaintainSkill1ChargeState(Player); // 一技能期间保持满层显示
		}

		private bool BlocksInput() =>
			Player.mouseInterface || Main.playerInventory || Player.talkNPC >= 0 || Player.chest != -1;

		// 供武器右键路径调用：手持且无 UI 遮挡时释放当前选中技能。
		public void TryActivateSelectedSkillFromInput() {
			if (Player.whoAmI != Main.myPlayer) return;
			if (!(Player.HeldItem.ModItem is SceneCamera)) return;
			if (BlocksInput()) return;
			TryActivateSelectedSkill();
		}

		private void TryActivateSelectedSkill() {
			var wp = Player.GetModPlayer<WeaponPlayer>();
			if (wp.StockCount <= 0)
				return;

			switch (wp.Skill) {
				case 0:
					if (Skill1Active)
						return;
					Skill1Active = true;
					wp.SkillActive = true; // 开启持续状态
					wp.SkillTimer = 0;
					SceneCameraSkills.MaintainSkill1ChargeState(Player);
					SoundEngine.PlaySound(SkillActiveSound, Player.Center);
					break;
				case 1:
					if (Skill2Timer > 0 || wp.SkillActive) // 持续中不可重复释放
						return;
					wp.SkillActive = true; // 开启持续状态
					wp.SkillTimer = 0;
					wp.UpdateActiveSkill2(); // 由 SceneCameraPlayer 驱动计时
					wp.DelStockCount();
					Skill2Timer = Skill2DurationTicks; // 启动 20 秒持续
					// 立即额外召唤一辆「免位」摄影车（不占仆从位，总数仍受 5 上限约束）
					if (CameraTruck.CountActiveForPlayer(Player) < CameraTruck.MaxTrucks)
						SceneCamera.SummonTruck(Player, Player.GetSource_Misc("SceneCameraSkill2"), free: true);
					SoundEngine.PlaySound(SkillActiveSound, Player.Center);
					break;
			}
		}
	}
}
