using Godot;
using System;


public partial class PlanetData : Resource {
	[Export] public NoiseTexture2D Heightmap;
	[Export] public NoiseTexture2D Splatmap;
	[Export] public Vector2i WorldSize;
	[Export] public Vector2i ChunkGridSize;

	public Vector2i ChunkSize => WorldSize / ChunkGridSize;
}

public partial class Planet : Node3D {

	[Export]
	private int seed = 123;

	[Export]
	private int chunkWidth = 5;

	[Export]
	private int chunkHeight = 5;

	[Export]
	private int worldWidth = 500;

	[Export]
	private int worldHeight = 250;

	[Export]
	private PlanetData planetData;

	public int ChunkWidth { get => chunkWidth; set { chunkWidth = value; } }
	public int ChunkHeight { get => chunkHeight; set { chunkHeight = value; } }
	public int Seed { get => seed; set { seed = value; } }
	public int WorldWidth { get => worldWidth; set { worldWidth = value; } }
	public int WorldHeight { get => worldHeight; set { worldHeight = value; } }

	public PlanetData PlanetData { get => planetData; set => planetData = value; }

	private Dictionary<Vector2i, MapChunk> chunks = new Dictionary<Vector2i, MapChunk>();

	public void Generate() {
		// RenderingServer.SetDebugGenerateWireframes(true);
		// GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe;

		GD.PrintS("Generating chunks", chunkWidth, chunkHeight);
		foreach (var child in GetChildren()) {
			RemoveChild(child);
		}

		var worldSize = new Vector2i(worldWidth, worldHeight);
		var chunkGridSize = new Vector2i(chunkWidth, chunkHeight);

		var noise = new FastNoiseLite();
		noise.Frequency = 0.002f;
		noise.Seed = seed;

		var heightmap = new NoiseTexture2D();
		heightmap.In3dSpace = true;
		heightmap.Width = worldSize.x;
		heightmap.Height = worldSize.y;
		heightmap.Noise = noise;

		var splatmap = heightmap.Duplicate() as NoiseTexture2D;
		var gradient = new Gradient();
		gradient.SetColor(0, new Color(1f, 0, 0));
		gradient.SetColor(1, new Color(0, 1f, 0));
		splatmap.ColorRamp = gradient;

		PlanetData = new PlanetData {
			WorldSize = worldSize,
			ChunkGridSize = chunkGridSize,
			Heightmap = heightmap,
			Splatmap = splatmap,
		};

		var heightmapDebug = GetParent().GetNode<TextureRect>("UI/Heightmap");
		heightmapDebug.Texture = heightmap;
		heightmapDebug.Size = worldSize / 2;

		var splatmapDebug = GetParent().GetNode<TextureRect>("UI/Splatmap");
		splatmapDebug.Texture = splatmap;
		splatmapDebug.Size = worldSize / 2;

		
		for (int x = 0; x < ChunkWidth; x++) {
			for (int y = 0; y < ChunkHeight; y++) {
				var chunk = SpawnChunk(new Vector2i(x, y));
				chunk.Position = new Vector3(chunk.ChunkPosition.x, 0, chunk.ChunkPosition.y);
			}
		}
	}

	public MapChunk SpawnChunk(Vector2i chunkID) {
		if (chunks.ContainsKey(chunkID)) {
			return chunks[chunkID];
		}
		GD.PrintS("Setup chunk", chunkID);
		var terrainChunkScene = ResourceLoader.Load<PackedScene>("res://view/MapView/TerrainChunk.tscn");
		var chunk = terrainChunkScene.Instantiate<MapChunk>();
		var chunkPosition = chunkID * PlanetData.ChunkSize;

		chunk.ChunkID = chunkID;
		chunk.ChunkPosition = chunkPosition;
		chunk.PlanetData = PlanetData;
		chunks[chunkID] = chunk;
		AddChild(chunk);
		// chunk.OnDespawn += () => this.handleChunkDespawn(chunk);
		return chunk;
	}

	private void handleChunkDespawn(MapChunk chunk) {
		RemoveChild(chunks[chunk.ChunkID]);
	}
}
