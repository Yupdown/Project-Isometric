﻿using System;
using Isometric.Interface;
using UnityEngine;

namespace Isometric.Items
{
    public class ItemBlock : Item
    {
        private Block block;

        public ItemBlock(string name, string blockKey) : base(name)
        {
            this.block = Block.GetBlockByKey(blockKey);
        }

        public override void OnUseItem(World world, Player player, ItemContainer itemContainer, Vector3Int tilePosition)
        {
            itemContainer.Apply(-1);

            world.PlaceBlock(tilePosition, block);
        }

        public override FAtlasElement element
        {
            get
            { return block.sprite; }
        }

        public override HoldType holdType
        {
            get
            { return HoldType.Block; }
        }

        public override CursorType cursorType
        {
            get
            { return CursorType.Construct; }
        }
    }
}