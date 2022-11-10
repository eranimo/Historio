using Godot;
using System;

public partial class GameMenu : Control {
	private GameView gameView;
	private LoadGameModal loadGameModal;
	private SaveGameModal saveGameModal;

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");

		loadGameModal = (LoadGameModal) GetNode("%LoadGameModal");
		saveGameModal = (SaveGameModal) GetNode("%SaveGameModal");

		var continueButton = (Button) GetNode("%ContinueButton");
		var exitButton = (Button) GetNode("%ExitGameButton");
		var loadGameButton = (Button) GetNode("%LoadGameButton");
		var saveGameButton = (Button) GetNode("%SaveGameButton");
		var mainMenuButton = (Button) GetNode("%MainMenuButton");

		continueButton.Connect("pressed",new Callable(this,nameof(handleContinuePressed)));
		exitButton.Connect("pressed",new Callable(this,nameof(handleExitPressed)));
		loadGameButton.Connect("pressed",new Callable(this,nameof(handleLoadPressed)));
		saveGameButton.Connect("pressed",new Callable(this,nameof(handleSavePressed)));
		mainMenuButton.Connect("pressed",new Callable(this,nameof(handleMainMenu)));
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

	private void handleMainMenu() {
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://scenes/MainMenu/MainMenu.tscn");
	}

	public override void _Process(double delta) {
		base._Process(delta);
		gameView.game.manager.ProcessMenu();
	}
}
