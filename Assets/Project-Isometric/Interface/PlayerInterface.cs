﻿using UnityEngine;
using Isometric.Items;
using Custom;

namespace Isometric.Interface
{
    public enum CursorType
    {
        None,
        Construct,
        Destruct,
        Target
    }

    public class PlayerInterface : MenuFlow
    {
        private Player _player;
        public Player player
        {
            get { return _player; }
        }
        public WorldCamera worldCamera
        {
            get { return _player.worldCamera; }
        }

        private ItemSelect itemSelect;
        private InventoryMenu inventoryMenu;

        private WorldCursor[] _cursors;
        private WorldCursor _currentCursor;

        public PlayerInterface(Player player) : base()
        {
            _player = player;

            itemSelect = new ItemSelect(this);
            inventoryMenu = new InventoryMenu(this);
        }

        public override void OnActivate()
        {
            base.OnActivate();

            _cursors = new WorldCursor[]
            {
                null,
                new ConstructCursor(this, worldCamera),
                new DestructCursor(this, worldCamera),
                new TargetCursor(this)
            };

            SetCursor(CursorType.None);
        }

        public void SetCursor(CursorType cursorType)
        {
            WorldCursor newCursor = _cursors[(int)cursorType];

            if (_currentCursor == newCursor)
                return;

            if (_currentCursor != null)
                _currentCursor.RemoveSelf();

            _currentCursor = newCursor;

            if (_currentCursor != null)
                AddElement(_currentCursor);
        }

        public override void RawUpdate(float deltaTime)
        {
            if (!player.game.paused)
                base.RawUpdate(deltaTime);
        }

        public override void Update(float deltaTime)
        {
            if (!inventoryMenu.activated)
            {
                if (Input.mouseScrollDelta.y != 0f && !itemSelect.activated)
                    AddSubLoopFlow(itemSelect);
                if (Input.GetKey(KeyCode.I))
                    AddSubLoopFlow(inventoryMenu);

                if (_currentCursor != null)
                    _currentCursor.CursorUpdate(player.world, player, MenuFlow.mousePosition);
            }

            base.Update(deltaTime);
        }

        public class ItemSelect : MenuFlow
        {
            public const int length = 8;

            private PlayerInterface playerInterface;

            public Player player
            {
                get
                { return playerInterface.player; }
            }
            
            private ItemContainerVisualizer[] visualizer;
            private int selectedIndex;

            private float factor;
            private float sleepTime;

            private float scrollAmount;
            private float selectSpriteAngle;

            public ItemSelect(PlayerInterface playerInterface) : base()
            {
                this.playerInterface = playerInterface;

                visualizer = new ItemContainerVisualizer[length];
                for (int index = 0; index < length; index++)
                {
                    visualizer[index] = new ItemContainerVisualizer(this, player.inventory[index]);
                    AddElement(visualizer[index]);
                }

                factor = 0f;
                sleepTime = 0f;
                scrollAmount = 0f;
                selectSpriteAngle = 0f;
            }

            public override void Update(float deltaTime)
            {
                Vector2 anchorPosition = player.screenPosition + Vector2.up * 16f;
                float scrollDelta = Input.mouseScrollDelta.y;

                sleepTime = scrollDelta != 0 ? 1f : sleepTime - deltaTime;
                factor = Mathf.Clamp01(factor + (sleepTime > 0f ? deltaTime : -deltaTime) * 5f);

                if (!(factor > 0f))
                    Terminate();

                scrollAmount += scrollDelta;

                int oldSelectedIndex = selectedIndex;
                selectedIndex = (int)Mathf.Repeat(scrollAmount, length);

                if (oldSelectedIndex != selectedIndex)
                    player.PickItem(player.inventory[selectedIndex]);

                selectSpriteAngle = Mathf.LerpAngle(selectSpriteAngle, (float)selectedIndex / length * 360f, deltaTime * 10f);

                for (int index = 0; index < length; index++)
                {
                    bool selected = index == selectedIndex;
                    float radian = (index + 1f - factor) / length * Mathf.PI * 2f;

                    visualizer[index].position = Vector2.Lerp(visualizer[index].position, anchorPosition + (new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)) * factor * (selected ? 40f : 32f)), deltaTime * 10f);
                    visualizer[index].container.alpha = factor;
                }

                base.Update(deltaTime);
            }
        }
    }
}