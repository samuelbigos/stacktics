using Godot;
using System;
using System.Collections.Generic;

public class Roof : StaticBody2D
{
      [Export] public float _speed = 5.0f;
      [Export] public float _damage = 1.0f;
      
      private List<Block> _contactingBodies = new List<Block>();

      public override void _Ready()
      {
      }

      public override void _Process(float delta)
      {
          if (Game.Instance.InIntro)
              return;
          
          if (_contactingBodies.Count == 0)
          {
              GlobalPosition += Vector2.Down * _speed;
          }
          
          foreach (Block crate in _contactingBodies)
          {
              // do less damage if there's multiple objects in the way, with some falloff.
              float dmgMod = 1.0f / _contactingBodies.Count;
              dmgMod /= 1.0f - Mathf.Pow(1.0f - dmgMod, 3.0f);
              crate.DoDamage(_damage * dmgMod * delta * Game.Instance.DamageAmp);
          }
      }

      public void _on_Area2D_body_entered(RigidBody2D other)
      {
          if (other.IsInGroup("crate"))
          {
              Block block = other as Block;
              _contactingBodies.Add(block);
          }
      }

      public void _on_Area2D_body_exited(RigidBody2D other)
      {
          if (other.IsInGroup("crate"))
          {
              Block block = other as Block;
              _contactingBodies.Remove(block);
          }
      }
}
