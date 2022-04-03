﻿using Microsoft.Xna.Framework.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.JungleBloody
{
	internal class TreeJungleBloody : ModTree
    {
        public override int CreateDust() => DustID.t_LivingWood;

        public override int DropWood() => ItemID.Wood;

        public override Texture2D GetTexture() => ModContent.Request<Texture2D>("StarlightRiver/Assets/Tiles/JungleBloody/TreeJungleBloody").Value;

        public override Texture2D GetBranchTextures(int i, int j, int trunkOffset, ref int frame) => ModContent.Request<Texture2D>("StarlightRiver/Assets/Tiles/JungleBloody/TreeJungleBloody_Branches").Value;

        public override Texture2D GetTopTextures(int i, int j, ref int frame, ref int frameWidth, ref int frameHeight, ref int xOffsetLeft, ref int yOffset)
        {
            frameWidth = 116;
            frameHeight = 98;
            xOffsetLeft = 48;
            yOffset = 2;
            return ModContent.Request<Texture2D>("StarlightRiver/Assets/Tiles/JungleBloody/TreeJungleBloody_Tops").Value;
        }
    }
}