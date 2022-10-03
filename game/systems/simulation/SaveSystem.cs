public class SaveSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		if (!this.HasElement<Game>()) {
			return;
		}
		var game = this.GetElement<Game>();
		var saveService = this.GetElement<SaveService>();
		var gameManager = this.GetElement<GameManager>();

		foreach (var e in this.Receive<SaveGameTrigger>()) {
			var saveData = new SaveData();
			saveData.Save(this);
			saveService.SaveGame(game, saveData, e.entry);
		}

		foreach (var e in this.Receive<LoadGameTrigger>()) {
			var saveEntry = saveService.LoadGame(e.savedGame, e.saveEntry);
			game.date.SetDay(saveEntry.metadata.dayTicks);
			// gameManager.Reset();
			saveEntry.saveData.Load(this);
		}
	}
}