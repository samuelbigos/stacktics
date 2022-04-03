using Godot;
using System;

public class Trajectory : Line2D
{
    public override void _Ready()
    {
    }

    public void Setup(Vector2 impulse)
    {
        Vector2 p = Vector2.Zero;
        Vector2 v = impulse;
        Vector2 gravity = new Vector2(0.0f, 98.0f);
        float step = 1.0f / 60.0f;

        Vector2[] points = new Vector2[Points.Length];
        for (int i = 0; i < Points.Length; i++)
        {
            points[i] = p;
            v += gravity * step;
            p += v * step;
        }

        Points = points;
    }
}
