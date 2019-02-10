﻿using System;
using UnityEngine;

namespace Isometric.Items
{
    public class ItemCoin : Item
    {
        public ItemCoin(string name) : base(name)
        {

        }

        public override int maxStack
        {
            get
            { return 999; }
        }

        public override FAtlasElement element
        {
            get
            { return Futile.atlasManager.GetElementWithName("items/coin"); }
        }
    }
}