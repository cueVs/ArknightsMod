using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace ArknightsMod.Content.Items.Weapons.Supporter.Pramanix
{
	public static class PramanixSoundHelper
	{
		private const float TailFadeSeconds = 0.12f;

		private static SoundStyle DomainHit;
		private static SoundStyle ShockwaveFire;
		private static SoundStyle Skill2Activate;
		private static SoundStyle Skill3Activate;
		private static SoundStyle Skill3Attack;
		private static bool loaded;

		public static void Load() {
			DomainHit = new SoundStyle("ArknightsMod/Sounds/PramanixDomainHit") {
				Volume = 1f,
				MaxInstances = 32,
			};
			ShockwaveFire = new SoundStyle("ArknightsMod/Sounds/PramanixShockwaveFire") {
				Volume = 1f,
				MaxInstances = 16,
			};
			Skill2Activate = new SoundStyle("ArknightsMod/Sounds/PramanixSkill2Activate") {
				Volume = 1f,
				MaxInstances = 4,
			};
			Skill3Activate = new SoundStyle("ArknightsMod/Sounds/PramanixSkill3Activate") {
				Volume = 1f,
				MaxInstances = 4,
			};
			Skill3Attack = new SoundStyle("ArknightsMod/Sounds/PramanixSkill3Attack") {
				Volume = 1f,
				MaxInstances = 8,
			};
			loaded = true;
		}

		public static void PlayDomainHit(Vector2 position) {
			if (!loaded || Main.netMode == NetmodeID.Server)
				return;
			PlayWithTailFade(DomainHit, position);
		}

		public static void PlayShockwaveFire(Vector2 position) {
			if (!loaded || Main.netMode == NetmodeID.Server)
				return;
			PlayWithTailFade(ShockwaveFire, position);
		}

		public static void PlaySkill2Activate(Vector2 position) {
			if (!loaded || Main.netMode == NetmodeID.Server)
				return;
			SoundEngine.PlaySound(Skill2Activate, position);
		}

		public static void PlaySkill3Activate(Vector2 position) {
			if (!loaded || Main.netMode == NetmodeID.Server)
				return;
			SoundEngine.PlaySound(Skill3Activate, position);
		}

		public static void PlaySkill3Attack(Vector2 position) {
			if (!loaded || Main.netMode == NetmodeID.Server)
				return;
			SoundEngine.PlaySound(Skill3Attack, position);
		}

		private static void PlayWithTailFade(SoundStyle style, Vector2 position) {
			float duration = (float)style.GetSoundEffect().Duration.TotalSeconds;
			SoundEngine.PlaySound(style, position, CreateTailFadeCallback(duration, TailFadeSeconds));
		}

		private static SoundUpdateCallback CreateTailFadeCallback(float totalSeconds, float fadeSeconds) {
			int[] startTick = { -1 };
			return sound => {
				if (startTick[0] < 0)
					startTick[0] = (int)Main.GameUpdateCount;

				float elapsed = (Main.GameUpdateCount - startTick[0]) / 60f;
				float fadeStart = MathHelper.Max(0f, totalSeconds - fadeSeconds);

				if (elapsed >= totalSeconds)
					return false;

				if (elapsed >= fadeStart) {
					float fadeT = (elapsed - fadeStart) / fadeSeconds;
					sound.Volume = MathHelper.Clamp(1f - fadeT, 0f, 1f);
				}

				return true;
			};
		}
	}
}
