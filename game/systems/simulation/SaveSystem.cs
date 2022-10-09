public class SaveSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		if (!this.HasElement<Game>()) {
			return;
		}
		var game = this.GetElement<Game>();
		var saveManager = this.GetElement<SaveManager>();
		var gameManager = this.GetElement<GameManager>();

		foreach (var e in this.Receive<SaveGameTrigger>()) {
			var saveData = new SaveData();
			saveData.Save(this);
			saveManager.SaveGame(game, saveData, e.entry);
		}

		foreach (var e in this.Receive<LoadGameTrigger>()) {
			var saveEntry = saveManager.LoadGame(e.savedGame, e.saveEntry);
			game.date.SetDay(saveEntry.metadata.dayTicks);
			saveEntry.saveData.Load(this);
			game.HandleGameLoaded();
		}
	}
}