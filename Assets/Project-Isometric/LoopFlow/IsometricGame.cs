﻿using UnityEngine;
using System.Collections;
using Isometric.Interface;

public class IsometricGame : LoopFlow
{
    private World world;

    private PauseMenu pauseMenu;

    public override void OnActivate()
    {
        base.OnActivate();

        world = new World(this);
        pauseMenu = new PauseMenu(this);
    }

    public override void Update(float deltaTime)
    {
        world.Update(deltaTime);

        base.Update(deltaTime);
    }

    public override void OnTerminate()
    {
        world.OnTerminate();

        base.OnTerminate();
    }

    public override bool OnExecuteEscape()
    {
        AddSubLoopFlow(pauseMenu);

        return false;
    }
}