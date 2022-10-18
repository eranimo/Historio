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
			var watch = System.Diagnostics.Stopwatch.StartNew();
			var saveData = new SaveData();
			saveData.Save(this);
			Godot.GD.PrintS($"(SaveSystem) Saved created in {watch.ElapsedMilliseconds}ms");
			saveManager.SaveGame(game, saveData, e.entry);
		}

		foreach (var e in this.Receive<LoadGameTrigger>()) {
			var watch = System.Diagnostics.Stopwatch.StartNew();
			var saveEntry = saveManager.LoadGame(e.savedGame, e.saveEntry);
			game.date.SetDay(saveEntry.metadata.dayTicks);
			saveEntry.saveData.Load(this);
			Godot.GD.PrintS($"(SaveSystem) Load processed in {watch.ElapsedMilliseconds}ms");
			game.HandleGameLoaded(saveEntry);
		}
	}
}