public partial class SaveSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		if (!World.HasElement<Game>()) {
			return;
		}
		var game = World.GetElement<Game>();
		var saveManager = World.GetElement<SaveManager>();
		var gameManager = World.GetElement<GameManager>();

		foreach (var e in World.Receive<SaveGameTrigger>(this)) {
			var watch = System.Diagnostics.Stopwatch.StartNew();
			var saveData = new SaveData();
			saveData.Save(this);
			Godot.GD.PrintS($"(SaveSystem) Saved created in {watch.ElapsedMilliseconds}ms");
			saveManager.SaveGame(game, saveData, e.entry);
		}

		foreach (var e in World.Receive<LoadGameTrigger>(this)) {
			var watch = System.Diagnostics.Stopwatch.StartNew();
			Godot.GD.PrintS($"(SaveSystem) Starting game load");
			var saveEntry = saveManager.LoadGame(e.savedGame, e.saveEntry);
			game.date.SetDay(saveEntry.metadata.dayTicks);
			saveEntry.saveData.Load(this);
			Godot.GD.PrintS($"(SaveSystem) Load processed in {watch.ElapsedMilliseconds}ms");
			game.HandleGameLoaded(saveEntry);
		}
	}
}