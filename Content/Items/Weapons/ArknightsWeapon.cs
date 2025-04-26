using ArknightsMod.Common.Items;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ArknightsMod.Content.Items.Weapons
{
	public abstract class ArknightsWeapon : ModItem
	{
		#region 技能等级
		// 等级系统开发完成后使用
		// Usable after the Level System completed
		private int[] skillLevel = new int[3];
		public override void LoadData(TagCompound tag) {
			tag[nameof(skillLevel)] = skillLevel;
		}

		public override void SaveData(TagCompound tag) {
			skillLevel = tag.GetIntArray(nameof(skillLevel));
		}
		#endregion
		public SkillData GetSkillData(int index) => SkillDataLoader.GetSkill(Name, index);
	}
}
