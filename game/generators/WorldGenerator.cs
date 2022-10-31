using System;
using Godot;
using System.Linq;
using System.Collections.Generic;
using MessagePack;
using PriorityQueues;

[MessagePackObject(keyAsPropertyName: true)]
public class WorldOptions {
	public WorldSize Size = WorldSize.Small;
	public int SeaLevel = 140;
	public double AxialTilt = 23.45;
}

public enum WorldSize {
	Small = 0,
	Medium = 1,
	Large = 2,
}

[MessagePackObject]
public class WorldData {
	[Key(0)]
	public Hex worldSize;
	[Key(1)]
	public WorldOptions options;
}

public class WorldGenerator : IGeneratorStep {
	private int TileWidth;
	private int TileHeight;

	private int GetWorldSize(WorldSize size) {
		switch (size) {
			case WorldSize.Small: return 150;
			case WorldSize.Medium: return 300;
			case WorldSize.Large: return 600;
			default: throw new Exception("Unknown size");
		}
	}

	public void Generate(GameOptions options, GameManager manager) {
		var worldOptions = options.world;
		var size = GetWorldSize(worldOptions.Size);
		this.TileWidth = size * 2;
		this.TileHeight = size;

		var heightNoise = new WorldNoise(this.TileWidth, this.TileHeight, options.Seed);
		var temperatureNoise = new WorldNoise(this.TileWidth, this.TileHeight, options.Seed * 2);
		var rainfallNoise = new WorldNoise(this.TileWidth, this.TileHeight, options.Seed * 3);

		var worldSize = new Hex(TileWidth, TileHeight);
		manager.world.initWorld(worldSize);

		var tiles = new Dictionary<Hex, TileData>();

		for (var x = 0; x < this.TileWidth; x++) {
			for (var y = 0; y < this.TileHeight; y++) {
				var height = heightNoise.Get(x, y) * 255;
				var temperature = temperatureNoise.Get(x, y) * 255;
				var rainfall = rainfallNoise.Get(x, y) * 255;
				var coordLong = ((x / (double) this.TileWidth) * 360) - 180;
				var coordLat = ((-y / (double) this.TileHeight) * 180) + 90;

				TileData tileData = new TileData {
					height = height,
					temperature = temperature,
					rainfall = rainfall,
					waterHeight = height,
				};
				Hex hex = new Hex(x, y);

				tiles.Add(hex, tileData);
			}
		}

		// calculate and cache neighbors
		var neighbors = new Dictionary<Hex, HashSet<Hex>>();
		foreach (var hex in tiles.Keys) {
			neighbors[hex] = hex.Neighbors().Where(hex => tiles.ContainsKey(hex)).ToHashSet();
		}

		// fill oceans but keep depressions
		var oceans = new HashSet<Hex>();
		var oceanQueue = new Stack<Hex>(worldSize.row * worldSize.col);
		oceanQueue.Push(new Hex(0, 0));
		oceanQueue.Push(new Hex(worldSize.col - 1, 0));
		oceanQueue.Push(new Hex(0, worldSize.row - 1));
		oceanQueue.Push(new Hex(worldSize.col - 1, worldSize.row - 1));
		while (oceanQueue.Count > 0) {
			var item = oceanQueue.Pop();
			if (tiles[item].height <= worldOptions.SeaLevel) {
				oceans.Add(item);

				foreach (var neighbor in neighbors[item]) {
					if (!oceans.Contains(neighbor)) {
						oceanQueue.Push(neighbor);
					}
				}
			}
		}
		GD.PrintS($"(WorldGenerator) Number of ocean tiles: {oceans.Count}");

		// fill depressions with lakes
		var open = new BinaryPriorityQueue<Hex>((a, b) => tiles[a].waterHeight.CompareTo(tiles[b].waterHeight));
		var pit = new Queue<Hex>();
		var closed = new HashSet<Hex>();
		foreach (var (hex, tileData) in tiles) {
			if (
				hex.col == 0 || hex.row == 0 ||
				hex.col == (worldSize.col - 1) || hex.row == (worldSize.row - 1)
			) {
				open.Enqueue(hex);
				closed.Add(hex);
			}
		}
		while (!open.IsEmpty() || pit.Count > 0) {
			Hex item;
			if (pit.Count > 0) {
				item = pit.Dequeue();
			} else {
				item = open.Dequeue();
			}
			foreach (var neighbor in neighbors[item]) {
				if (closed.Contains(neighbor)) {
					continue;
				}
				closed.Add(neighbor);
				var waterHeight = tiles[item].waterHeight;
				if (tiles[neighbor].height <= waterHeight) {
					tiles[neighbor].waterHeight = waterHeight;
					pit.Enqueue(neighbor);
				} else {
					open.Enqueue(neighbor);
				}
			}
		}

		// river generation
		var rng = new RandomNumberGenerator();
		rng.Seed = (ulong) options.Seed;
		int riverSegments = 0;
		foreach (var (hex, tile) in tiles) {
			if (rng.Randf() < 0.1f) {
				foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection))) {
					bool addRiver = rng.Randf() < 0.20f;
					tile.riverSegments[dir] = addRiver;
					if (addRiver) {
						riverSegments++;
					}
				}
			}
		}

		GD.PrintS("(WorldGenerator) river generation", riverSegments);

		// decide biomes
		foreach (var (hex, tileData) in tiles) {
			if (oceans.Contains(hex)) {
				if (tileData.height < worldOptions.SeaLevel - 10) {
					tileData.biome = Tile.BiomeType.Ocean;
				} else if (tileData.height < worldOptions.SeaLevel) {
					tileData.biome = Tile.BiomeType.Coast;
				}
			} else {
				if (tileData.temperature < 25) {
					tileData.biome = Tile.BiomeType.Arctic;
				} else if (tileData.temperature < 178) {
					tileData.biome = Tile.BiomeType.Temperate;
					if (tileData.rainfall > 127) {
						tileData.feature = Tile.FeatureType.Forest;
					} else {
						tileData.feature = Tile.FeatureType.Grassland;
					}
				} else {
					tileData.biome = Tile.BiomeType.Desert;
				}

				if (tileData.waterHeight > tileData.height) {
					tileData.biome = Tile.BiomeType.Freshwater;
					tileData.feature = Tile.FeatureType.Lake;
				}
			}
		}

		foreach (var (hex, tileData) in tiles) {
			Entity tile = manager.Spawn()
				.Add<Location>(new Location { hex = hex, })
				.Add<TileData>(tileData)
				.Id();
			manager.world.AddTile(hex, tile);
		}

		GD.PrintS($"(WorldGenerator) Added {manager.world.tiles.Count} tiles");
		manager.state.GetElement<PathfindingService>().setup();

		manager.state.AddElement<WorldData>(new WorldData {
			worldSize = worldSize,
			options = worldOptions,
		});
	}
}
