using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class SavedGameMetadata {
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
	public object relation;
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
		var persisted = system.QueryBuilder<Entity>().Has<Persisted>().Build();

		foreach (var entity in persisted) {
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
						relation = obj,
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
		var newEntities = new Dictionary<int, Entity>();

		// remove all entities
		var persisted = system.QueryBuilder<Entity>().Has<Persisted>().Build();
		foreach (var e in persisted) {
			system.Despawn(e);
		}

		var addMethods = typeof(EntityBuilder)
			.GetMethods(BindingFlags.Instance | BindingFlags.Public)
			.Where(x => x.Name == "Add");
		foreach (var method in addMethods) {
			Godot.GD.PrintS("method", method.ToString());
			Godot.GD.PrintS("\t generic args", String.Join(",", method.GetParameters().Select(x => x.ToString())));
			Godot.GD.PrintS("\t generic args", String.Join(",", method.GetParameters().Select(x => x.ParameterType.IsGenericParameter)));
		}
		var addMethodRelation = addMethods
			.Single(x => x.IsGenericMethodDefinition
				&& x.GetParameters().Length == 1
				&& x.GetParameters()[0].ParameterType == typeof(Entity)
			);
		var addMethodRelationWithData = addMethods
			.Single(x => x.IsGenericMethodDefinition
				&& x.GetParameters().Length == 2
				&& x.GetParameters()[1].ParameterType == typeof(Entity)
			);
		var addMethodComponent = addMethods
			.Single(x => x.IsGenericMethodDefinition
				&& x.GetParameters().Length == 1
				&& x.GetParameters()[0].ParameterType.IsGenericParameter
			);

	
		foreach (var (entityID, serializedEntity) in entities) {
			newEntities[entityID] = system.Spawn().Id();
		}

		foreach (var (entityID, serializedEntity) in entities) {
			var builder = system.On(newEntities[entityID]);
			foreach (var item in serializedEntity.components) {
				var addMethodG = addMethodComponent.MakeGenericMethod(new Type[] { item.type });
				addMethodG.Invoke(builder, new object[] { item.component });
			}
			foreach (var item in serializedEntity.relations) {
				var entity = newEntities[item.entity];
				if (item.relation is null) {
					var addMethodG = addMethodRelation.MakeGenericMethod(new Type[] { item.type });
					addMethodG.Invoke(builder, new object[] { entity });
				} else {
					var addMethodG = addMethodRelationWithData.MakeGenericMethod(new Type[] { item.type });
					addMethodG.Invoke(builder, new object[] { item.relation, entity });
				}
			}
		}
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
	private readonly string SAVE_METADATA_FILENAME = "metadata.dat";

	public SaveService(GameManager manager) {
		this.manager = manager;
		formatter = new BinaryFormatter();
	}

	public List<SavedGameMetadata> GetSaves() {
		var saves = new List<SavedGameMetadata>();
		var saveDirectories = new List<string>();

		Directory.CreateDirectory(Godot.ProjectSettings.GlobalizePath(SAVE_GAME_FOLDER));

		var savedGameFolder = Godot.ProjectSettings.GlobalizePath(SAVE_GAME_FOLDER);
		try {
			foreach (string name in Directory.GetDirectories(savedGameFolder)) {
				try {
					var path = Path.Combine(name, SAVE_METADATA_FILENAME);
					var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
					SavedGameMetadata savedGame = (SavedGameMetadata) formatter.Deserialize(stream);
					saves.Add(savedGame);
					stream.Close();
				} catch (FileNotFoundException) {};
			}
		} catch (UnauthorizedAccessException) {
			return saves;
		}

		return saves;
	}

	private void saveSavedGame(SavedGameMetadata savedGame) {
		Directory.CreateDirectory(Godot.ProjectSettings.GlobalizePath(SAVE_GAME_FOLDER));
		Directory.CreateDirectory(Godot.ProjectSettings.GlobalizePath($"{SAVE_GAME_FOLDER}/{savedGame.name}"));
		var path = Godot.ProjectSettings.GlobalizePath($"{SAVE_GAME_FOLDER}/{savedGame.name}/{SAVE_METADATA_FILENAME}");
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
		Godot.GD.PrintS("(SaveService) Saved game save at", saveEntryPath);
		stream.Close();
	}

	public void DeleteSavedGame(SavedGameMetadata savedGame) {
		// delete save
		var saveFilePath = Godot.ProjectSettings.GlobalizePath($"{SAVE_GAME_FOLDER}/{savedGame.name}");
		Godot.GD.PrintS("(SaveService) Deleted game at", saveFilePath);
		Directory.Delete(saveFilePath, true);
	}

	public void DeleteSave(SavedGameMetadata savedGame, SavedGameEntryMetadata saveMetadata) {
		// delete metadata
		savedGame.saves.Remove(saveMetadata);
		saveSavedGame(savedGame);

		// delete save file
		var saveFilePath = Godot.ProjectSettings.GlobalizePath($"{SAVE_GAME_FOLDER}/{savedGame.name}/saves/{saveMetadata.name}.sav");
		Godot.GD.PrintS("(SaveService) Deleted game save at", saveFilePath);
		File.Delete(saveFilePath);
	}

	public SavedGameEntry LoadGame(SavedGameMetadata savedGame, SavedGameEntryMetadata saveMetadata) {
		var saveFilePath = Godot.ProjectSettings.GlobalizePath($"{SAVE_GAME_FOLDER}/{savedGame.name}/saves/{saveMetadata.name}.sav");
		Godot.GD.PrintS("(SaveService) Loading game save at", saveFilePath);
		var stream = new FileStream(saveFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		SavedGameEntry saveGameEntry = (SavedGameEntry) formatter.Deserialize(stream);
		stream.Close();
		return saveGameEntry;
	}
}