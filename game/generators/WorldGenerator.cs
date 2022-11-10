using System;
using Godot;
using System.Linq;
using System.Collections.Generic;
using MessagePack;
using PriorityQueues;

[MessagePackObject(keyAsPropertyName: true)]
public partial class WorldOptions {
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
public partial class WorldData {
	[Key(0)]
	public Hex worldSize;
	[Key(1)]
	public WorldOptions options;
}

public partial class WorldGenerator : IGeneratorStep {
	private int TileWidth;
	private int TileHeight;
	private HexGrid hexGrid;

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

		hexGrid = new HexGrid(TileWidth, TileHeight);

		var heightNoise = new WorldNoise(this.TileWidth, this.TileHeight, options.Seed, 5, 2.5f);
		var temperatureNoise = new WorldNoise(this.TileWidth, this.TileHeight, options.Seed * 2);
		var rainfallNoise = new WorldNoise(this.TileWidth, this.TileHeight, options.Seed * 3, 5, 2.5f);

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
		var neighborsWithDir = new Dictionary<Hex, Dictionary<HexDirection, Hex>>();
		foreach (var hex in tiles.Keys) {
			var validNeighbors = hex.NeighborsWithDir()
				.Where(item => tiles.ContainsKey(item.hex))
				.ToHashSet();
			neighbors[hex] = validNeighbors
				.Select(item => item.hex)
				.ToHashSet();
			neighborsWithDir[hex] = new Dictionary<HexDirection, Hex>();
			foreach (var (n, dir) in validNeighbors) {
				neighborsWithDir[hex][dir] = n;
			}			
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
		GD.PrintS($"(WorldGenerator) Number of land tiles: {tiles.Count - oceans.Count}");

		// find lakes
		var open = new BinaryPriorityQueue<Hex>((a, b) => tiles[a].waterHeight.CompareTo(tiles[b].waterHeight));
		var closed = new HashSet<Hex>();
		var pit = new Queue<Hex>();
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

			var waterHeight = tiles[item].waterHeight;

			foreach (var neighbor in neighbors[item]) {
				if (closed.Contains(neighbor)) {
					continue;
				}
				closed.Add(neighbor);
				
				if (tiles[neighbor].waterHeight <= waterHeight) {
					tiles[neighbor].waterHeight = waterHeight;
					pit.Enqueue(neighbor);
				} else {
					open.Enqueue(neighbor);
				}
			}
		}

		var lakes = new HashSet<Hex>();
		foreach (var (hex, tileData) in tiles) {
			if (!oceans.Contains(hex) && tileData.waterHeight > tileData.height) {
				lakes.Add(hex);
			}
		}
		GD.PrintS($"(WorldGenerator) Number of lake tiles: {lakes.Count}");

		// find flow directions
		open = new BinaryPriorityQueue<Hex>((a, b) => tiles[a].waterHeight.CompareTo(tiles[b].waterHeight));
		closed = new HashSet<Hex>();
		var flowDirs = new Dictionary<Hex, HexDirection>();
		foreach (var (hex, tileData) in tiles) {
			if (hexGrid.IsInside(hex)) {
				open.Enqueue(hex);
				closed.Add(hex);

				if (hex.row == 0) {
					flowDirs[hex] = HexDirection.North;
				} else if (hex.row == (worldSize.row - 1)) {
					flowDirs[hex] = HexDirection.South;
				} else if (hex.col == 0) {
					flowDirs[hex] = HexDirection.SouthWest;
				} else if (hex.col == (worldSize.col - 1)) {
					flowDirs[hex] = HexDirection.SouthEast;
				}
			}
		}
		while (!open.IsEmpty()) {
			Hex item = open.Dequeue();
			foreach (var (dir, neighbor) in neighborsWithDir[item]) {
				if (!closed.Contains(neighbor)) {
					flowDirs[neighbor] = dir.Opposite();
					closed.Add(neighbor);
					open.Enqueue(neighbor);
				}
			}
		}

		// river generation
		var riverWater = new Dictionary<Hex, float>();
		var riverFlow = new Dictionary<Hex, float>();
		var riverQueue = new Queue<Hex>();
		foreach (var (hex, tileData) in tiles) {
			var initialRainfall = tileData.rainfall / 10f;
			riverWater[hex] = initialRainfall;
			riverFlow[hex] = initialRainfall;
			riverQueue.Enqueue(hex);
		}
		while (riverQueue.Count > 0) {
			var item = riverQueue.Dequeue();
			var downstreamHex = item.Neighbor(flowDirs[item]);

			if (tiles.ContainsKey(downstreamHex)) {
				if (oceans.Contains(downstreamHex)) {
					riverWater[item] = 0;
				} else {
					riverWater[downstreamHex] += riverWater[item];
					riverFlow[downstreamHex] += riverWater[item];
					riverWater[item] = 0;
					riverQueue.Enqueue(downstreamHex);
				}
			}
		}
		GD.PrintS($"(WorldGenerator) river generation");
		GD.PrintS($"(WorldGenerator) River Flow\tMin: {riverFlow.Values.Min()} Max: {riverFlow.Values.Max()} Avg: {riverFlow.Values.Average()}");

