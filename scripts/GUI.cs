using Godot;
using System;

public class GUI : CanvasLayer
{
    private Label _scoreLabel;
    private Control _loseScreen;
    private Label _loseScreenScore;
    
    public override void _Ready()
    {
        _scoreLabel = GetNode<Label>("Score");
        _loseScreen = GetNode<Control>("LoseScreen");
        _loseScreenScore = GetNode<Label>("LoseScreen/VBoxContainer/Score");
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        int scoreInMS = (int)(Game.Instance.Score * 1000.0f);
        int ms = (scoreInMS % 1000) / 100;
        int seconds = (scoreInMS / 1000) % 60;
        int minutes = (scoreInMS / 1000 / 60);

        if (!Game.Instance.GameOver)
        {
            _scoreLabel.Text = $"{minutes}m {seconds}.{ms}s";
            _loseScreenScore.Text = $"{minutes}m {seconds}.{ms}s";
        }
    }

    public void ShowLoseScreen()
    {
        _loseScreen.Visible = true;
        _scoreLabel.Visible = false;
    }
}
