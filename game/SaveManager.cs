using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using MessagePack;

[MessagePackObject]
public class SavedGameMetadata {
	[Key(0)]
	public string name; // directory filename
	[Key(1)]
	public List<SavedGameEntryMetadata> saves = new List<SavedGameEntryMetadata>();
}

[MessagePackObject]
public class SavedGameEntryMetadata {
	[Key(0)]
	public string name; // also filename
	[Key(1)]
	public string saveName;
	[Key(2)]
	public string countryName;
	[Key(3)]
	public int dayTicks;
	[Key(4)]
	public DateTime saveDate;
}

[MessagePackObject]
public class SavedGameEntry {
	[Key(0)]
	public SavedGameEntryMetadata metadata;
	[Key(1)]
	public SaveData saveData;
}

[MessagePackObject]
public class SerializedComponent {
	[Key(0)]
	public Type type;
	[Key(1)]
	public object component;
}

[MessagePackObject]
public class SerializedRelation {
	[Key(0)]
	public Type type;
	[Key(1)]
	public object relation;
	[Key(2)]
	public int entity;
}

[MessagePackObject]
public class SerializedEntity {
	[Key(0)]
	public int id;
	[Key(1)]
	public List<SerializedComponent> components = new List<SerializedComponent>();
	[Key(2)]
	public List<SerializedRelation> relations = new List<SerializedRelation>();
}

[MessagePackObject]
public struct SerializedTile {
	[Key(0)]
	public Hex hex;
	[Key(1)]
	public TileData tileData;
	[Key(2)]
	public Dictionary<int, ViewState> countriesToViewStates;
}

[MessagePackObject]
public struct SerializedWorld {
	[Key(0)]
	public WorldData worldData;
	[Key(1)]
	public List<SerializedTile> tiles;
}

[MessagePackObject]
public class SaveData {
	[Key(0)]
	public Dictionary<int, SerializedEntity> entities = new Dictionary<int, SerializedEntity>();
	[Key(1)]
	public int playerCountryID;
	[Key(2)]
	public SerializedWorld world;

	public void Save(ISystem system) {
		var persisted = system.QueryBuilder<Entity>().Has<Persisted>().Build();

		playerCountryID = system.GetElement<Player>().playerCountry.Identity.Id;

		foreach (var entity in persisted) {
			// Godot.GD.PrintS("Entity", entity);
			var serializedEntity = new SerializedEntity { id = entity.Identity.Id };
			foreach (var (type, obj) in system.GetComponents(entity)) {
				if (type.Type == entity.GetType()) {
					continue;
				}
				if (obj is Godot.Node) {
					throw new Exception("Cannot serialize Godot Nodes");
				} else if (type.IsRelation) {
					// Godot.GD.PrintS("\tRelation:", type.Type, type.Identity.Id);
					serializedEntity.relations.Add(new SerializedRelation {
						type = type.Type,
						relation = obj,
						entity = type.Identity.Id
					});
				} else {
					// Godot.GD.PrintS("\tComponent:", type.Type, obj);
					serializedEntity.components.Add(new SerializedComponent {
						type = type.Type,
						component = obj
					});
				}
			}
			entities[serializedEntity.id] = serializedEntity;
		}

		world = new SerializedWorld {
			worldData = system.GetElement<WorldData>(),
			tiles = new List<SerializedTile>(),
		};
		var tiles = system.Query<Location, TileViewState, TileData>();
		foreach (var (loc, tileViewState, tileData) in tiles) {
			var countriesToViewStates = new Dictionary<int, ViewState>();
			foreach (var (entity, viewState) in tileViewState.countriesToViewStates) {
				countriesToViewStates[entity.Identity.Id] = viewState;
			}
			world.tiles.Add(new SerializedTile {
				hex = loc.hex,
				tileData = tileData,
				countriesToViewStates = countriesToViewStates,
			});
		}
	}

