using Godot;
using System;

public class Block : RigidBody2D
{
    [Export] public float _maxHealth = 1.0f;
    [Export] public float _spinSpeed = 10.0f;
    [Export] public float _dampVal = 0.2f;

    private OpenSimplexNoise _noise = new OpenSimplexNoise();
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    
    private float Decay = 1.0f;
    private Vector2 MaxOffset = new Vector2(0.02f, 0.02f);
    private float MaxRoll = 0.175f;
    private int TraumaPower = 2;
    private float MaxTrauma = 0.5f;

    public Sprite Sprite;
    
    private float _currentHealth;
    private bool _isHeld;
    private bool _throw;
    private Vector2 _throwImpulse;
    private float _mouseWheel;
    private bool _dampLinVel;

    private float _trauma = 0.0f;
    private float _noiseY = 0.0f;

    public override void _Ready()
    {
        _currentHealth = _maxHealth;
        Sprite = GetNode<Sprite>("Sprite");

        MaxOffset = MaxOffset * GetViewportRect().Size;
        _noise.Seed = (int)_rng.Randi();
        _noise.Period = 4;
        _noise.Octaves = 2;
    }

    public void Pickup()
    {
        _isHeld = true;
    }

    public void Drop(Vector2 impulse)
    {
        _isHeld = false;
        _throw = true;
        _throwImpulse = impulse;
    }
    
    public void DoDamage(float damage)
    {
        if (_isHeld)
            Destroy();
        
        _currentHealth -= damage;
        if (_currentHealth <= 0.0f)
            Destroy();

        AddTrauma(damage * 2.0f);
    }

    private void AddTrauma(float trauma)
    {
        _trauma = Mathf.Min(_trauma + trauma, MaxTrauma);
    }

    private void Destroy()
    {
        Game.Instance.DestroyBlock(this);
        QueueFree();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (_trauma > 0.0f)
        {
            _trauma = Mathf.Max(_trauma - Decay * delta, 0.0f);
            var amount = Mathf.Pow(_trauma, TraumaPower);
            var rot = MaxRoll * amount * _noise.GetNoise2d(_noise.Seed, _noiseY);
            var offset = new Vector2(0.0f, 0.0f);
            offset.x = MaxOffset.x * amount * _noise.GetNoise2d(_noise.Seed * 2.0f, _noiseY);
            offset.y = MaxOffset.y * amount * _noise.GetNoise2d(_noise.Seed * 3.0f, _noiseY);
            _noiseY += delta * 100.0f;

            Sprite.Transform = new Transform2D(rot, offset);
        }
    }
    
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton)
        {
            InputEventMouseButton emb = (InputEventMouseButton)@event;
            if (emb.IsPressed())
            {
                switch (emb.ButtonIndex)
                {
                    case (int)ButtonList.WheelUp:
                        _mouseWheel = -1.0f;
                        break;
                    case (int)ButtonList.WheelDown:
                        _mouseWheel = 1.0f;
                        break;
                }
            }
        }
    }

    public override void _IntegrateForces(Physics2DDirectBodyState state)
    {
        base._IntegrateForces(state);
        
        if (_isHeld)
        {
            state.LinearVelocity = GetGlobalMousePosition() - GlobalPosition;
            state.LinearVelocity *= 0.5f;
        }
        
        if (_throw)
        {
            state.LinearVelocity = _throwImpulse;
            _throw = false;
        }

        if (_isHeld)
        {
            if (_mouseWheel != 0.0f)
            {
                state.AngularVelocity = _mouseWheel * _spinSpeed / Engine.TimeScale;
                _mouseWheel = 0.0f;
            }
            else
            {
                state.AngularVelocity = 0.0f;
            }
        }

        if (_dampLinVel)
        {
            state.LinearVelocity = state.LinearVelocity * _dampVal;
            state.AngularVelocity = state.AngularVelocity * _dampVal;
            _dampLinVel = false;
        }
    }

    public void _on_Crate_body_entered(PhysicsBody2D other)
    {
        // when a crate hits another crate, damp the velocity to make it easier to stack them.
        if (other.IsInGroup("crate") || other.IsInGroup("floor"))
        {
            _dampLinVel = true;
        }
    }
}
