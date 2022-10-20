using System;
using Godot;
using System.Collections.Generic;
using MessagePack;

[MessagePackObject(keyAsPropertyName: true)]
public class WorldOptions {
	public WorldSize Size = WorldSize.Small;
	public int Sealevel = 140;
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
				var temperature = temperatureNoise.Get(x, y);
				var rainfall = rainfallNoise.Get(x, y);
				var coordLong = ((x / (double) this.TileWidth) * 360) - 180;
				var coordLat = ((-y / (double) this.TileHeight) * 180) + 90;

				TileData tileData = new TileData {
					height = height,
					temperature = temperature,
					rainfall = rainfall,
				};
				Hex hex = new Hex(x, y);

				if (tileData.height < worldOptions.Sealevel - 10) {
					tileData.biome = Tile.BiomeType.Ocean;
				} else if (tileData.height < worldOptions.Sealevel) {
					tileData.biome = Tile.BiomeType.Coast;
				} else {
					if (tileData.temperature < 0.10) {
						tileData.biome = Tile.BiomeType.Arctic;
					} else if (tileData.temperature < 0.70) {
						tileData.biome = Tile.BiomeType.Temperate;
						if (tileData.rainfall > 0.5) {
							tileData.feature = Tile.FeatureType.Forest;
						} else {
							tileData.feature = Tile.FeatureType.Grassland;
						}
					} else {
						tileData.biome = Tile.BiomeType.Desert;
					}
				}

				tiles.Add(hex, tileData);
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


		foreach (var (hex, tileData) in tiles) {
			manager.world.AddTile(
				hex,
				manager.Spawn()
					.Add<Location>(new Location { hex = hex, })
					.Add<TileData>(tileData)
					.Id()
			);
		}

		GD.PrintS($"(WorldGenerator) Added {manager.world.tiles.Count} tiles");
		manager.state.GetElement<PathfindingService>().setup();

		manager.state.AddElement<WorldData>(new WorldData {
			worldSize = worldSize,
			options = worldOptions,
		});
	}
}
