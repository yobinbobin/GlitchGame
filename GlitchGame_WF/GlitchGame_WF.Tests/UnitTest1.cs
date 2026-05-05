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
        for (int i = 0; i < 10; i++)
            controller.NextLevel();

        Assert.Equal(1, controller.CurrentLevelNumber);
    }

    [Fact]
    public void Level10_IsCelebrationLevel()
    {
        var controller = new GameController();
        for (int i = 0; i < 9; i++)
            controller.NextLevel();

        Assert.True(controller.IsCelebrationLevel);
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

public class LevelDefinitionTests
{
    [Fact]
    public void LevelManager_Has10Levels()
    {
        var manager = new LevelManager();
        Assert.Equal(10, manager.MaxLevel);
    }

    [Fact]
    public void GroundPlatform_IsWiderThanScreen_AndThicker()
    {
        var level1 = new LevelManager().GoToLevel(1);
        var ground = level1.Platforms.First(p => p.Y == 500);

        Assert.True(ground.Width >= 882);
        Assert.True(ground.Height >= 40);
    }
}

public class PlayerPhysicsTests
{
    [Fact]
    public void ApplyGravity_ClampsPlayerTopToGroundTop()
    {
        var player = new Player
        {
            X = 0,
            Y = 1000,
            VelocityY = 0
        };
        player.GroundY = 500;

        player.ApplyGravity(reverseGravity: false);

        Assert.Equal(500 - player.Height, player.Y);
        Assert.True(player.IsGrounded);
    }
}

public class CollectibleRulesTests
{
    [Fact]
    public void CollectCoins_FakeCoin_DoesNotIncreaseScore_WhenFlagEnabled()
    {
        var player = new Player { X = 0, Y = 0 };
        var coins = new List<Coin>
        {
            new Coin(0, 0, isFake: true),
        };

        player.CollectCoins(coins, ignoreFakeCoinScore: true);

        Assert.Equal(0, player.Score);
        Assert.True(coins[0].Collected);
    }

    [Fact]
    public void CollectPlatforms_CollectiblePlatform_IncreasesScoreAndMarksCollected()
    {
        var player = new Player { X = 10, Y = 10 };
        var platforms = new List<Platform>
        {
            new Platform(0, 0, 100, 40, isCollectible: true),
        };

        player.CollectPlatforms(platforms);

        Assert.Equal(10, player.Score);
        Assert.True(platforms[0].Collected);
    }
}
