using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using MessagePack;

[MessagePackObject]
public partial class SavedGameMetadata {
	[Key(0)]
	public string name; // directory filename
	[Key(1)]
	public List<SavedGameEntryMetadata> saves = new List<SavedGameEntryMetadata>();
}

[MessagePackObject]
public partial class SavedGameEntryMetadata {
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
public partial class SavedGameEntry {
	[Key(0)]
	public SavedGameEntryMetadata metadata;
	[Key(1)]
	public SaveData saveData;
}

[MessagePack.Union(0, typeof(SerializedWorld))]
[MessagePack.Union(1, typeof(SerializedCountries))]
[MessagePack.Union(2, typeof(SerializedUnits))]
public abstract class SerializedState {
	public abstract void Save(ISystem system);
	public abstract void Load(ISystem system, ref LoadData loadData);
}

[MessagePackObject]
public partial class SerializedWorld : SerializedState {
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
		worldData = system.World.GetElement<WorldData>();
		var query = system.World.Query<Location, TileData>().Build();
		foreach (var (loc, tileData) in query) {
			tiles.Add(new SerializedTile {
				hex = loc.hex,
				tileData = tileData,
			});
		}
	}

	public override void Load(ISystem system, ref LoadData loadData) {
		system.World.AddElement<WorldData>(worldData);
		var worldService = system.World.GetElement<WorldService>();
		worldService.initWorld(worldData.worldSize);
		foreach (var tile in tiles) {
			var entity = system.World.Spawn()
				.Add(new Location { hex = tile.hex })
				.Add(tile.tileData)
				.Id();

			worldService.AddTile(tile.hex, entity);
		}

		system.World.GetElement<PathfindingService>().setup();
	}
}

[MessagePackObject]
public partial class SerializedCountries : SerializedState {
	[MessagePackObject]
	public partial class SerializedCountry {
		[Key(0)]
		public CountryData countryData;
	}

	[MessagePackObject]
	public partial class SerializedCountryTile {
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
	public partial class SerializedSettlement {
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
		var player = system.World.GetElement<Player>();
		foreach (var (country, countryData) in system.World.Query<Entity, CountryData>().Build()) {
			// serialize Countries
			countries[country.Identity.Id] = new SerializedCountry {
				countryData = countryData,
			};

			// serialize Settlements
			var settlementQuery = system.World
				.Query<Entity, SettlementData>()
				.Has<SettlementOwner>(country)
				.Build();

			foreach (var (settlement, settlementData) in settlementQuery) {
				settlements[settlement.Identity.Id] = new SerializedSettlement {
					ownerCountry = country.Identity.Id,
					settlementData = settlementData,
					isCapital = system.World.HasComponent<CapitalSettlement>(settlement),
				};
			}
			
			// serialize Settlement Tiles
			var tilesQuery = system.World
				.Query<Entity, ViewStateNode, Location>()
				.Has<CountryTile>(country)
				.Build();

			foreach (var (countryTile, viewStateNode, location) in tilesQuery) {
				countryTiles[countryTile.Identity.Id] = new SerializedCountryTile {
					ownerCountry = country.Identity.Id,
					viewStateNode = viewStateNode,
					location = location,
					ownerSettlement = system.World.GetTarget<CountryTileSettlement>(countryTile).Identity.Id,
				};
			}

		}
		playerCountry = player.playerCountry.Identity.Id;
	}

	public override void Load(ISystem system, ref LoadData loadData) {
		var world = system.World.GetElement<WorldService>();

		// deserialize Countries
		foreach (var (countryID, country) in this.countries) {
			var entity = system.World.Spawn()
				.Add(country.countryData)
				.Id();
			loadData.countries[countryID] = entity;

			system.World.Send(new CountryAdded { country = entity });
		}

		system.World.AddElement<Player>(new Player {
			playerCountry = loadData.countries[playerCountry],
		});

		// deserialize Settlements
		foreach (var (settlementID, settlement) in this.settlements) {
			var country = loadData.countries[settlement.ownerCountry];
			var entity = system.World.Spawn()
				.Add(settlement.settlementData)
				.Add<SettlementOwner>(country);
			if (settlement.isCapital) {
				entity.Add<CapitalSettlement>();
			}
			loadData.settlements[settlementID] = entity.Id();
		}
		
		// deserialize Country Tiles
		foreach (var (countryTileID, countryTile) in this.countryTiles) {
			var country = loadData.countries[countryTile.ownerCountry];
			var settlement = loadData.settlements[countryTile.ownerSettlement];
			var tile = world.GetTile(countryTile.location.hex);
			var entity = system.World.Spawn()
				.Add(countryTile.location)
				.Add(countryTile.viewStateNode)
				.Add<CountryTile>(country)
				.Add<ViewStateOwner>(country)
				.Add<CountryTileSettlement>(settlement)
				.Id();

			system.World.Send(new SettlementBorderUpdated {
				settlement = settlement,
				countryTile = entity,
			});
			loadData.countryTiles[countryTileID] = entity;
			system.World.Send(new ViewStateNodeUpdated { entity = entity });
		}
	}
}

[MessagePackObject]
public partial class SerializedUnits : SerializedState {
	[MessagePackObject]
	public partial class SerializedUnit {
		[Key(0)]
		public UnitData unitData;

