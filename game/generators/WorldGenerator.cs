using System;
using Godot;
using System.Collections.Generic;


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

public class WorldData {
	public Hex worldSize;
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

		manager.world.initWorld(new Point(TileWidth, TileHeight));

		for (var x = 0; x < this.TileWidth; x++) {
			for (var y = 0; y < this.TileHeight; y++) {
				var height = heightNoise.Get(x, y) * 255;
				var temperature = temperatureNoise.Get(x, y);
				var rainfall = rainfallNoise.Get(x, y);
				var coordLong = ((x / (double) this.TileWidth) * 360) - 180;
				var coordLat = ((-y / (double) this.TileHeight) * 180) + 90;

				TileData tile = new TileData {
					height = height,
					temperature = temperature,
					rainfall = rainfall,
				};
				Hex hex = new Hex(x, y);

				if (tile.height < worldOptions.Sealevel - 10) {
					tile.biome = Tile.BiomeType.Ocean;
				} else if (tile.height < worldOptions.Sealevel) {
					tile.biome = Tile.BiomeType.Coast;
				} else {
					if (tile.temperature < 0.10) {
						tile.biome = Tile.BiomeType.Arctic;
					} else if (tile.temperature < 0.70) {
						tile.biome = Tile.BiomeType.Temperate;
						if (tile.rainfall > 0.5) {
							tile.feature = Tile.FeatureType.Forest;
						} else {
							tile.feature = Tile.FeatureType.Grassland;
						}
					} else {
						tile.biome = Tile.BiomeType.Desert;
					}
				}

				manager.world.AddTile(hex, tile);
			}
		}
		GD.PrintS($"Added {manager.world.tiles.Count} tiles");
		manager.state.GetElement<Pathfinder>().setup();

		manager.state.AddElement<WorldData>(new WorldData {
			worldSize = new Hex(TileWidth, TileHeight),
			options = worldOptions,
		});
	}
}
