class Persisted {}


// trigger for saving a game
public class SaveGameTrigger {
	public SavedGameEntryMetadata entry;
}

public class SaveModalLoadTrigger {};
public class SaveModalSaveTrigger {
	public string name;
}

public class LoadGameTrigger {
	public SavedGame saveGame;
	public SavedGameEntryMetadata entry;
}