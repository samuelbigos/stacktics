using Godot;
using System;
using System.Collections.Generic;

public class Game : Node2D
{
    [Export] public List<PackedScene> _blockScenes;
    [Export] public PackedScene _blockDestroyVFX;
    [Export] public float _spawnFreq;
    [Export] public float _slowmoRegen = 0.1f;
    [Export] public float _slowmoDegen = 0.25f;
    
    [Export] public Color WallColour;
    [Export] public Color PlayerColour;

    [Export] public List<AudioStream> _destroySfx;
    [Export] public AudioStream _dieSfx;

    [Export] public float Gravity;
    
    public static Game Instance;

    public float Score = 0.0f;
    public bool GameOver = false;
    public float DamageAmp = 1.0f;
    public bool InIntro = false;
    
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private GUI _GUI;
    private Player _player;
    private AudioStreamPlayer _sfx;
    private Camera2D _cam;
    private ProgressBar _slowmoBar;
    private float _spawnTimer;
    private int _spawned = 0;
    private int _blocksLeft = 0;
    private int _blocksRight = 0;
    private float _camYMax;
    private float _slowmoVal = 1.0f;
    private bool _slowmoCD;
    private float _slowmoCDTimer;
    private Button _startButton;
    private float _time;

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

        _sfx = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        _cam = GetNode<Camera2D>("Camera2D");
        _camYMax = _cam.GlobalPosition.y;
        _slowmoBar = GetNode<ProgressBar>("CanvasLayer/Slowmo");

        if (GetNode<Global>("/root/Global").InIntro)
        {
            _startButton = GetNode<Button>("CanvasLayer/StartButton");
            _startButton.Visible = true;
            GetNode<Control>("CanvasLayer/Score").Visible = false;
            InIntro = true;
        }
    }

    public override void _Process(float delta)
    {
        Score += delta / Engine.TimeScale;
        _time += delta / Engine.TimeScale;
        
        float camMaxY = 0.0f;
        Vector2 centre = new Vector2(160.0f, 160.0f);
        Vector2 cameraMouseOffset = GetGlobalMousePosition() - centre;
        cameraMouseOffset.x = 0.0f;
        cameraMouseOffset.y = Mathf.Min(camMaxY, cameraMouseOffset.y);
        var camerOffset = -centre + GetViewport().Size * 0.5f - cameraMouseOffset * 0.2f;
        var cameraTransform = new Transform2D(new Vector2(1.0f, 0.0f), new Vector2(0.0f, 1.0f), camerOffset);
        GetViewport().CanvasTransform = cameraTransform;

        if (InIntro)
        {
            _startButton.RectPivotOffset = _startButton.RectSize / 2.0f;
            _startButton.RectScale = new Vector2(1.25f + Mathf.Abs(Mathf.Sin(_time)) * 0.5f,
                1.25f + Mathf.Abs(Mathf.Sin(_time)) * 0.5f);;
            _startButton.RectRotation = (Mathf.Sin(_time)) * 5.0f;
        }

        if (_player != null && _player.Dead && !GameOver)
        {
            GameOver = true;
            _GUI.ShowLoseScreen();
            DestroyPlayer();
        }

        _spawnTimer -= delta;
        if (_spawnTimer < 0.0f)
        {
            _spawnTimer = _spawnFreq;
            Vector2 pos;
            if (_blocksLeft == 0 && _blocksRight == 0)
            {
                pos = _spawned % 2 == 0 ? new Vector2(-16, 150) : new Vector2(336, 150);
                SpawnCrate(pos);
            }
            else if (_blocksLeft == 0)
            {
                pos = new Vector2(-32, 150);
                SpawnCrate(pos);
            }
            else if (_blocksRight == 0)
            {
                pos = new Vector2(352, 150);
                SpawnCrate(pos);
            }
        }

        if (!GameOver)
        {
            _slowmoVal = Mathf.Min(_slowmoVal + delta * _slowmoRegen, 1.0f);
            _slowmoBar.Value = _slowmoVal;
            
            _slowmoCDTimer -= delta;
            if (_slowmoCDTimer < 0.0f)
            {
                
                if (Input.IsActionPressed("slowmo") && _slowmoVal > 0.0f)
                {
                    _slowmoVal = Mathf.Max(_slowmoVal - _slowmoDegen * delta * 10.0f, 0.0f);
                    Engine.TimeScale = 0.1f;
                    _slowmoBar.Visible = true;
                    
                    if (_slowmoVal <= 0.0f)
                    {
                        _slowmoCD = true;
                        _slowmoCDTimer = 2.5f;
                        Engine.TimeScale = 1.0f;
                    }
                }
                else
                {
                    Engine.TimeScale = 1.0f;
                    _slowmoBar.Visible = _slowmoVal > 0.9f ? false : true;
                }
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

        _sfx.Stream = _destroySfx[(int) (_rng.Randi() % _destroySfx.Count)];
        _sfx.Play();
    }
    
    public void DestroyPlayer()
    {
        CPUParticles2D vfx = _blockDestroyVFX.Instance() as CPUParticles2D;
        AddChild(vfx);
        vfx.GlobalPosition = _player.GlobalPosition;
        vfx.Color = new Color("db1c1c");
        vfx.Emitting = true;

        _sfx.Stream = _dieSfx;
        _sfx.Play();
        _player.QueueFree();
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
            DamageAmp = 2.0f;
            _blocksRight++;
            _blocksLeft++;
        }
    }

    public void _on_Restart_button_down()
    {
        GetTree().ReloadCurrentScene();
    }

    public void _on_StartButton_button_down()
    {
        GetNode<Global>("/root/Global").InIntro = false;
        GetTree().ReloadCurrentScene();
    }
}
