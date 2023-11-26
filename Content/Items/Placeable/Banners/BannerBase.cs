using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace ArknightsMod.Content.Items.Placeable.Banners
{
    public class BannerTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.SolidBottom, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.StyleWrapLimit = 111;
            TileObjectData.addTile(Type);
            DustType = -1;
            //TileID.Sets.DisableSmartCursor[Type] = true;

            AddMapEntry(new Color(13, 88, 130), CreateMapEntryName());
        }

        //public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        //{
        //    //sway in wind
        //    bool intoRenderTargets = true;
        //    bool flag = intoRenderTargets || Main.LightingEveryFrame;
        //    if (Main.tile[i, j].TileFrameX % 18 == 0 && Main.tile[i, j].TileFrameY % 54 == 0 && flag)
        //    {
        //        Main.instance.TilesRenderer.AddSpecialPoint(i, j, 5);
        //    }

        //    return false;
        //}

        //public override void KillMultiTile(int i, int j, int frameX, int frameY)
        //{
        //    int style = frameX / 18;
        //    int itemType = BannerBase.BannerIndexToItemType(style);
        //    if (itemType != 0)
        //    {
        //        Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 48, itemType);
        //    }
        //}

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (closer)
            {
                int style = Main.tile[i, j].TileFrameX / 18;
                int npcType = BannerBase.BannerIndexToNPCType(style);
                if (npcType != 0)
                {
                    int bannerItem = NPCLoader.GetNPC(npcType).BannerItem;
                    if (ItemID.Sets.BannerStrength.IndexInRange(bannerItem) && ItemID.Sets.BannerStrength[bannerItem].Enabled)
                    {
                        Main.SceneMetrics.NPCBannerBuff[npcType] = true;
                        Main.SceneMetrics.hasBanner = true;
                    }
                }
            }
        }

        //public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
        //{
        //    if (i % 2 == 1)
        //    {
        //        spriteEffects = SpriteEffects.FlipHorizontally;
        //    }
        //}
    }

    public abstract class BannerBase : ModItem
    {
        public abstract int BannerIndex { get; }
        public abstract int NPCType { get; }
        public virtual int BannerKills => 50;

        private static Dictionary<int, int> bannerIndexToNPCType = new Dictionary<int, int>();

        public override void Unload()
        {
            bannerIndexToNPCType = null;
        }

		public override void SetStaticDefaults() {
			bannerIndexToNPCType.Add(BannerIndex, NPCType);

			//SacrificeTotal = (1);

			//string npcKey = "{$Mods.Polarities.NPCName." + NPCLoader.GetNPC(NPCType).Name + "}";
			//DisplayName.SetDefault(npcKey + "{$Mods.Polarities.ItemName.BannerBase}");
			//Tooltip.SetDefault("{$CommonItemTooltip.BannerBonus}" + npcKey);

			ItemID.Sets.KillsToBanner[Type] = BannerKills;
		}

		public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 28;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 2);
            Item.createTile = TileType<BannerTile>();

            Item.placeStyle = BannerIndex;
        }

        //public override void ModifyTooltips(List<TooltipLine> tooltips)
        //{
        //    foreach (TooltipLine tooltip in tooltips)
        //    {
        //        if (tooltip.Name.Equals("Tooltip0"))
        //        {
        //            tooltip.Text = tooltip.Text.Replace("{NPCName}", Language.GetTextValue("Mods.Polarities.NPCName." + NPCLoader.GetNPC(NPCType).Name));
        //            break;
        //        }
        //    }
        //}

        public static int BannerIndexToNPCType(int index)
        {
            if (bannerIndexToNPCType.ContainsKey(index))
            {
                return bannerIndexToNPCType[index];
            }
            else
            {
                return 0;
            }
        }

        public static int BannerIndexToItemType(int index)
        {
            return NPCLoader.GetNPC(BannerIndexToNPCType(index)).BannerItem;
        }
    }

    public class OriginiumSlugBanner : BannerBase { public override int BannerIndex => 0; public override int NPCType => NPCType<NPCs.Enemy.OriginiumSlug>(); }
	public class OriginiumSlugAlphaBanner : BannerBase { public override int BannerIndex => 1; public override int NPCType => NPCType<NPCs.Enemy.OriginiumSlugAlpha>(); }
}