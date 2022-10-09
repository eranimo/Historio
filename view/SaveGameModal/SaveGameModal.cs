using Godot;
using System;

public class SaveGameModal : Control {
	private Label countryName;
	private LineEdit saveNameInput;
	private Button saveButton;
	private VBoxContainer saveEntryList;

	public string CountryName { get => countryName.Text; set => countryName.Text = value; }
	public string SaveNameInput { get => saveNameInput.Text; set => saveNameInput.Text = value; }

	private PackedScene SaveEntryListItem = ResourceLoader.Load<PackedScene>("res://view/SaveGameModal/SaveEntryListItem.tscn");
	private GameView gameView;
	private SaveManager saveManager;

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		gameView.game.state.AddElement<SaveGameModal>(this);
		saveManager = new SaveManager();

		var closeButton = (TextureButton) GetNode("%CloseButton");
		closeButton.Connect("pressed", this, nameof(handleClose));

		countryName = (Label) GetNode("%CountryName");

		saveNameInput = (LineEdit) GetNode("%SaveNameInput");

		saveButton = (Button) GetNode("%SaveButton");
		saveButton.Connect("pressed", this, nameof(handleSave));

		saveEntryList = (VBoxContainer) GetNode("%SaveEntryList");

	}

	public void OpenModal() {
		Show();

		foreach (var child in saveEntryList.GetChildren()) {
			((Node) child).QueueFree();
		}

		SavedGameMetadata savedGame = gameView.game.savedGame;
		foreach (SavedGameEntryMetadata save in savedGame.saves) {
			var listItem = (SaveEntryListItem) SaveEntryListItem.Instance();
			saveEntryList.AddChild(listItem);
			listItem.SaveEntryName = save.name;
			listItem.SaveDate = save.saveDate.ToString();
			listItem.SaveEntryDelete += () => {
				saveManager.DeleteSave(savedGame, save);
			};
			listItem.SaveEntryOverwrite += () => {
				saveManager.DeleteSave(savedGame, save);
				gameView.game.state.Send(new SaveGameTrigger { entry = save });
			};
		}
		gameView.game.state.Send(new SaveModalLoadTrigger());
	}

	private void handleClose() {
		Hide();
	}

	private void handleSave() {
		gameView.game.state.Send(new SaveModalSaveTrigger {
			name = saveNameInput.Text
		});
	}

	public override void _UnhandledInput(InputEvent @event) {

		if (@event.IsActionPressed("ui_cancel") && Visible) {
			Hide();
			GetTree().SetInputAsHandled();
		}
	}
}
