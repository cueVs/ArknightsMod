using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

#pragma warning disable CS8981

namespace ArknightsMod.Content.Buffs
{
	public class sjds : ModBuff
	{
		public override void SetStaticDefaults()
		{
			//DisplayName.SetDefault("神幻诅咒");
			//Description.SetDefault("你的灵魂正在被焚烧");
			Main.debuff[Type] = true;  // Is it a debuff?
			Main.pvpBuff[Type] = true; // Players can give other players buffs, which are listed as pvpBuff
			Main.buffNoSave[Type] = true; // Causes this buff not to persist when exiting and rejoining the world

			//BuffID.Sets.LongerExpertDebuff[Type] = true; // If this buff is a debuff, setting this to true will make this buff last twice as long on players in expert mode
		}
		public override void Update(Player player, ref int buff)
		{
			if (player.lifeRegen > 0)
			{
				player.lifeRegen = 0;
			}
			player.lifeRegenTime = 0;
			// 让玩家的减血速率增加40
			player.lifeRegen -= 1;
		}
		public override void Update(NPC npc, ref int buffIndex)
		{
			if (!npc.immortal)
			{
                if (npc.HasBuff(ModContent.BuffType<sjds2>()))
                {
                    for (int i = 0; i < 19; i++) 
                    { 
                        if (npc.buffType[i]== ModContent.BuffType<sjds>())
                        {
                            npc.buffType[i] = 0;
                            break;
                        }
                            
                    }
                }
                npc.life -= 1;//减血速率
				
                if (Main.netMode != NetmodeID.MultiplayerClient && npc.life % 4 == 0)
                {
                    CombatText.NewText(npc.Hitbox, CombatText.DamagedHostile, 1,false,true);//伤害
                    Dust v = Dust.NewDustDirect(npc.Center, 0, 0, 1, 0f, 0f, 0, new Color(255, 180, 20, 220), 1.7f);
                    v.noGravity = true;
                    //Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.GreenFairy);
                    //dust.velocity *= 3;//减血速率
                    //dust.scale *= 2f;
                    //dust.noGravity = true;
                }
				npc.checkDead();
			}
           
        }
	}
    public class sjds2 : ModBuff
    {
        public override string Texture => "ArknightsMod/Content/Buffs/sjds";
        public override void SetStaticDefaults()
        {
            //DisplayName.SetDefault("神幻诅咒");
            //Description.SetDefault("你的灵魂正在被焚烧");
            Main.debuff[Type] = true;  // Is it a debuff?
            Main.pvpBuff[Type] = true; // Players can give other players buffs, which are listed as pvpBuff
            Main.buffNoSave[Type] = true; // Causes this buff not to persist when exiting and rejoining the world

            //BuffID.Sets.LongerExpertDebuff[Type] = true; // If this buff is a debuff, setting this to true will make this buff last twice as long on players in expert mode
        }
        public override void Update(Player player, ref int buff)
        {
            if (player.lifeRegen > 0)
            {
                player.lifeRegen = 0;
            }
            player.lifeRegenTime = 0;
            // 让玩家的减血速率增加40
            player.lifeRegen -= 1;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            if (!npc.immortal)
            {
                npc.life -= 1;//减血速率
                
                if (Main.netMode != NetmodeID.MultiplayerClient&& npc.life % 4==0)
                {
                    npc.life -= 1;
                    CombatText.NewText(npc.Hitbox, CombatText.DamagedHostile, 2, false, true);//伤害
                    Dust v = Dust.NewDustDirect(npc.Center, 0, 0, 1, 0f, 0f, 0, new Color(255, 180, 20, 220), 1.7f);
                    v.noGravity = true;
                    //Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.GreenFairy);
                    //dust.velocity *= 3;//减血速率
                    //dust.scale *= 2f;
                    //dust.noGravity = true;
                }
                npc.checkDead();
            }

        }
    }
}
