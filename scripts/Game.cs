using Godot;
using System;
using System.Collections.Generic;

public class Game : Node2D
{
    [Export] public List<PackedScene> _blockScenes;
    [Export] public PackedScene _blockDestroyVFX;
    [Export] public float _spawnFreq;
    
    [Export] public Color WallColour;
    [Export] public Color PlayerColour;

    public static Game Instance;

    public float Score = 0.0f;
    public bool GameOver = false;
    
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private GUI _GUI;
    private Player _player;
    private float _spawnTimer;
    private int _spawned = 0;
    private int _blocksLeft = 0;
    private int _blocksRight = 0;
    
    public override void _Ready()
    {
        Instance = this;
        
        _rng.Randomize();
        
        for (int i = 0; i < 3; i++)
            SpawnCrate(new Vector2(32 + _rng.Randi() % 288, 128 + _rng.Randi() % 64));

        _spawnTimer = _spawnFreq;

        _GUI = GetNode<GUI>("CanvasLayer");
        _player = GetNode<Player>("Player");
        
        GetNode<Sprite>("Roof/Sprite").Modulate = WallColour;
        GetNode<Sprite>("ConveyorRight/Sprite").Modulate = WallColour;
        GetNode<Sprite>("ConveyorLeft/Sprite").Modulate = WallColour;
        GetNode<Sprite>("Floor/Sprite").Modulate = WallColour;
        GetNode<Sprite>("Player/Sprite").Modulate = PlayerColour;
        GetNode<Sprite>("Player/Grabber/Sprite").Modulate = PlayerColour;
    }

    public override void _Process(float delta)
    {
        Score += delta;

        if (_player.Dead && !GameOver)
        {
            GameOver = true;
            _GUI.ShowLoseScreen();
        }

        _spawnTimer -= delta;
        if (_spawnTimer < 0.0f)
        {
            _spawnTimer = _spawnFreq;
            Vector2 pos;
            if (_blocksLeft == 0 && _blocksRight == 0)
            {
                pos = _spawned % 2 == 0 ? new Vector2(-16, 196) : new Vector2(336, 196);
                SpawnCrate(pos);
            }
            else if (_blocksLeft == 0)
            {
                pos = new Vector2(-16, 196);
                SpawnCrate(pos);
            }
            else if (_blocksRight == 0)
            {
                pos = new Vector2(336, 196);
                SpawnCrate(pos);
            }
        }
    }

    public void DestroyBlock(Block block)
    {
        CPUParticles2D vfx = _blockDestroyVFX.Instance() as CPUParticles2D;
        AddChild(vfx);
        vfx.GlobalPosition = block.GlobalPosition;
        vfx.Color = block.Sprite.Modulate;
        vfx.Emitting = true;
    }

    public void SpawnCrate(Vector2 pos)
    {
        Block addedBlock = _blockScenes[(int) (_rng.Randi() % _blockScenes.Count)].Instance<Block>();
        addedBlock.GlobalPosition = pos;
        addedBlock.Rotation = _rng.Randf() * 2.0f * (float)Math.PI;
        AddChild(addedBlock);
        _spawned++;
    }
    
    public void _on_StopSpawnLeft_body_entered(PhysicsBody2D other)
    {
        if (other.IsInGroup("crate"))
            _blocksLeft++;
    }
    
    public void _on_StopSpawnLeft_body_exited(PhysicsBody2D other)
    {
        if (other.IsInGroup("crate"))
            _blocksLeft--;
    }
    
    public void _on_StopSpawnRight_body_entered(PhysicsBody2D other)
    {
        if (other.IsInGroup("crate"))
            _blocksRight++;
    }
    
    public void _on_StopSpawnRight_body_exited(PhysicsBody2D other)
    {
        if (other.IsInGroup("crate"))
            _blocksRight--;
    }

    public void _on_StopSpawnLeft_area_entered(Area2D area)
    {
        if (area.IsInGroup("roof"))
        {
            _blocksRight++;
            _blocksLeft++;
        }
    }

    public void _on_Restart_button_down()
    {
        GetTree().ReloadCurrentScene();
    }
}
