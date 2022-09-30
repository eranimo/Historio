using Godot;
using System;

public class GameMenu : Control {
	private Button continueButton;
	private Button exitButton;

	public override void _Ready() {
		continueButton = (Button) GetNode("%ContinueButton");
		exitButton = (Button) GetNode("%ExitGameButton");

		continueButton.Connect("pressed", this, nameof(handleContinuePressed));
		exitButton.Connect("pressed", this, nameof(handleExitPressed));
	}

	public void ShowMenu() {
		Show();
		GetTree().Paused = true;
	}

	public void HideMenu() {
		Hide();
		GetTree().Paused = false;
	}

	public override void _UnhandledInput(InputEvent @event) {
		base._UnhandledInput(@event);

		if (@event.IsActionPressed("ui_cancel")) {
			if (Visible) {
				HideMenu();
			} else {
				ShowMenu();
			}
		}
	}

	private void handleContinuePressed() {
		HideMenu();
	}

	private void handleExitPressed() {
		GetTree().Quit();
	}
}