		var rivers = new HashSet<Hex>();
		foreach (var hex in tiles.Keys) {
			if (riverFlow[hex] >= 500) {
				rivers.Add(hex);
			}
		}
		GD.PrintS($"(WorldGenerator) Number of river tiles: {rivers.Count}");

		// generate streams
		var validStreamEdges = new HashSet<HexEdge>();
		var streamFlow = new Dictionary<HexEdge, float>();
		var streamWater = new Dictionary<HexEdge, float>();
		var streamHeight = new Dictionary<HexEdge, float>();
		var streamQueue = new Queue<HexEdge>();

		foreach (var (hex, tileData) in tiles) {
			foreach (var (dir, neighbor) in neighborsWithDir[hex]) {
				if (
					lakes.Contains(hex) || lakes.Contains(neighbor) ||
					oceans.Contains(hex) || oceans.Contains(neighbor) ||
					rivers.Contains(hex) || rivers.Contains(neighbor)
				) {
					continue;
				}
				var edge = hex.GetEdge(dir);
				if (validStreamEdges.Contains(edge)) {
					continue;
				}

				validStreamEdges.Add(edge);
				var c = 2f;
				if (tiles.ContainsKey(edge.H3)) {
					c++;
				}
				if (tiles.ContainsKey(edge.H4)) {
					c++;
				}

				float v = (
					tiles[edge.H1].rainfall + tiles[edge.H2].rainfall +
					(tiles.ContainsKey(edge.H3) ? tiles[edge.H3].rainfall : 0) +
					(tiles.ContainsKey(edge.H4) ? tiles[edge.H4].rainfall : 0)
				) / c;
				float h = (
					tiles[edge.H1].height + tiles[edge.H2].height +
					(tiles.ContainsKey(edge.H3) ? tiles[edge.H3].height : 0) +
					(tiles.ContainsKey(edge.H4) ? tiles[edge.H4].height : 0)
				) / c;
				// float h = (tiles[edge.H1].height + tiles[edge.H2].height) / 2f;
				streamFlow[edge] = v;
				streamWater[edge] = v;
				streamHeight[edge] = h;
				streamQueue.Enqueue(edge);
			}
		}
		GD.PrintS("Valid stream edge count:", validStreamEdges.Count);

		var possibleEdges = new List<(HexEdge, Hex)>();
		while (streamQueue.Count > 0) {
			var item = streamQueue.Dequeue();
			var itemHeight = streamHeight[item];
			var itemWater = streamWater[item];

			possibleEdges.Clear();
			if (tiles.ContainsKey(item.H3)) {
				possibleEdges.Add((item.E3, item.H3));
				possibleEdges.Add((item.E4, item.H3));
			} else if (tiles.ContainsKey(item.H4)) {
				possibleEdges.Add((item.E1, item.H4));
				possibleEdges.Add((item.E2, item.H4));
			}

			var validEdges = possibleEdges.FindAll(i => streamHeight.ContainsKey(i.Item1) && streamHeight[i.Item1] < itemHeight);

			if (validEdges.Count == 0) {
				streamWater[item] = 0;
				continue;
			}

			var (downstreamEdge, downstreamHex) = validEdges.MinBy(i => streamHeight[i.Item1]);

			streamWater[downstreamEdge] += itemWater;
			streamFlow[downstreamEdge] += itemWater;
			streamWater[item] = 0;

			if (!(rivers.Contains(downstreamHex) || lakes.Contains(downstreamHex) || oceans.Contains(downstreamHex))) {
				streamQueue.Enqueue(downstreamEdge);
			}
		}
		GD.PrintS($"(WorldGenerator) stream generation");
		GD.PrintS($"(WorldGenerator) Stream Flow\tMin: {streamFlow.Values.Min()} Max: {streamFlow.Values.Max()} Avg: {streamFlow.Values.Average()}");

		// decide biomes
		foreach (var (hex, tileData) in tiles) {
			tileData.flowDir = flowDirs[hex];
			tileData.riverFlow = riverFlow[hex];

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

				if (lakes.Contains(hex)) {
					tileData.biome = Tile.BiomeType.Freshwater;
					tileData.feature = Tile.FeatureType.Lake;
				} else if (rivers.Contains(hex)) {
					tileData.biome = Tile.BiomeType.Freshwater;
					tileData.feature = Tile.FeatureType.River;
				}

				foreach (var (edge, dir) in hex.EdgesWithDir()) {
					if (streamFlow.ContainsKey(edge)) {
						tileData.streamFlow[dir] = streamFlow[edge];
						if (streamFlow[edge] >= 1000) {
							tileData.streams[edge.Direction] = true;
						}
					}
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
