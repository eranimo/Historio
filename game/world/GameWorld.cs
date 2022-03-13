using System;
using LibNoise;
using Godot;
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

class WorldNoise {
	public int width;
	public int height;
	public int octaves;
	public int frequency;
	public float amplitude;
	private LibNoise.Primitive.ImprovedPerlin noise;

	public WorldNoise(int width, int height, int seed, int octaves = 5, int frequency = 2, float amplitude = 0.5f) {
		this.width = width;
		this.height = height;
		this.octaves = octaves;
		this.frequency = frequency;
		this.amplitude = amplitude;
		this.noise = new LibNoise.Primitive.SimplexPerlin(seed, NoiseQuality.Best);
	}

	/// <summary>Gets a coordinate noise value projected onto a sphere</summary>
	public float Get(int x, int y) {
		var coordLong = ((x / (double) this.width) * 360) - 180;
		var coordLat = ((-y / (double) this.height) * 180) + 90;
		var inc = ((coordLat + 90.0) / 180.0) * Math.PI;
		var azi = ((coordLong + 180.0) / 360.0) * (2 * Math.PI);
		var nx = (float) (Math.Sin(inc) * Math.Cos(azi));
		var ny = (float) (Math.Sin(inc) * Math.Sin(azi));
		var nz = (float) (Math.Cos(inc));

		float amplitude = 1;
		float freq = 1;
		var v = 0f;
		for (var i = 0; i < this.octaves; i++) {
			v += this.noise.GetValue(nx * freq, ny * freq, nz * freq) * amplitude;
			amplitude *= this.amplitude;
			freq *= this.frequency;
		}

		v = (v + 1) / 2;
		return v;
	}
}

public class WorldGenerator {
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

	public GameWorld Generate() {
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
				var coord = new OffsetCoord(x, y);
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
		var worldSize = new OffsetCoord(TileWidth, TileHeight);
		var gameWorld = new GameWorld(options, worldSize, tiles);
		return gameWorld;
	}
}

public class GameWorld {
	public readonly WorldOptions options;
	public readonly OffsetCoord worldSize;
	public readonly List<Tile> tiles;
	private Dictionary<OffsetCoord, Tile> tilesByCoord = new Dictionary<OffsetCoord, Tile>();

	public GameWorld(
		WorldOptions options,
		OffsetCoord worldSize,
		List<Tile> tiles
	) {
		this.options = options;
		this.worldSize = worldSize;
		this.tiles = tiles;

		foreach (Tile tile in tiles) {
			tilesByCoord[tile.coord] = tile;
		}
	}

	public Tile GetTile(OffsetCoord coord) {
		return tilesByCoord[coord];
	}

	public bool IsValidTile(OffsetCoord coord) {
		return coord.col >= 0 && coord.row >= 0 && coord.col < worldSize.col && coord.row < worldSize.row;
	}
}
