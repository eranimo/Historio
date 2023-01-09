using Godot;
using System;


public static class VectorConvert {
	public static Vector2 ToVector2(Vector2i vec) {
		return new Vector2(vec.x, vec.y);
	}

	public static Vector2i ToVector2i(Vector2 vec) {
		return new Vector2i(
			Convert.ToInt32(vec.x),
			Convert.ToInt32(vec.y)
		);
	}
}

public partial class Planet : Node3D {

	[Export]
	private int seed = 123;

	// size of hex grid
	[Export]
	private Vector2i worldSizeHexes = new Vector2i(200, 100);

	// size of each chunk in hexs
	[Export]
	private Vector2i chunkSizeHexes = new Vector2i(10, 5);

	// size of each hex in map units
	[Export]
	private float hexSize = 1f;

	public Vector2i WorldSizeHexes { get => worldSizeHexes; set { worldSizeHexes = value; Generate(); } }
	public Vector2i ChunkSizehexes { get => chunkSizeHexes; set { chunkSizeHexes = value; Generate(); } }
	public float HexSize { get => hexSize; set { hexSize = value; Generate(); } }

	private Dictionary<Hex, int> hexHeights = new Dictionary<Hex, int>();
	private Dictionary<Vector2i, MapChunk> chunks = new Dictionary<Vector2i, MapChunk>();

	// shader things
	public NoiseTexture2D splatmap;
	public NoiseTexture2D heightmap;

	// world size in map units
	public Vector2i WorldSize => VectorConvert.ToVector2i(new Vector2(worldSizeHexes.x, worldSizeHexes.y) * hexSize);

	// chunk size in map units
	public Vector2i ChunkSize => VectorConvert.ToVector2i(new Vector2(chunkSizeHexes.x, chunkSizeHexes.y) * hexSize);

	// chunk grid size in hexes
	public Vector2i ChunkGridSizeHexes => worldSizeHexes / chunkSizeHexes;


	public void Generate() {
		GD.PrintS("(Planet) Generating");
		// RenderingServer.SetDebugGenerateWireframes(true);
		// GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe;

		GD.PrintS("\t worldSizeHexes", worldSizeHexes);
		GD.PrintS("\t chunkSizeHexes", chunkSizeHexes);
		GD.PrintS("\t hexSize", hexSize);
		GD.PrintS("\t worldSize", WorldSize);
		GD.PrintS("\t chunkSize", ChunkSize);
		GD.PrintS("\t chunkGridSizeHexes", ChunkGridSizeHexes);

		var hexHeightNoise = new WorldNoise(worldSizeHexes.x, worldSizeHexes.y, seed);
		for (int x = 0; x < WorldSizeHexes.x; x++) {
			for (int y = 0; y < WorldSizeHexes.y; y++) {
				var height = Convert.ToInt32(hexHeightNoise.Get(x, y) * 5);
				hexHeights[new Hex(x, y)] = height;
			}
		}

		foreach (var child in GetChildren()) {
			RemoveChild(child);
		}

		var noise = new FastNoiseLite();
		noise.Frequency = 0.002f;
		noise.Seed = seed;

		heightmap = new NoiseTexture2D();
		heightmap.In3dSpace = true;
		heightmap.Width = Convert.ToInt32(WorldSize.x);
		heightmap.Height = Convert.ToInt32(WorldSize.y);
		heightmap.Noise = noise;

		splatmap = heightmap.Duplicate() as NoiseTexture2D;
		var gradient = new Gradient();
		gradient.SetColor(0, new Color(1f, 0, 0));
		gradient.SetColor(1, new Color(0, 1f, 0));
		splatmap.ColorRamp = gradient;

		var heightmapDebug = GetParent().GetNode<TextureRect>("UI/Heightmap");
		heightmapDebug.Texture = heightmap;
		heightmapDebug.Size = WorldSize / 2;

		var splatmapDebug = GetParent().GetNode<TextureRect>("UI/Splatmap");
		splatmapDebug.Texture = splatmap;
		splatmapDebug.Size = WorldSize / 2;

		for (int x = 0; x < ChunkGridSizeHexes.x; x++) {
			for (int y = 0; y < ChunkGridSizeHexes.y; y++) {
				var chunk = SpawnChunk(new Vector2i(x, y));
				chunk.Position = new Vector3(chunk.ChunkPosition.x, 0, chunk.ChunkPosition.y);
			}
		}
	}

	public MapChunk SpawnChunk(Vector2i chunkID) {
		if (chunks.ContainsKey(chunkID)) {
			return chunks[chunkID];
		}
		// GD.PrintS("Setup chunk", chunkID);
		var terrainChunkScene = ResourceLoader.Load<PackedScene>("res://view/MapView/TerrainChunk.tscn");
		var chunk = terrainChunkScene.Instantiate<MapChunk>();
		chunk.Setup(this);
		var chunkPosition = chunkID * ChunkSize;

		chunk.ChunkID = chunkID;
		chunk.ChunkPosition = chunkPosition;
		chunks[chunkID] = chunk;
		AddChild(chunk);
		// chunk.OnDespawn += () => this.handleChunkDespawn(chunk);
		return chunk;
	}

	private void handleChunkDespawn(MapChunk chunk) {
		RemoveChild(chunks[chunk.ChunkID]);
	}
}
