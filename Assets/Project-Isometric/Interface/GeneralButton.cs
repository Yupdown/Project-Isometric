﻿using System;
using UnityEngine;

namespace Isometric.Interface
{
    public class GeneralButton : ButtonBase
    {
        private RoundedRect rect1;
        private RoundedRect rect2;

        private FLabel label;

        private Action clickCallback;

        private float hoverfactor;

        private static AudioClip _onHoverAudio;
        private static AudioClip _onPressAudio;

        public GeneralButton(Menu menu, string name, Action clickCallback) : base(menu)
        {
            rect1 = new RoundedRect(menu);
            rect2 = new RoundedRect(menu);

            AddElement(rect1);
            AddElement(rect2);

            label = new FLabel("font", name);
            label.scale = 0.5f;
            container.AddChild(label);

            this.clickCallback = clickCallback;

            if (_onHoverAudio == null)
            {
                _onHoverAudio = Resources.Load<AudioClip>("SoundEffects/UIMetal3");
                _onPressAudio = Resources.Load<AudioClip>("SoundEffects/UIArp");
            }
        }

        public override void OnActivate()
        {
            base.OnActivate();

            hoverfactor = 0f;
        }

        public override void Update(float deltaTime)
        {
            hoverfactor = Mathf.Lerp(hoverfactor, hovering && !pressing ? 1f : 0f, deltaTime * 16f);

            Vector2 rectSize = size - Vector2.one * 12f;

            rect1.size = rectSize + Vector2.Lerp(Vector2.zero, Vector2.one * 6f, hoverfactor);
            rect2.size = rectSize + Vector2.Lerp(Vector2.zero, Vector2.one * 2f, hoverfactor);

            base.Update(deltaTime);
        }

        public override void OnMouseHover()
        {
            base.OnMouseHover();

            AudioEngine.PlaySound(_onHoverAudio);
        }

        public override void OnPressUp()
        {
            base.OnPressUp();

            if (clickCallback != null)
                clickCallback();

            AudioEngine.PlaySound(_onPressAudio);
        }
    }
}