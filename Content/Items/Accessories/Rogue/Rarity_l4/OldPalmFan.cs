using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.Localization;
namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l4
{
    public class OldPalmFan : ModItem
    {
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			int currentCount = CountAliveTownNPCs();
			float currentBonus = 0.03f * currentCount;
			Player player = Main.LocalPlayer;
			if (player != null) {
				string TXT = Language.GetTextValue("Mods.Alk");//当前人数/加成
				// 添加提示(负责本地化的可能会很累)
				tooltips.Add(new TooltipLine(Mod, "npc", $"{TXT} {currentCount}/{currentBonus}"));
				foreach (TooltipLine line in tooltips) {
					if (line.Mod == "Terraria" || line.Mod == Mod.Name) {
					}
				}
			}
		}
		public override void SetStaticDefaults()
        {
           
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
			Item.value = Item.sellPrice(0, 16, 0, 0);
			Item.rare = ItemRarityID.Master;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            
            int townNpcCount = CountAliveTownNPCs();


            float damageBonus = 0.03f * townNpcCount;


            player.GetDamage(DamageClass.Generic) += damageBonus;
        }


        //计算世界上所有存活的城镇NPC数量

        private int CountAliveTownNPCs()
        {
            int count = 0;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                // 检查NPC是否满足条件：
                
                if (npc.active && npc.townNPC && npc.friendly && npc.lifeMax > 5)
                {
                    count++;
                }
            }

            return count;
        }

        
        
    }
}