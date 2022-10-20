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

[MessagePack.Union(0, typeof(SerializedWorld))]
[MessagePack.Union(1, typeof(SerializedCountries))]
public abstract class SerializedState {
	public abstract void Save(ISystem system);
	public abstract void Load(ISystem system);
}

[MessagePackObject]
public class SerializedWorld : SerializedState {
	[MessagePackObject]
	public struct SerializedTile {
		[Key(0)]
		public Hex hex;

		[Key(1)]
		public TileData tileData;
	}

	[Key(0)]
	public WorldData worldData;

	[Key(1)]
	public List<SerializedTile> tiles = new List<SerializedTile>();

	public override void Save(ISystem system) {
		worldData = system.GetElement<WorldData>();
		var query = system.Query<Location, TileData>();
		foreach (var (loc, tileData) in query) {
			tiles.Add(new SerializedTile {
				hex = loc.hex,
				tileData = tileData,
			});
		}
	}

	public override void Load(ISystem system) {
		system.AddElement<WorldData>(worldData);
		var worldService = system.GetElement<WorldService>();
		worldService.initWorld(worldData.worldSize);
		foreach (var tile in tiles) {
			var entity = system.Spawn()
				.Add(new Location { hex = tile.hex })
				.Add(tile.tileData)
				.Id();

			worldService.AddTile(tile.hex, entity);
		}

		system.GetElement<PathfindingService>().setup();
	}
}

[MessagePackObject]
public class SerializedCountries : SerializedState {
	[MessagePackObject]
	public class SerializedCountry {
		[Key(0)]
		public CountryData countryData;
	}

	[MessagePackObject]
	public class SerializedCountryTile {
		[Key(0)]
		public int ownerCountry;

		[Key(1)]
		public ViewStateNode viewStateNode;

		[Key(2)]
		public Location location;

		[Key(3)]
		public int ownerSettlement;
	}

	[MessagePackObject]
	public class SerializedSettlement {
		[Key(0)]
		public int ownerCountry;

		[Key(1)]
		public SettlementData settlementData;

		[Key(2)]
		public bool isCapital;
	}

	[Key(0)]
	public int playerCountry;

	[Key(1)]
	public Dictionary<int, SerializedCountry> countries = new Dictionary<int, SerializedCountry>();

	[Key(2)]
	public Dictionary<int, SerializedSettlement> settlements = new Dictionary<int, SerializedSettlement>();
	
	[Key(3)]
	public Dictionary<int, SerializedCountryTile> countryTiles = new Dictionary<int, SerializedCountryTile>();

	

	public override void Save(ISystem system) {
		countries.Clear();
		var player = system.GetElement<Player>();
		foreach (var (country, countryData) in system.Query<Entity, CountryData>()) {
			// serialize Countries
			countries[country.Identity.Id] = new SerializedCountry {
				countryData = countryData,
			};

			// serialize Settlements
			var settlementQuery = system
				.QueryBuilder<Entity, SettlementData>()
				.Has<SettlementOwner>(country)
				.Build();

			foreach (var (settlement, settlementData) in settlementQuery) {
				settlements[settlement.Identity.Id] = new SerializedSettlement {
					ownerCountry = country.Identity.Id,
					settlementData = settlementData,
					isCapital = system.HasComponent<CapitalSettlement>(settlement),
				};
			}
			
			// serialize Settlement Tiles
			var tilesQuery = system
				.QueryBuilder<Entity, ViewStateNode, Location>()
				.Has<CountryTile>(country)
				.Build();

			foreach (var (countryTile, viewStateNode, location) in tilesQuery) {
				countryTiles[countryTile.Identity.Id] = new SerializedCountryTile {
					ownerCountry = country.Identity.Id,
					viewStateNode = viewStateNode,
					location = location,
					ownerSettlement = system.GetTarget<CountryTileSettlement>(countryTile).Identity.Id,
				};
			}

		}
		playerCountry = player.playerCountry.Identity.Id;
	}

	public override void Load(ISystem system) {
		var world = system.GetElement<WorldService>();
		var countries = new Dictionary<int, Entity>();
		var settlements = new Dictionary<int, Entity>();
		foreach (var (countryID, country) in this.countries) {
			var entity = system.Spawn()
				.Add(country.countryData)
				.Id();
			countries[countryID] = entity;

			system.Send(new CountryAdded { country = entity });
		}

		system.AddElement<Player>(new Player {
			playerCountry = countries[playerCountry],
		});

		foreach (var (settlementID, settlement) in this.settlements) {
			var country = countries[settlement.ownerCountry];
			var entity = system.Spawn()
				.Add(settlement.settlementData)
				.Add<SettlementOwner>(country);
			if (settlement.isCapital) {
				entity.Add<CapitalSettlement>();
			}
			settlements[settlementID] = entity.Id();
		}
		
		foreach (var (countryTileID, countryTile) in this.countryTiles) {
			var country = countries[countryTile.ownerCountry];
			var settlement = settlements[countryTile.ownerSettlement];
			var tile = world.GetTile(countryTile.location.hex);
			var entity = system.Spawn()
				.Add(countryTile.location)
				.Add(countryTile.viewStateNode)
				.Add<CountryTile>(country)
				.Add<ViewStateOwner>(country)
				.Add<CountryTileSettlement>(settlement)
				.Id();

			system.Send(new SettlementBorderUpdated {
				settlement = settlement,
				countryTile = entity,
			});
			system.Send(new ViewStateNodeUpdated { entity = entity });
		}
	}
}

[MessagePackObject]
public class SaveData {
	[Key(0)]
	public List<SerializedState> state = new List<SerializedState> {
		new SerializedWorld(),
		new SerializedCountries(),
	};

	public void Save(ISystem system) {
		foreach (var serializedState in state) {
			serializedState.Save(system);
		}
	}

	public void Load(ISystem system) {
		foreach (var serializedState in state) {
			serializedState.Load(system);
		}
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
		// var p = Godot.ProjectSettings.GlobalizePath($"{SAVE_GAME_FOLDER}/{game.savedGame.name}/saves/{saveMetadata.name}.json");
		// var s = new FileStream(p, FileMode.Create, FileAccess.Write, FileShare.None);
		// var textWriter = new StreamWriter(s);
		// MessagePackSerializer.SerializeToJson(textWriter, saveEntry);

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
