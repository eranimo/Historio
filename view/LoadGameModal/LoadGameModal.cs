using Godot;
using System;

public class LoadGameModal : Control {
	private GameView gameView;

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");

		var closeButton = (TextureButton) GetNode("%CloseButton");

		closeButton.Connect("pressed", this, nameof(handleClose));
	}

	private void handleClose() {
		Hide();
	}

	public void OpenModal() {
		Show();

		var saveService = gameView.game.state.GetElement<SaveService>();
		var savedGames = saveService.GetSaves();

		foreach (var child in saveEntryList.GetChildren()) {
			((Node) child).QueueFree();
		}

		foreach (SavedGame savedGame in savedGames) {
			var listItem = (SaveEntryListItem) SaveEntryListItem.Instance();

			saveEntryList.AddChild(listItem);
		}
	}

	public override void _UnhandledInput(InputEvent @event) {

		if (@event.IsActionPressed("ui_cancel") && Visible) {
			Hide();
			GetTree().SetInputAsHandled();
		}
	}
}
