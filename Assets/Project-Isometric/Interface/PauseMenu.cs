﻿using UnityEngine;
using Custom;

namespace Isometric.Interface
{
    public class PauseMenu : PopupMenuFlow
    {
        private LoopFlow pauseTarget;
        private OptionsMenu optionsMenu;

        private FSprite[] cinematicEdge;

        private GeneralButton[] buttons;

        private static AudioClip _onPauseAudio;

        public PauseMenu(LoopFlow pauseTarget) : base(null, true, 0.5f, 0.5f)
        {
            this.pauseTarget = pauseTarget;

            optionsMenu = new OptionsMenu(this);

            AddElement(new FadePanel(this));

            cinematicEdge = new FSprite[2];
            for (int index = 0; index < cinematicEdge.Length; index++)
            {
                cinematicEdge[index] = new FSprite("pixel");

                cinematicEdge[index].width = Futile.screen.width;
                cinematicEdge[index].color = Color.black;
                cinematicEdge[index].y = Futile.screen.halfHeight * (index > 0 ? 1f : -1f);
            }

            container.AddChild(cinematicEdge[0]);
            container.AddChild(cinematicEdge[1]);

            buttons = new GeneralButton[3];
            buttons[0] = new GeneralButton(this, "Resume", RequestTerminate);
            buttons[1] = new GeneralButton(this, "To Menu", BackToMenu);
            buttons[2] = new GeneralButton(this, "Options", OpenOptions);

            for (int index = 0; index < buttons.Length; index++)
            {
                buttons[index].size = new Vector2(48f, 16f);
                AddElement(buttons[index]);
            }

            buttons[0].position = MenuFlow.rightDown + Vector2.right * -30f;
            buttons[1].position = MenuFlow.leftDown + Vector2.right * 30f;
            buttons[2].position = MenuFlow.leftDown + Vector2.right * 90f;

            if (_onPauseAudio == null)
            {
                _onPauseAudio = Resources.Load<AudioClip>("SoundEffects/UIMetalHit");
            }
        }

        public override void Update(float deltaTime)
        {
            pauseTarget.paused = !(factor < 1f);
            pauseTarget.timeScale = 1f - Mathf.Clamp01(factor * 2f);

            for (int index = 0; index < cinematicEdge.Length; index++)
                cinematicEdge[index].scaleY = Mathf.Lerp(0f, 72f, CustomMath.Curve(factor, -3f));

            for (int index = 0; index < buttons.Length; index++)
                buttons[index].position = new Vector2(buttons[index].position.x,MenuFlow.screenHeight * -0.5f + Mathf.Lerp(-16f, 16f, CustomMath.Curve(factor * 4f - (index + 1), -1f)));

            base.Update(deltaTime);
        }

        public override void OnActivate()
        {
            base.OnActivate();

            AudioEngine.PlaySound(_onPauseAudio);
        }

        public override void OnTerminate()
        {
            pauseTarget.paused = false;
            pauseTarget.timeScale = 1f;

            base.OnTerminate();
        }

        public void BackToMenu()
        {
            loopFlowManager.RequestSwitchLoopFlow(new MainMenu());
        }

        public void OpenOptions()
        {
            AddSubLoopFlow(optionsMenu);
        }
    }
}