using Godot;
using System;
using System.Collections.Generic;

public class Player : KinematicBody2D
{
    [Export] public float _moveAccel = 1.0f;
    [Export] public float _moveMax = 32.0f;
    [Export] public float _throwStrength = 1.0f;
    [Export] public AudioStream _footstepSfx;

    public bool Dead = false;
    
    private Sprite _sprite;
    private Grabber _grabber;
    private Trajectory _trajectory;
    private List<Block> _potentialGrabs = new List<Block>();
    private bool _leftMouseDown = false;
    private Block _grabbed = null;
    private float _footstepTimer;
    private Vector2 _vel;
    
    private AudioStreamPlayer _sfx;
    private AudioStreamPlayer _sfxThrow;

    private CPUParticles2D _footstepVFXLeft;
    private CPUParticles2D _footstepVFXRight;

    public override void _Ready()
    {
        _sprite = GetNode<Sprite>("Sprite");
        _grabber = GetNode<Grabber>("Grabber");
        _trajectory = GetNode<Trajectory>("../Trajectory");
        _sfx = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        _sfxThrow = GetNode<AudioStreamPlayer>("AudioStreamPlayer2");
        _footstepVFXLeft = GetNode<CPUParticles2D>("FootstepVFXLeft");
        _footstepVFXRight = GetNode<CPUParticles2D>("FootstepVFXRight");
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (!IsInstanceValid(_grabbed))
        {
            _potentialGrabs.Remove(_grabbed);
            _grabbed = null;
            _trajectory.Visible = false;
        }

        if (_grabbed == null)
        {
            _footstepTimer -= delta * Mathf.Clamp(_vel.Length(), 1.0f, 10.0f);
            _sprite.Offset = new Vector2(0.0f, Mathf.Sin(-Mathf.Clamp(_footstepTimer, 0.0f, 1.0f) * 5.0f));
            if (_footstepTimer < 0.0f && _vel.Length() > 1.0f)
            {
                _sfx.Stream = _footstepSfx;
                _sfx.Play();
                _footstepTimer = 1.0f;
                if (_vel.x < 0.0f)
                {
                    _footstepVFXLeft.Emitting = true;
                    _footstepVFXLeft.Restart();
                }
                else
                {
                    _footstepVFXRight.Emitting = true;
                    _footstepVFXRight.Restart();
                }
            }
        }

        _sprite.FlipH = _vel.x < 0.0f ? true : false;
        
        if (Input.IsActionPressed("grab"))
        {
            if (!_leftMouseDown)
            {
                _leftMouseDown = true;
            
                // find the closest potential grab target.
                Block closest = null;
                float dist = float.MaxValue;
                foreach (Block crate in _potentialGrabs)
                {
                    float d = (crate.GlobalPosition - _grabber.GlobalPosition).Length();
                    if (d < dist)
                    {
                        closest = crate;
                        dist = d;
                    }
                }

                if (closest != null)
                {
                    _grabbed = closest;
                    _grabbed.Pickup();
                    _trajectory.Visible = true;
                }
            }
        }
        else
        {
            _leftMouseDown = false;
            if (_grabbed != null)
            {
                _grabbed.Drop(GetThrowImpulse());
                _sfxThrow.Play();
                _grabbed = null;
                _trajectory.Visible = false;
            }
        }

        if (_trajectory.Visible)
        {
            if (_grabbed != null)
            {
                _trajectory.Setup(GetThrowImpulse(), _grabbed.Sprite, _grabbed.Rotation);
                _trajectory.GlobalPosition = _grabber.GlobalPosition;
            }
        }
    }

    private Vector2 GetThrowImpulse()
    {
        if (_grabbed == null)
            return Vector2.Zero;
         
        return (GetViewport().GetMousePosition() - _grabber.GlobalPosition) * _throwStrength; 
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        _grabber.GlobalPosition =
            GlobalPosition + (GetViewport().GetMousePosition() - GlobalPosition).Clamped(_moveMax);

        float deadzone = 8.0f;
        if ((_grabber.GlobalPosition - GlobalPosition).Length() > deadzone)
        {
            Vector2 target = _grabber.GlobalPosition - (_grabber.GlobalPosition - GlobalPosition).Normalized() * deadzone;
            target.y = target.y = GlobalPosition.y;
            Vector2 gravity = Vector2.Down * Game.Instance.Gravity;
            _vel = (target - GlobalPosition).Clamped(_moveMax);
            if (_grabbed == null)
            {
                MoveAndSlide(_vel * _moveAccel + gravity);
            }

            float l = _vel.x / _moveMax + 0.5f;
            _sprite.Rotation = Mathf.Lerp(-Mathf.Pi * 0.25f, Mathf.Pi * 0.25f, Mathf.Clamp(l, 0.0f, 1.0f));
        }

        if (_grabbed != null)
        {
            _grabbed.GlobalPosition = _grabber.GlobalPosition;
        }
    }

    public void _on_Grabber_body_entered(RigidBody2D other)
    {
        if (other.IsInGroup("crate"))
        {
            Block block = other as Block;
            _potentialGrabs.Add(block);
        }
    }
    
    public void _on_Grabber_body_exited(RigidBody2D other)
    {
        if (other.IsInGroup("crate"))
        {
            Block block = other as Block;
            _potentialGrabs.Remove(block);
        }
    }

    public void _on_Area2D_body_entered(PhysicsBody2D other)
    {
        if (other.IsInGroup("roof"))
        {
            Dead = true;
        }
    }
}
