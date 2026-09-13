using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l4
{
    public class ChildlikePlaythings : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 36;
            Item.accessory = true;
			Item.value = Item.sellPrice(0, 16, 0, 0);
			Item.rare = ItemRarityID.Master;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ChildlikePlaythingsPlayer>().active = true;
        }
    }

    public class ChildlikePlaythingsPlayer : ModPlayer
    {
        public bool active;
        private int timer;

        public override void ResetEffects()
        {
            active = false;
        }

        public override void PostUpdateEquips()
        {
            if (!active) return;

            timer++;
            if (timer >= 60)
            {
                timer = 0;
                ApplyDebuffDamage();
            }
        }

        private void ApplyDebuffDamage()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.life > 0 && !npc.friendly && npc.damage > 0)
                {
                    int debuffCount = CountDebuffs(npc);
                    if (debuffCount > 0)
                    {
                        int magicDamage = 100 * debuffCount;
                        
                        IEntitySource source = Player.GetSource_Misc("ChildlikePlaythings");

                        ApplyDamage(npc, magicDamage, DamageClass.Magic, source, "Magic");
                        

                        CreateVisualEffects(npc);
                    }
                }
            }
        }

        private int CountDebuffs(NPC npc)
        {
            int count = 0;
            for (int j = 0; j < npc.buffType.Length; j++)
            {
                if (npc.buffType[j] > 0 && npc.buffTime[j] > 0 && Main.debuff[npc.buffType[j]])
                {
                    count++;
                }
            }
            return count;
        }

        private void ApplyDamage(NPC npc, int damage, DamageClass damageClass, IEntitySource source, string damageText)
        {

            NPC.HitInfo hitInfo = new NPC.HitInfo()
            {
                Damage = damage,
                Knockback = 0,
                HitDirection = 0,
                Crit = false,
                DamageType = DamageClass.Magic,
            };

            npc.StrikeNPC(hitInfo);


        }

        private void CreateVisualEffects(NPC npc)
        {
            for (int k = 0; k < 15; k++)
            {
                Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height,
                    DustID.PurpleTorch, 0f, 0f, 100, default, 1.5f);
                dust.velocity *= 0.5f;
                dust.noGravity = true;
            }

            if (Main.rand.NextBool(3))
            {
                SoundEngine.PlaySound(SoundID.Item104.WithVolumeScale(0.5f), npc.Center);
            }
        }
    }
}