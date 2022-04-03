using Godot;
using System;

public class Block : RigidBody2D
{
    [Export] public float _maxHealth = 1.0f;
    [Export] public float _spinSpeed = 10.0f;
    [Export] public float _dampVal = 0.2f;
    
    private float _currentHealth;

    private bool _isHeld;
    private bool _throw;
    private Vector2 _throwImpulse;
    private float _mouseWheel;
    private bool _dampLinVel;

    public override void _Ready()
    {
        _currentHealth = _maxHealth;
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
    }

    private void Destroy()
    {
        QueueFree();
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
                state.AngularVelocity = _mouseWheel * _spinSpeed;
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
            _dampLinVel = false;
        }
    }

    public void _on_Crate_body_entered(PhysicsBody2D other)
    {
        // when a crate hits another crate, damp the velocity to make it easier to stack them.
        if (other.IsInGroup("crate"))
        {
            _dampLinVel = true;
        }
    }
}
