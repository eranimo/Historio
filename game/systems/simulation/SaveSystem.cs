public class SaveSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		var game = this.GetElement<Game>();
		var saveService = this.GetElement<SaveService>();

		foreach (var e in this.Receive<SaveGameTrigger>()) {
			var saveData = new SaveData();
			saveData.Save(this);
			saveService.SaveGame(game, saveData, e.entry);
		}

		foreach (var e in this.Receive<LoadGameTrigger>()) {
			var saveEntry = saveService.LoadGame(e.saveGame, e.entry);
			game.date.SetDay(saveEntry.metadata.dayTicks);
			saveEntry.saveData.Load(this);
		}
	}
}