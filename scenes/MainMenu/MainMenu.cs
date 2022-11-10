using Godot;
using System;

public partial class MainMenu : Control {
	private LoadGameModal loadGameModal;
	private SaveManager saveManager;

	public override void _Ready(){
		saveManager = new SaveManager();

		var NewGameButton = (Button) GetNode("%NewGameButton");
		var LoadGameButton = (Button) GetNode("%LoadGameButton");
		var SettingsButton = (Button) GetNode("%SettingsButton");
		var ExitButton = (Button) GetNode("%ExitButton");

		loadGameModal = (LoadGameModal) GetNode("%LoadGameModal");

		NewGameButton.Connect("pressed",new Callable(this,nameof(handleNewGame)));
		LoadGameButton.Connect("pressed",new Callable(this,nameof(handleLoadGame)));
		SettingsButton.Connect("pressed",new Callable(this,nameof(handleSettings)));
		ExitButton.Connect("pressed",new Callable(this,nameof(handleExit)));
	}

	private void handleNewGame() {
		GetTree().ChangeSceneToFile("res://scenes/GameView/GameView.tscn");
	}

	private void handleLoadGame() {
		loadGameModal.OpenModal();
	}

	private void handleSettings() {

	}

	private void handleExit() {
		GetTree().Quit();
	}
}
