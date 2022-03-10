using Godot;
using System;

public class GameHeader : PanelContainer {
	[Export]
	private NodePath playButtonPath;
	private Button playButton;


	[Export]
	private NodePath dateDisplayPath;
	private Label dateDisplay;

	private GameContext gameContext;

	public override void _Ready() {
		gameContext = (GameContext) GetTree().Root.GetNode("GameContext");
		playButton = GetNode<Button>(playButtonPath);
		dateDisplay = GetNode<Label>(dateDisplayPath);

		playButton.Connect("pressed", this, nameof(handlePlayPressed));

		gameContext.Game.PlayState.Subscribe((bool isPlaying) => {
			GD.PrintS("isPlaying", isPlaying);
			if (isPlaying) {
				playButton.Text = "Pause";
			} else {
				playButton.Text = "Play";
			}
		});
	}

	private void handlePlayPressed() {
		if (gameContext.Game.IsPlaying) {
			gameContext.Game.Pause();
		} else {
			gameContext.Game.Play();
		}
		
	}
}
