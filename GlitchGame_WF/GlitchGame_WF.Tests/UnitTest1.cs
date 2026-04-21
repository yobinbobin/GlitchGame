using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GlitchGame_WF.Controller;
using GlitchGame_WF.Models;

namespace GlitchGame_WF.Tests;

public class GameControllerGlitchTests
{
    [Fact]
    public void Level2_Inverts_AD_HorizontalInput()
    {
        var controller = new GameController();
        controller.NextLevel(); // level 2
        float startX = controller.PlayerX;

        controller.HandleInput(new HashSet<Keys> { Keys.A });

        Assert.True(controller.PlayerX > startX);
    }

    [Fact]
    public void Level3_InputLag_DelaysFirstMovement()
    {
        var controller = new GameController();
        controller.NextLevel(); // level 2
        controller.NextLevel(); // level 3
        float startX = controller.PlayerX;

        controller.HandleInput(new HashSet<Keys> { Keys.D });

        Assert.Equal(startX, controller.PlayerX);
    }

    [Fact]
    public void Level4_HyperSpeed_UsesBiggerHorizontalStepThanBase()
    {
        var controller = new GameController();
        controller.NextLevel(); // level 2
        controller.NextLevel(); // level 3
        controller.NextLevel(); // level 4
        float startX = controller.PlayerX;

        controller.HandleInput(new HashSet<Keys> { Keys.D });

        Assert.InRange(controller.PlayerX - startX, 5.0001f, float.MaxValue);
    }

    [Fact]
    public void Level5_EnablesPhantomCollisionMode()
    {
        var controller = new GameController();
        controller.NextLevel(); // level 2
        controller.NextLevel(); // level 3
        controller.NextLevel(); // level 4
        controller.NextLevel(); // level 5

        Assert.True(controller.IsPhantomCollisionModeEnabled);
    }

    [Fact]
    public void NextLevel_AfterLevel5_WrapsBackToLevel1()
    {
        var controller = new GameController();
        controller.NextLevel();
        controller.NextLevel();
        controller.NextLevel();
        controller.NextLevel();
        controller.NextLevel();

        Assert.Equal(1, controller.CurrentLevelNumber);
    }
}

public class PlatformCollisionTests
{
    [Fact]
    public void ApplyPlatforms_IgnoresPhantomPlatforms_WhenFlagEnabled()
    {
        var player = new Player
        {
            X = 10,
            Y = 50,
            VelocityY = 10
        };
        var phantomPlatform = new Platform(0, 100, 200, 20, isPhantom: true);

        player.ApplyGravity();
        player.ApplyPlatforms(new List<Platform> { phantomPlatform }, ignorePhantomPlatforms: true);

        Assert.True(player.Y > 60);
    }

    [Fact]
    public void Level5_ContainsPhantomPlatforms()
    {
        var level = new LevelManager().GoToLevel(5);

        Assert.Contains(level.Platforms, p => p.IsPhantom);
    }
}
