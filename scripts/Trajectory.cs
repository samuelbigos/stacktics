using Godot;
using System.Collections.Generic;

public class Trajectory : Line2D
{
    private List<Sprite> _preview = new List<Sprite>();
    private List<float> _previewPoints = new List<float>();

    public override void _Ready()
    {
        _preview.Add(GetNode<Sprite>("Sprite"));
        _preview.Add(GetNode<Sprite>("Sprite2"));
        _preview.Add(GetNode<Sprite>("Sprite3"));
        _preview.Add(GetNode<Sprite>("Sprite4"));
        _preview.Add(GetNode<Sprite>("Sprite5"));

        for (int i = 0; i < _preview.Count; i++)
        {
            _previewPoints.Add((Points.Length / _preview.Count) * i);
        }
    }
    
    public override void _Process(float delta)
    {
        for (int i = 0; i < _preview.Count; i++)
        {
            _previewPoints[i] += delta * 100.0f;
            int p = (int) _previewPoints[i];
            p = p % (Points.Length - 1);
            _preview[i].GlobalPosition = GlobalPosition + Points[p];
        }
    }

    public void Setup(Vector2 impulse, Sprite preview, float rot)
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

        for (int i = 0; i < _preview.Count; i++)
        {
            _preview[i].Texture = preview.Texture;
            _preview[i].Rotation = rot;
            Color col = preview.Modulate;
            col.a *= 0.3f;
            _preview[i].Modulate = col;
        }
    }
}
