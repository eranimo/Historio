using Godot;
using System;

public class LoadGameModal : Control {
	private GameView gameView;
	private VBoxContainer saveList;

	private PackedScene LoadSaveEntryListItem = ResourceLoader.Load<PackedScene>("res://view/LoadGameModal/LoadSaveEntryListItem.tscn");
	private PackedScene LoadSaveListItem = ResourceLoader.Load<PackedScene>("res://view/LoadGameModal/LoadSaveListItem.tscn");

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");

		var closeButton = (TextureButton) GetNode("%CloseButton");

		closeButton.Connect("pressed", this, nameof(handleClose));

		saveList = (VBoxContainer) GetNode("%SaveList");
	}

	private void handleClose() {
		Hide();
	}

	public void OpenModal() {
		Show();

		var saveService = gameView.game.state.GetElement<SaveService>();
		var savedGames = saveService.GetSaves();

		foreach (var child in saveList.GetChildren()) {
			((Node) child).QueueFree();
		}

		foreach (SavedGame savedGame in savedGames) {
			var listItem = (LoadSaveListItem) LoadSaveListItem.Instance();

			saveList.AddChild(listItem);
		}
	}

	public override void _UnhandledInput(InputEvent @event) {

		if (@event.IsActionPressed("ui_cancel") && Visible) {
			Hide();
			GetTree().SetInputAsHandled();
		}
	}
}
