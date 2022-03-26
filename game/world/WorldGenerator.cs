using System;
using System.Collections.Generic;


public class WorldOptions {
	public WorldSize Size = WorldSize.Small;
	public int Seed = 12345;
	public int Sealevel = 140;
	public double AxialTilt = 23.45;
}

public enum WorldSize {
	Small = 0,
	Medium = 1,
	Large = 2,
}

public class WorldGenerator : GameGenerator {
	public WorldOptions options;
	private int TileWidth;
	private int TileHeight;

	public WorldGenerator(WorldOptions options) {
		this.options = options;
	}

	private int GetWorldSize(WorldSize size) {
		switch (size) {
			case WorldSize.Small: return 150;
			case WorldSize.Medium: return 300;
			case WorldSize.Large: return 600;
			default: throw new Exception("Unknown size");
		}
	}

	public override void Generate(GameManager manager) {
		var size = GetWorldSize(this.options.Size);
		this.TileWidth = size * 2;
		this.TileHeight = size;

		var heightNoise = new WorldNoise(this.TileWidth, this.TileHeight, this.options.Seed);
		var temperatureNoise = new WorldNoise(this.TileWidth, this.TileHeight, this.options.Seed * 2);
		var rainfallNoise = new WorldNoise(this.TileWidth, this.TileHeight, this.options.Seed * 3);

		var tiles = new List<Tile>();

		for (var x = 0; x < this.TileWidth; x++) {
			for (var y = 0; y < this.TileHeight; y++) {
				var height = heightNoise.Get(x, y) * 255;
				var temperature = temperatureNoise.Get(x, y);
				var rainfall = rainfallNoise.Get(x, y);
				var coord = new Hex(x, y);
				var coordLong = ((x / (double) this.TileWidth) * 360) - 180;
				var coordLat = ((-y / (double) this.TileHeight) * 180) + 90;

				Tile tile = new Tile(coord);
				tile.height = height;
				tile.temperature = temperature;
				tile.rainfall = rainfall;
				tiles.Add(tile);
			}
		}

		foreach (Tile tile in tiles) {
			if (tile.height < this.options.Sealevel - 10) {
				tile.biome = BiomeType.Ocean;
			} else if (tile.height < this.options.Sealevel) {
				tile.biome = BiomeType.Coast;
			} else {
				if (tile.temperature < 0.10) {
					tile.biome = BiomeType.Arctic;
				} else if (tile.temperature < 0.70) {
					tile.biome = BiomeType.Temperate;
					if (tile.rainfall > 0.5) {
						tile.feature = FeatureType.Forest;
					} else {
						tile.feature = FeatureType.Grassland;
					}
				} else {
					tile.biome = BiomeType.Desert;
				}
			}
		}
		var worldSize = new Hex(TileWidth, TileHeight);

		foreach (Tile tile in tiles) {
			manager.AddEntity(tile);
		}
		manager.state.worldSize = worldSize;
		manager.state.worldOptions = options;
	}
}
