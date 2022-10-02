using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class SavedGame {
	public string name; // directory filename
	public List<SavedGameEntryMetadata> saves = new List<SavedGameEntryMetadata>();
}

[Serializable]
public class SavedGameEntryMetadata {
	public string name; // also filename
	public string saveName;
	public string countryName;
	public int dayTicks;
	public DateTime saveDate;
}

[Serializable]
public class SavedGameEntry {
	public SavedGameEntryMetadata metadata;
	public SaveData saveData;
}

[Serializable]
public class SerializedComponent {
	public Type type;
	public object component;
}

[Serializable]
public class SerializedRelation {
	public Type type;
	public int entity;
}

[Serializable]
public class SerializedEntity {
	public int id;
	public List<SerializedComponent> components = new List<SerializedComponent>();
	public List<SerializedRelation> relations = new List<SerializedRelation>();
}

[Serializable]
public class SaveData {
	public Dictionary<int, SerializedEntity> entities = new Dictionary<int, SerializedEntity>();

	public void Save(ISystem system) {
		var unitQuery = system.QueryBuilder<Entity>().Has<Persisted>().Build();

		foreach (var entity in unitQuery) {
			Godot.GD.PrintS(entity);
			var serializedEntity = new SerializedEntity { id = entity.Identity.Id };
			foreach (var (type, obj) in system.GetComponents(entity)) {
				if (type.Type == entity.GetType()) {
					continue;
				}
				if (obj is Godot.Node) {
					throw new Exception("Cannot serialize Godot Nodes");
				} else if (type.IsRelation) {
					Godot.GD.PrintS("\tRelation:", type.Type, type.Identity.Id);
					serializedEntity.relations.Add(new SerializedRelation {
						type = type.Type,
						entity = type.Identity.Id
					});
				} else {
					Godot.GD.PrintS("\tComponent:", type.Type, obj);
					serializedEntity.components.Add(new SerializedComponent {
						type = type.Type,
						component = obj
					});
				}
			}
			entities[serializedEntity.id] = serializedEntity;
		}
	}

	public void Load(ISystem system) {
		
	}
}

/**
SaveSystem represents a save system where each saved game represents multiple saves.
	- each "save game" is stored in a folder under the "saved_games" folder
	- each save game folder has:
		- "save.sav" file, a serialized SavedGame struct
		- "saves" dir, holding files that are serialized SavedGameData structs
*/
public class SaveService {
	private GameManager manager;
	private BinaryFormatter formatter;
	private readonly string SAVE_GAME_FOLDER = "user://saved_games";

	public SaveService(GameManager manager) {
		this.manager = manager;
		formatter = new BinaryFormatter();
	}

	public List<SavedGame> GetSaves() {
		var saves = new List<SavedGame>();
		var saveDirectories = new List<string>();

		var savedGameFolder = Godot.ProjectSettings.GlobalizePath(SAVE_GAME_FOLDER);
		try {
			foreach (string name in Directory.GetDirectories(savedGameFolder)) {
				var path = Godot.ProjectSettings.GlobalizePath($"{SAVE_GAME_FOLDER}/{name}/save.dat");
				var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
				SavedGame savedGame = (SavedGame) formatter.Deserialize(stream);
				saves.Add(savedGame);
				stream.Close();
			}
		} catch (UnauthorizedAccessException) {
			return saves;
		}

		return saves;
	}

	private void saveSavedGame(SavedGame savedGame) {
		Directory.CreateDirectory(Godot.ProjectSettings.GlobalizePath(SAVE_GAME_FOLDER));
		Directory.CreateDirectory(Godot.ProjectSettings.GlobalizePath($"{SAVE_GAME_FOLDER}/{savedGame.name}"));
		var path = Godot.ProjectSettings.GlobalizePath($"{SAVE_GAME_FOLDER}/{savedGame.name}/save.sav");
		var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
		formatter.Serialize(stream, savedGame);
		stream.Close();
	}

	public void SaveGame(Game game, SaveData saveData, SavedGameEntryMetadata saveMetadata) {
		// add metadata for new save
		var saveDate = DateTime.Now;
		game.savedGame.saves.Add(saveMetadata);

		// serialize SavedGame with entry metadata
		saveSavedGame(game.savedGame);

		// create saves folder if not exists
		Directory.CreateDirectory(Godot.ProjectSettings.GlobalizePath($"{SAVE_GAME_FOLDER}/{game.savedGame.name}/saves"));

		// serialize save entry
		var saveEntryPath = Godot.ProjectSettings.GlobalizePath($"{SAVE_GAME_FOLDER}/{game.savedGame.name}/saves/{saveMetadata.name}.sav");

		SavedGameEntry saveEntry = new SavedGameEntry {
			metadata = saveMetadata,
			saveData = saveData
		};
		
		var stream = new FileStream(saveEntryPath, FileMode.Create, FileAccess.Write, FileShare.None);
		formatter.Serialize(stream, saveEntry);
		Godot.GD.PrintS("(SaveService) Saved game file to", saveEntryPath);
		stream.Close();
	}

	public void DeleteSaveGame(SavedGame savedGame, SavedGameEntryMetadata saveMetadata) {
		// delete metadata
		savedGame.saves.Remove(saveMetadata);
		saveSavedGame(savedGame);

		// delete save file
		var saveFilePath = Godot.ProjectSettings.GlobalizePath($"{SAVE_GAME_FOLDER}/{savedGame.name}/saves/{saveMetadata.name}.sav");
		Godot.GD.PrintS("(SaveService) Deleted game file at", saveFilePath);
		File.Delete(saveFilePath);
	}

	public SavedGameEntry LoadGame(SavedGame savedGame, SavedGameEntryMetadata saveMetadata) {
		var saveFilePath = Godot.ProjectSettings.GlobalizePath($"{SAVE_GAME_FOLDER}/{savedGame.name}/saves/{saveMetadata.name}.sav");
		Godot.GD.PrintS("(SaveService) Loading game file from", saveFilePath);
		var stream = new FileStream(saveFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		SavedGameEntry saveGameEntry = (SavedGameEntry) formatter.Deserialize(stream);
		stream.Close();
		return saveGameEntry;
	}
}