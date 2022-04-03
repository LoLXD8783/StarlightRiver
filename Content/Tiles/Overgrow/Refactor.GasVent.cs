﻿using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Overgrow
{
	internal class GasVent : ModTile
    {
        public override string Texture => AssetDirectory.OvergrowTile + "GasVent";

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = false;
            Main.tileFrameImportant[Type] = true;
            Main.tileBlockLight[Type] = false;
            Main.tileLighted[Type] = true;

            dustType = DustType<Dusts.Gas>();
            AddMapEntry(new Color(255, 186, 66));
        }
    }
}