	public void Load(ISystem system) {
		var newEntities = new Dictionary<int, Entity>();

		// remove all entities
		var persisted = system.QueryBuilder<Entity>().Has<Persisted>().Build();
		foreach (var e in persisted) {
			system.Despawn(e);
		}

		// find methods
		var addMethods = typeof(EntityBuilder)
			.GetMethods(BindingFlags.Instance | BindingFlags.Public)
			.Where(x => x.Name == "Add");

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

	
		// spawn components
		foreach (var (entityID, serializedEntity) in entities) {
			newEntities[entityID] = system.Spawn().Id();
		}

		// add components on entities
		foreach (var (entityID, serializedEntity) in entities) {
			var builder = system.On(newEntities[entityID]);
			try {
				foreach (var item in serializedEntity.components) {
					var addMethodG = addMethodComponent.MakeGenericMethod(new Type[] { item.type });
					// TODO: figure out why I need to create a new instance of the object and copy over the properties
					var comp = Activator.CreateInstance(item.type);
					foreach (var prop in comp.GetType().GetProperties()) {
						prop.SetValue(comp, item.component.GetType().GetProperty(prop.Name));
					}
					addMethodG.Invoke(builder, new object[] { comp });
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
			} catch (Exception err) {
				Godot.GD.PrintS($"(SaveManager) Failed to load entity ({entityID})");
				Godot.GD.PrintErr(err);
			}
		}

		// add player
		system.AddElement<Player>(new Player {
			playerCountry = newEntities[playerCountryID],
		});

		// deserialize world
		system.AddElement<WorldData>(world.worldData);
		var worldService = system.GetElement<WorldService>();
		worldService.initWorld(world.worldData.worldSize);
		foreach (var tile in world.tiles) {
			var tileViewState = new TileViewState();
			foreach (var (entityID, viewState) in tile.countriesToViewStates) {
				var country = newEntities[entityID];
				tileViewState.countriesToViewStates[country] = viewState;
			}
			var entity = system.Spawn()
				.Add(new Location { hex = tile.hex })
				.Add(tile.tileData)
				.Add(tileViewState).Id();

			worldService.AddTile(tile.hex, entity);
		}


		// setup after load
		system.GetElement<PathfindingService>().setup();
	}
}

/**
SaveManager represents a save system where each saved game represents multiple saves.
	- each "save game" is stored in a folder under the "saved_games" folder
	- each save game folder has:
		- "save.sav" file, a serialized SavedGame struct
		- "saves" dir, holding files that are serialized SavedGameData structs
*/
public class SaveManager {
	private readonly string SAVE_GAME_FOLDER = "user://saved_games";
	private readonly string SAVE_METADATA_FILENAME = "metadata.dat";


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
					SavedGameMetadata savedGame = MessagePackSerializer.Deserialize<SavedGameMetadata>(stream);
					saves.Add(savedGame);
					stream.Close();
				} catch (FileNotFoundException) {};
			}
		} catch (UnauthorizedAccessException) {
			return saves;
		}

		Godot.GD.PrintS("Saves", saves);

		return saves;
	}

	private void saveSavedGame(SavedGameMetadata savedGame) {
		Directory.CreateDirectory(Godot.ProjectSettings.GlobalizePath(SAVE_GAME_FOLDER));
		Directory.CreateDirectory(Godot.ProjectSettings.GlobalizePath($"{SAVE_GAME_FOLDER}/{savedGame.name}"));
		var path = Godot.ProjectSettings.GlobalizePath($"{SAVE_GAME_FOLDER}/{savedGame.name}/{SAVE_METADATA_FILENAME}");
		var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
		MessagePackSerializer.Serialize(stream, savedGame);
		stream.Close();
	}

	public void SaveGame(Game game, SaveData saveData, SavedGameEntryMetadata saveMetadata) {
		var watch = System.Diagnostics.Stopwatch.StartNew();
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
		MessagePackSerializer.Serialize(stream, saveEntry);

		// JSON debugging
		var p = Godot.ProjectSettings.GlobalizePath($"{SAVE_GAME_FOLDER}/{game.savedGame.name}/saves/{saveMetadata.name}.json");
		var s = new FileStream(p, FileMode.Create, FileAccess.Write, FileShare.None);
		var textWriter = new StreamWriter(s);
		MessagePackSerializer.SerializeToJson(textWriter, saveEntry);

		Godot.GD.PrintS($"(SaveService) Saved game save at {saveEntryPath} in {watch.ElapsedMilliseconds}ms");
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
		var watch = System.Diagnostics.Stopwatch.StartNew();
		var saveFilePath = Godot.ProjectSettings.GlobalizePath($"{SAVE_GAME_FOLDER}/{savedGame.name}/saves/{saveMetadata.name}.sav");
		var stream = new FileStream(saveFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		SavedGameEntry saveGameEntry = MessagePackSerializer.Deserialize<SavedGameEntry>(stream);
		stream.Close();
		Godot.GD.PrintS($"(SaveService) Loading game save at {saveFilePath} in {watch.ElapsedMilliseconds}ms");
		return saveGameEntry;
	}
}
