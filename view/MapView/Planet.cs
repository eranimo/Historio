using Godot;
using System;


public partial class PlanetData : Resource {
	[Export] public NoiseTexture2D Heightmap;
	[Export] public NoiseTexture2D Normalmap;
	[Export] public NoiseTexture2D Splatmap;
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
	public int ChunkHeight { get => chunkHeight; set { chunkHeight = value; generate(); } }
	public int Seed { get => seed; set { seed = value; generate(); } }
	public int WorldWidth { get => worldWidth; set { worldWidth = value; generate(); } }
	public int WorldHeight { get => worldHeight; set { worldHeight = value; generate(); } }

	public Planet() {
		// generate();
	}

	public override void _Ready() {
		generate();
	}

	private void generate() {
		// RenderingServer.SetDebugGenerateWireframes(true);
		// GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe;

		GD.PrintS("Generating chunks", chunkWidth, chunkHeight);
		foreach (var child in GetChildren()) {
			RemoveChild(child);
		}

		var worldSize = new Vector2(worldWidth, worldHeight);
		var chunkGridSize = new Vector2(chunkWidth, chunkHeight);
		var chunkSize = worldSize / chunkGridSize;
		var terrainSize = chunkGridSize * worldSize;
		GD.PrintS("\t worldSize", worldSize);
		GD.PrintS("\t terrainSize", terrainSize);
		GD.PrintS("\t chunkSize", chunkSize);

		var noise = new FastNoiseLite();
		noise.Frequency = 0.002f;
		noise.Seed = seed;

		var heightmap = new NoiseTexture2D();
		heightmap.In3dSpace = true;
		heightmap.Width = (int) worldSize.x;
		heightmap.Height = (int) worldSize.y;
		heightmap.Noise = noise;

		var normalmap = heightmap.Duplicate() as NoiseTexture2D;
		normalmap.AsNormalMap = true;

		var splatmap = heightmap.Duplicate() as NoiseTexture2D;
		var gradient = new Gradient();
		gradient.SetColor(0, new Color(1f, 0, 0));
		gradient.SetColor(1, new Color(0, 1f, 0));
		splatmap.ColorRamp = gradient;

		planetData = new PlanetData {
			Heightmap = heightmap,
			Normalmap = normalmap,
			Splatmap = splatmap,
		};

		var heightmapDebug = GetParent().GetNode<TextureRect>("UI/Heightmap");
		heightmapDebug.Texture = heightmap;
		heightmapDebug.Size = worldSize / 2f;

		var normalmapDebug = GetParent().GetNode<TextureRect>("UI/Normalmap");
		normalmapDebug.Texture = normalmap;
		normalmapDebug.Size = worldSize / 2f;

		var splatmapDebug = GetParent().GetNode<TextureRect>("UI/Splatmap");
		splatmapDebug.Texture = splatmap;
		splatmapDebug.Size = worldSize / 2f;

		var terrainChunkScene = ResourceLoader.Load<PackedScene>("res://view/MapView/TerrainChunk.tscn");
		for (int x = 0; x < ChunkWidth; x++) {
			for (int y = 0; y < ChunkHeight; y++) {
				var chunk = terrainChunkScene.Instantiate<MapChunk>();
				var chunkPosition = new Vector2(x, y) * chunkSize;
				chunk.Position = new Vector3(chunkPosition.x, 0, chunkPosition.y);
				
				GD.PrintS("Setup chunk", chunkPosition);
				chunk.WorldSize = worldSize;
				chunk.ChunkSize = chunkSize;
				chunk.TerrainSize = terrainSize;
				chunk.ChunkPosition = chunkPosition;
				chunk.PlanetData = planetData;
				AddChild(chunk);
			}
		}
	}
}
