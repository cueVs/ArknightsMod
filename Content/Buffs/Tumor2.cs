using Terraria;
using Terraria.Localization;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Microsoft.Xna.Framework;
using ArknightsMod.Content.Items.Material;
using Microsoft.Xna.Framework.Graphics;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http.Headers;
using Terraria.GameContent.Biomes.Desert;
using System;
using Mono.Cecil;
using System.Reflection.Metadata;
using Terraria.GameContent;
using ArknightsMod.Common.Damageclasses;

namespace ArknightsMod.Content.Buffs
{
	public class Tumor2 : ModBuff
	{


		public override void Update(Player player, ref int buffIndex) {
			player.maxRunSpeed *= 0.35f;
			player.GetAttackSpeed(DamageClass.Generic) -= 0.75f;
			Dust.NewDust(player.position, player.width, player.height, DustID.Blood, 0f, 0f, 150, default, 1.5f);
		}
	}
}
