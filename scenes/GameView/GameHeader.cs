using Godot;
using System;

public class GameHeader : PanelContainer {
	[Export]
	private NodePath playButtonPath;
	private Button playButton;


	[Export]
	private NodePath dateDisplayPath;
	private Label dateDisplay;

	private GameView gameView;

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		playButton = GetNode<Button>(playButtonPath);
		dateDisplay = GetNode<Label>(dateDisplayPath);

		dateDisplay.Text = gameView.game.date.ToString();
		gameView.game.GameDateChanged.Subscribe((GameDate date) => {
			dateDisplay.Text = date.ToString();
		});

		playButton.Connect("pressed", this, nameof(handlePlayPressed));

		gameView.game.PlayState.Subscribe((bool isPlaying) => {
			GD.PrintS("isPlaying", isPlaying);
			if (isPlaying) {
				playButton.Text = "Pause";
			} else {
				playButton.Text = "Play";
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
}
