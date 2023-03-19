using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria;
using Terraria.ModLoader;


namespace ArknightsMod.Common.Systems
{
	public class WeaponPlayer : ModPlayer
	{
		// SkillGauge
		public float Charge = 0;
		public float overCharge1 = 0;
		public float overCharge2 = 0;
		public const int ChargeMax = 100;
		public bool ChargeActive = false;

	}
}