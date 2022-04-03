﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Tiles.Permafrost;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Forest
{
	internal class AuroraBerryBush : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.PermafrostTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            AnchorData anchor = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 2, 0);
            int[] valid = new int[] { ModContent.TileType< PermafrostIce>(), ModContent.TileType<PermafrostSnow>() };

            //TileObjectData.newTile.RandomStyleRange = 3;
            TileObjectData.newTile.DrawYOffset = 2;
            QuickBlock.QuickSetFurniture(this, 2, 2, DustID.Grass, SoundID.Dig, false, new Color(200, 255, 220), false, false, "", anchor, default, valid);
        }

        public override void RandomUpdate(int i, int j) //RandomUpdate is vanilla's less-than-ideal way of handling having the entire world loaded at once. a bunch of tiles update every tick at pure random. thanks redcode.
        {
            Tile tile = Framing.GetTileSafely(i, j); //you could probably add more safety checks if you want to be extra giga secure, but we assume RandomUpdate only calls valid tiles here
            TileObjectData data = TileObjectData.GetTileData(tile); //grabs the TileObjectData associated with our tile. So we dont have to use as many magic numbers
            int fullFrameWidth = data.Width * (data.CoordinateWidth + data.CoordinatePadding); //the width of a full frame of our multitile in pixels. We get this by multiplying the size of 1 full frame with padding by the width of our tile in tiles.

            if (tile.frameX == 0 && tile.frameY % 36 == 0) //this checks to make sure this is only the top-left tile. We only want one tile to do all the growing for us, and top-left is the standard. otherwise each tile in the multitile ticks on its own due to stupid poopoo redcode.
                if (Main.rand.Next(2) == 0 && tile.frameX == 0) //a random check here can slow growing as much as you want.
                    for (int x = 0; x < data.Width; x++) //this for loop iterates through every COLUMN of the multitile, starting on the top-left.
                        for (int y = 0; y < data.Height; y++) //this for loop iterates through every ROW of the multitile, starting on the top-left.
                        {
                            //These 2 for loops together iterate through every specific tile in the multitile, allowing you to move each one's frame
                            Tile targetTile = Main.tile[i + x, j + y]; //find the tile we are targeting by adding the offsets we find via the for loops to the coordinates of the top-left tile.
                            targetTile.frameX += (short)fullFrameWidth; //adds the width of the frame to that specific tile's frame. this should push it forward by one full frame of your multitile sprite. cast to short because vanilla.
                        }
        }

        public override bool NewRightClick(int i, int j)
        {
            if (Main.tile[i, j].frameX > 35) //Only runs if it has berries
            {
                Tile tile = Framing.GetTileSafely(i, j); //Selects current tile

                int newX = i; //Here to line 67 adjusts the tile position so we get the top-left of the multitile
                int newY = j;
                if (tile.frameX % 36 == 18) newX = i - 1;
                if (tile.frameY % 36 == 18) newY = j - 1;

                for (int k = 0; k < 2; k++)
                    for (int l = 0; l < 2; ++l)
                        Main.tile[newX + k, newY + l].frameX -= 36; //Changes frames to berry-less

                Item.NewItem(new Vector2(i, j) * 16, ItemType<AuroraBerry>()); //Drops berries
            }
            return true;
        }

        public override void MouseOver(int i, int j)
        {
            if (Framing.GetTileSafely(i, j).frameX >= 32)
            {
                Player Player = Main.LocalPlayer;
                Player.showItemIcon2 = ItemType<AuroraBerry>();
                Player.noThrow = 2;
                Player.showItemIcon = true;
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new Vector2(i * 16, j * 16), ItemType<AuroraBerry>()); //drop a bush Item

            //if (frameX > 35)
            //    Item.NewItem(new Vector2(i, j) * 16, ItemType<ForestBerries>()); //Drops berries if harvestable
        }
    }

    public class AuroraBerry : ModItem
    {
        public override string Texture => AssetDirectory.PermafrostTile + Name;

        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            //Item.consumable = true;
            Item.maxStack = 99;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.SwingThrow;
            //Item.useStyle = ItemUseStyleID.EatingUsing;
            //Item.healLife = 5;
            //Item.potion = true;
            //Item.UseSound = SoundID.Item2;
            Item.createTile = TileType<AuroraBerryBush>();
        }

        public override bool UseItem(Player Player)
        {
            //Player.AddBuff(BuffID.PotionSickness, 15);
            return true;
        }
    }
}