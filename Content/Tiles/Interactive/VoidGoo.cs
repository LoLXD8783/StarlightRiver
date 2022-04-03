﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Interactive
{
	internal class VoidGoo : ModTile
    {
        private int Frame = 0;

        public override string Texture => AssetDirectory.InteractiveTile + Name;

        public override void SetDefaults()
        {
            (this).QuickSet(int.MaxValue, DustType<Dusts.Void>(), SoundID.Drown, Color.Black, ItemType<VoidGooItem>());
            animationFrameHeight = 88;
        }

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            if (++frameCounter >= 5)
            {
                frameCounter = 0;
                if (++frame >= 3)
                    frame = 0;
            }
            Frame = frame;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Tiles/Interactive/VoidGooGlow").Value;
            spriteBatch.Draw(tex, (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition, new Rectangle(Main.tile[i, j].TileFrameX, Main.tile[i, j].TileFrameY + 88 * Frame, 16, 16), Color.White);
        }
    }

    internal class VoidGooItem : QuickTileItem { public VoidGooItem() : base("Void Goo", "", TileType<VoidGoo>(), 0, AssetDirectory.InteractiveTile) { } }
}