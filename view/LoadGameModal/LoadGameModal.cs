using Godot;
using System.Linq;

public class LoadGameModal : Control {
	private SaveManager saveManager;
	private VBoxContainer saveList;
	private VBoxContainer saveEntryList;
	private Label noSavedGamesText;
	private VBoxContainer selectedSave;
	private VBoxContainer noSelectedSave;
	private Label noSaveEntriesText;
	private Label selectedSaveCountryName;
	private PackedScene LoadSaveEntryListItem = ResourceLoader.Load<PackedScene>("res://view/LoadGameModal/LoadSaveEntryListItem.tscn");
	private PackedScene LoadSaveListItem = ResourceLoader.Load<PackedScene>("res://view/LoadGameModal/LoadSaveListItem.tscn");
	private SavedGameMetadata currentSavedGame;

	public override void _Ready() {
		saveManager = new SaveManager();

		var closeButton = (TextureButton) GetNode("%CloseButton");
		closeButton.Connect("pressed", this, nameof(handleClose));

		saveList = (VBoxContainer) GetNode("%SaveList");
		saveEntryList = (VBoxContainer) GetNode("%SaveEntryList");
		noSavedGamesText = (Label) GetNode("%NoSavedGamesText");
		selectedSave = (VBoxContainer) GetNode("%SelectedSave");
		noSelectedSave = (VBoxContainer) GetNode("%NoSelectedSave");
		noSaveEntriesText = (Label) GetNode("%NoSaveEntriesText");
		selectedSaveCountryName = (Label) GetNode("%SelectedSaveCountryName");
	}

	private void handleClose() {
		Hide();
	}

	public void OpenModal() {
		Show();
		update();
	}

	private void update() {
		var savedGames = saveManager.GetSaves();

		foreach (var child in saveList.GetChildren()) {
			((Node) child).QueueFree();
		}

		noSavedGamesText.Visible = savedGames.Count == 0;
		noSelectedSave.Visible = savedGames.Count > 0;

		foreach (SavedGameMetadata savedGame in savedGames) {
			var latestSave = savedGame.saves.OrderByDescending(i => i.saveDate).First();
			var listItem = (LoadSaveListItem) LoadSaveListItem.Instance();
			GD.PrintS(listItem);
			saveList.AddChild(listItem);
			listItem.CountryName = latestSave.countryName;
			listItem.LastSaveDate = latestSave.saveDate.ToString();
			listItem.OnSelect += () => selectSave(savedGame);
			listItem.OnDelete += () => deleteSave(savedGame);
			listItem.OnLoadLatest += () => loadSave(savedGame, latestSave);
		}
	}

	private void selectSave(SavedGameMetadata savedGame) {
		currentSavedGame = savedGame;
		noSavedGamesText.Hide();
		noSelectedSave.Hide();
		selectedSave.Show();

		noSaveEntriesText.Visible = savedGame.saves.Count == 0;
		selectedSaveCountryName.Visible = savedGame.saves.Count > 0;
		var latestSave = savedGame.saves.OrderByDescending(i => i.saveDate).First();

		if (!(latestSave is null)) {
			selectedSaveCountryName.Text = latestSave.countryName;
		}

		foreach (var child in saveEntryList.GetChildren()) {
			((Node) child).QueueFree();
		}

		foreach (var entry in savedGame.saves) {
			var listItem = (LoadSaveEntryListItem) LoadSaveEntryListItem.Instance();
			saveEntryList.AddChild(listItem);
			listItem.SaveEntryName = entry.saveName;
			listItem.SaveDate = entry.saveDate.ToString();
			listItem.OnLoad += () => loadSave(savedGame, entry);
			listItem.OnDelete += () => deleteSaveEntry(savedGame, entry);
		}
	}

	private void deleteSave(SavedGameMetadata savedGame) {
		saveManager.DeleteSavedGame(savedGame);
		update();
	}

	private void deleteSaveEntry(SavedGameMetadata savedGame, SavedGameEntryMetadata entry) {
		saveManager.DeleteSave(savedGame, entry);
		update();
	}

	private void loadSave(SavedGameMetadata savedGame, SavedGameEntryMetadata saveEntry) {
		var loadState = (LoadState) GetTree().Root.GetNode("LoadState");
		loadState.savedGame = savedGame;
		loadState.saveEntry = saveEntry;
		GetTree().Paused = false;
		if (GetTree().CurrentScene.Name == "GameView") { 
			GetTree().ReloadCurrentScene();
		} else {
			GetTree().ChangeScene("res://scenes/GameView/GameView.tscn");
		}
	}

	public override void _UnhandledInput(InputEvent @event) {

		if (@event.IsActionPressed("ui_cancel") && Visible) {
			Hide();
			GetTree().SetInputAsHandled();
		}
	}
}
