using Godot;
using System;

public class GameHeader : PanelContainer {
	private TextureButton playButton;
	private TextureButton pauseButton;
	private Label dateDisplay;
	private ToolButton speedDownButton;
	private ToolButton speedUpButton;
	private ToolButton speedDisplay;
	private TextureButton menuButton;
	private GameView gameView;

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		playButton = GetNode<TextureButton>("%PlayButton");
		pauseButton = GetNode<TextureButton>("%PauseButton");
		dateDisplay = GetNode<Label>("%DateDisplay");
		speedDownButton = GetNode<ToolButton>("%SpeedDownButton");
		speedUpButton = GetNode<ToolButton>("%SpeedUpButton");
		speedDisplay = GetNode<ToolButton>("%SpeedDisplay");
		menuButton = GetNode<TextureButton>("%MenuButton");

		dateDisplay.Text = gameView.game.date.ToString();
		gameView.game.GameDateChanged.Subscribe((GameDate date) => {
			dateDisplay.Text = date.ToString();
		});

		playButton.Connect("pressed", this, nameof(handlePlayPressed));
		pauseButton.Connect("pressed", this, nameof(handlePlayPressed));
		speedDownButton.Connect("pressed", this, nameof(handleSpeedDown));
		speedUpButton.Connect("pressed", this, nameof(handleSpeedUp));
		speedDisplay.Connect("pressed", this, nameof(handleSpeedDisplayPress));
		menuButton.Connect("pressed", this, nameof(handleMenuPress));

		

		gameView.game.PlayState.Subscribe((bool isPlaying) => {
			if (isPlaying) {
				pauseButton.Hide();
				playButton.Show();
			} else {
				playButton.Hide();
				pauseButton.Show();
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

	private void handleMenuPress() {
		GD.PrintS("Open game menu");
		gameView.GameController.GameMenu.ShowMenu();
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
