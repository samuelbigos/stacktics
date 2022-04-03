using Godot;
using System;

public class Game : Node2D
{
    [Export] public PackedScene _crateScene;

    public static Game Instance;

    public float Score = 0.0f;
    public bool GameOver = false;
    
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private GUI _GUI;
    private Player _player;

    public override void _Ready()
    {
        Instance = this;
        
        for (int i = 0; i < 10; i++)
            SpawnCrate();

        _GUI = GetNode<GUI>("CanvasLayer");
        _player = GetNode<Player>("Player");
    }

    public override void _Process(float delta)
    {
        Score += delta;

        if (_player.Dead && !GameOver)
        {
            GameOver = true;
            _GUI.ShowLoseScreen();
        }
    }

    public void SpawnCrate()
    {
        Block addedBlock = _crateScene.Instance<Block>();
        addedBlock.GlobalPosition = new Vector2(_rng.Randi() % 256, 128 + _rng.Randi() % 64);
        addedBlock.Rotation = _rng.Randf() * 2.0f * (float)Math.PI;
        AddChild(addedBlock);
    }

    public void _on_Restart_button_down()
    {
        GetTree().ReloadCurrentScene();
    }
}
