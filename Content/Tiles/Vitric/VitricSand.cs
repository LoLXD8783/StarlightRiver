﻿using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	/*internal class VitricSandGrad : ModTile
    {
        public override void SetDefaults()
        {
            QuickBlock.QuickSet(this, 0, DustType<Content.Dusts.Air>(), SoundID.Dig, new Color(172, 131, 105), Mod.ItemType("VitricSandItem"));
            Main.tileMerge[Type][TileID.Sandstone] = true;
            Main.tileMerge[Type][TileID.HardenedSand] = true;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return base.TileFrame(i, j, ref resetFrame, ref noBreak);
        }
    }

    internal class VitricSandGradItem : QuickTileItem 
    {
        public VitricSandGradItem() : base("Glassy Sand Test", "", StarlightRiver.Instance.TileType("VitricSand"), 0) { }
    }*/

	internal class VitricSandWall : ModWall
    {
        public override string Texture => AssetDirectory.VitricTile + Name;

        public override void SetDefaults()
        {
            WallID.Sets.Conversion.HardenedSand[Type] = true;
            QuickBlock.QuickSetWall(this, DustID.Copper, SoundID.Dig, ItemType<VitricSandWallItem>(), false, new Color(114, 78, 80));
        }
    }

    internal class VitricSandWallItem : QuickWallItem
    { 
        public VitricSandWallItem() : base("Vitric Sand Wall", "", WallType<VitricSandWall>(), 0, AssetDirectory.VitricTile) { } 
    }
}