		[Key(1)]
		public Location location;

		[Key(2)]
		public ActionQueue actionQueue;
		
		[Key(3)]
		public Movement movement;

		[Key(4)]
		public ViewStateNode viewStateNode;

		[Key(5)]
		public int ownerCountry;
	}

	[Key(0)]
	public Dictionary<int, SerializedUnit> units = new Dictionary<int, SerializedUnit>();

	public override void Save(ISystem system) {
		var unitQuery = system.World.Query<Entity, UnitData, Location, ActionQueue, Movement, ViewStateNode>().Build();

		foreach (var (entity, unitData, location, actionQueue, movement, viewStateNode) in unitQuery) {
			var serializedUnit = new SerializedUnit {
				unitData = unitData,
				location = location,
				actionQueue = actionQueue,
				movement = movement,
				viewStateNode = viewStateNode,
				ownerCountry = system.World.GetTarget<UnitCountry>(entity).Identity.Id,
			};
			units[entity.Identity.Id] = serializedUnit;
		}
		Godot.GD.PrintS($"Saved {units.Count} units");
	}

	public override void Load(ISystem system, ref LoadData loadData) {
		Godot.GD.PrintS($"Loaded {units.Count} units");
		foreach (var (unitID, serializedUnit) in units) {
			var entity = system.World.Spawn()
				.Add(serializedUnit.unitData)
				.Add(serializedUnit.location)
				.Add(serializedUnit.actionQueue)
				.Add(serializedUnit.movement)
				.Add(serializedUnit.viewStateNode)
				.Add<UnitCountry>(loadData.countries[serializedUnit.ownerCountry])
				.Add<ViewStateOwner>(loadData.countries[serializedUnit.ownerCountry])
				.Id();
			loadData.units[unitID] = entity;

			system.World.Send(new UnitAdded { unit = entity });
			system.World.Send(new ViewStateNodeUpdated { entity = entity });
		}
		Godot.GD.PrintS($"Deserialized {loadData.units.Count} units");
	}
}

public partial class LoadData {
	public Dictionary<int, Entity> countries = new Dictionary<int, Entity>();
	public Dictionary<int, Entity> settlements = new Dictionary<int, Entity>();
	public Dictionary<int, Entity> countryTiles = new Dictionary<int, Entity>();
	public Dictionary<int, Entity> units = new Dictionary<int, Entity>();
}

[MessagePackObject]
public partial class SaveData {
	[Key(0)]
	public SerializedWorld world = new SerializedWorld();

	[Key(1)]
	public SerializedCountries countries = new SerializedCountries();

	[Key(2)]
	public SerializedUnits units = new SerializedUnits();

	public void Save(ISystem system) {
		world.Save(system);
		countries.Save(system);
		units.Save(system);
	}

	public void Load(ISystem system) {
		var loadData = new LoadData();
		world.Load(system, ref loadData);
		countries.Load(system, ref loadData);
		units.Load(system, ref loadData);
	}
}

/**
SaveManager represents a save system where each saved game represents multiple saves.
	- each "save game" is stored in a folder under the "saved_games" folder
	- each save game folder has:
		- "save.sav" file, a serialized SavedGame struct
		- "saves" dir, holding files that are serialized SavedGameData structs
*/
public partial class SaveManager {
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
		var saveJSON = (object item, string key) => {
			var p = Godot.ProjectSettings.GlobalizePath($"{SAVE_GAME_FOLDER}/{game.savedGame.name}/saves/{saveMetadata.name} - {key}.json");
			var s = new FileStream(p, FileMode.Create, FileAccess.Write, FileShare.None);
			var textWriter = new StreamWriter(s);
			MessagePackSerializer.SerializeToJson(textWriter, item);
		};
		saveJSON(saveData.countries, "countries");
		saveJSON(saveData.world, "world");
		saveJSON(saveData.units, "units");

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
