using Godot;
using System;

public class GameHeader : PanelContainer {
	private Button playButton;
	private Label dateDisplay;
	private ToolButton speedDownButton;
	private ToolButton speedUpButton;
	private ToolButton speedDisplay;
	private GameView gameView;

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		playButton = GetNode<ToolButton>("MarginContainer/HBoxContainer/Left/PlayButton");
		dateDisplay = GetNode<Label>("MarginContainer/HBoxContainer/Left/DateDisplay");
		speedDownButton = GetNode<ToolButton>("MarginContainer/HBoxContainer/Left/SpeedDownButton");
		speedUpButton = GetNode<ToolButton>("MarginContainer/HBoxContainer/Left/SpeedUpButton");
		speedDisplay = GetNode<ToolButton>("MarginContainer/HBoxContainer/Left/SpeedDisplay");
		dateDisplay.Text = gameView.game.date.ToString();
		gameView.game.GameDateChanged.Subscribe((GameDate date) => {
			dateDisplay.Text = date.ToString();
		});

		playButton.Connect("pressed", this, nameof(handlePlayPressed));
		speedDownButton.Connect("pressed", this, nameof(handleSpeedDown));
		speedUpButton.Connect("pressed", this, nameof(handleSpeedUp));
		speedDisplay.Connect("pressed", this, nameof(handleSpeedDisplayPress));

		gameView.game.PlayState.Subscribe((bool isPlaying) => {
			if (isPlaying) {
				playButton.Icon = ResourceLoader.Load<Texture>("res://assets/icons/PlayIcon.tres");
			} else {
				playButton.Icon = ResourceLoader.Load<Texture>("res://assets/icons/PauseIcon.tres");
			}
		});

		gameView.game.Speed.Subscribe((GameSpeed speed) => {
			if (speed == GameSpeed.Slow) {
				speedDisplay.Icon = ResourceLoader.Load<Texture>("res://assets/SpeedDisplaySlow.tres");
			} else if (speed == GameSpeed.Normal) {
				speedDisplay.Icon = ResourceLoader.Load<Texture>("res://assets/SpeedDisplayNormal.tres");
			} else if (speed == GameSpeed.Fast) {
				speedDisplay.Icon = ResourceLoader.Load<Texture>("res://assets/SpeedDisplayFast.tres");
			}
		});
	}

	private void handlePlayPressed() {
		if (gameView.game.IsPlaying) {
			gameView.game.Pause();
		} else {
			gameView.game.Play();
		}
	}

	private void handleSpeedDown() {
		gameView.game.Slower();
	}

	private void handleSpeedUp() {
		gameView.game.Faster();
	}

	private void handleSpeedDisplayPress() {
		gameView.game.ToggleSpeed();
	}
}
