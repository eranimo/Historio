class Persisted {}


// trigger for saving a game
public partial class SaveGameTrigger {
	public SavedGameEntryMetadata entry;
}

public partial class SaveModalLoadTrigger {};
public partial class SaveModalSaveTrigger {
	public string name;
}

public partial class LoadGameTrigger {
	public SavedGameMetadata savedGame;
	public SavedGameEntryMetadata saveEntry;
}