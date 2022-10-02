using Godot;
using System;

public class GameMenu : Control {
	private GameView gameView;
	private Button continueButton;
	private Button exitButton;
	private Button loadGameButton;
	private Button saveGameButton;
	private LoadGameModal loadGameModal;
	private SaveGameModal saveGameModal;

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");

		continueButton = (Button) GetNode("%ContinueButton");
		exitButton = (Button) GetNode("%ExitGameButton");
		loadGameButton = (Button) GetNode("%LoadGameButton");
		saveGameButton = (Button) GetNode("%SaveGameButton");

		loadGameModal = (LoadGameModal) GetNode("%LoadGameModal");
		saveGameModal = (SaveGameModal) GetNode("%SaveGameModal");

		continueButton.Connect("pressed", this, nameof(handleContinuePressed));
		exitButton.Connect("pressed", this, nameof(handleExitPressed));
		loadGameButton.Connect("pressed", this, nameof(handleLoadPressed));
		saveGameButton.Connect("pressed", this, nameof(handleSavePressed));
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

	private void handleLoadPressed() {
		loadGameModal.OpenModal();
	}

	private void handleSavePressed() {
		saveGameModal.OpenModal();
	}

	public override void _Process(float delta) {
		base._Process(delta);
		gameView.game.manager.ProcessMenu();
	}
}
