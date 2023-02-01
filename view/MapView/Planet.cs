using Godot;
using System;


public static class VectorConvert {
	public static Vector2 ToVector2(Vector2i vec) {
		return new Vector2(vec.x, vec.y);
	}

	public static Vector2 Flatten(Vector3 vec) {
		return new Vector2(vec.x, vec.z);
	}

	public static Vector2i ToVector2i(Vector2 vec) {
		return new Vector2i(
			Convert.ToInt32(vec.x),
			Convert.ToInt32(vec.y)
		);
	}

	public static Vector3 ToVector3(Vector2 vec, float y = 0) {
		return new Vector3(vec.x, y, vec.y);
	}
}

public class PlanetTile {
	private Hex hex;
	public Hex Hex { get => hex; set => hex = value; }

	public Tile.BiomeType Biome { get; set; }
	public Tile.TerrainType Terrain { get; set; }
	public int Level { get; set; }

	public PlanetTile(Hex _hex) {
		Hex = _hex;
	}
}

public partial class TextureDataGrid {
	private Vector2i size;
	private Image image;
	private ImageTexture texture;

	public TextureDataGrid(Vector2i size){
		this.size = size;
		this.image = Image.Create(size.x, size.y, false, Image.Format.Rgbaf);
		Texture = ImageTexture.CreateFromImage(image);
	}

	public ImageTexture Texture { get => texture; set => texture = value; }

	public void Set(Vector2i position, Color value) {
		image.SetPixel(position.x, position.y, value);
	}

	public void Update() {
		Texture.Update(image);
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
	public Vector2i ChunkSizeHexes { get => chunkSizeHexes; set { chunkSizeHexes = value; Generate(); } }
	public float HexSize { get => hexSize; set { hexSize = value; Generate(); } }

	private Dictionary<Vector2i, MapChunk> chunks = new Dictionary<Vector2i, MapChunk>();
	private Dictionary<Hex, PlanetTile> hexTiles = new Dictionary<Hex, PlanetTile>();
	private MultiMap<Vector2i, PlanetTile> chunkTiles = new MultiMap<Vector2i, PlanetTile>();

	// world size in map units
	public Vector2i WorldSize => VectorConvert.ToVector2i(new Vector2(worldSizeHexes.x, worldSizeHexes.y) * hexSize);

	// chunk size in map units
	public Vector2i ChunkSize => VectorConvert.ToVector2i(new Vector2(chunkSizeHexes.x, chunkSizeHexes.y) * hexSize);

	// chunk grid size in hexes
	public Vector2i ChunkGridSizeHexes => worldSizeHexes / chunkSizeHexes;

	public TextureDataGrid hexData;

	private TerrainChunkMesh terrainChunkMesh;

	public void Generate() {
		GD.PrintS("(Planet) Generating");

		// RenderingServer.SetDebugGenerateWireframes(true);
		// GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe;

		var watch = System.Diagnostics.Stopwatch.StartNew();

		GD.PrintS("\t worldSizeHexes", worldSizeHexes);
		GD.PrintS("\t chunkSizeHexes", chunkSizeHexes);
		GD.PrintS("\t hexSize", hexSize);
		GD.PrintS("\t worldSize", WorldSize);
		GD.PrintS("\t chunkSize", ChunkSize);
		GD.PrintS("\t chunkGridSizeHexes", ChunkGridSizeHexes);

		var heightNoise = new WorldNoise(worldSizeHexes.x, worldSizeHexes.y, seed);
		var terrainNoise = new WorldNoise(worldSizeHexes.x, worldSizeHexes.y, seed * 2);
		var temperatureNoise = new WorldNoise(worldSizeHexes.x, worldSizeHexes.y, seed * 3);
		var rainfallNoise = new WorldNoise(worldSizeHexes.x, worldSizeHexes.y, seed * 3);

		hexData = new TextureDataGrid(worldSizeHexes);

		for (int x = 0; x < WorldSizeHexes.x; x++) {
			for (int y = 0; y < WorldSizeHexes.y; y++) {
				var height = Convert.ToInt32(Mathf.Round(heightNoise.Get(x, y) * 255));
				var terrain = heightNoise.Get(x, y);
				var temperature = temperatureNoise.Get(x, y);
				var rainfall = rainfallNoise.Get(x, y);
				var hex = new Hex(x, y);
				var tile = new PlanetTile(hex);
				
				if (height < 100) {
					tile.Biome = Tile.BiomeType.Ocean;
					tile.Level = 1;
				} else if (height < 150) {
					tile.Biome = Tile.BiomeType.Coast;
					tile.Level = 2;
				} else {
					tile.Biome = Tile.BiomeType.Temperate;
					tile.Level = 3;
					
					if (terrain < 0.33) {
						tile.Terrain = Tile.TerrainType.Plains;
					} else if (terrain < 0.66) {
						tile.Terrain = Tile.TerrainType.Hills;
					} else {
						tile.Terrain = Tile.TerrainType.Mountains;
					}

					if (temperature < 0.3) {
						tile.Biome = Tile.BiomeType.Arctic;
					} else if (temperature < 0.6) {
						tile.Biome = Tile.BiomeType.Temperate;
					} else {
						if (rainfall < 0.3) {
							tile.Biome = Tile.BiomeType.Desert;
						} else {
							tile.Biome = Tile.BiomeType.Tropical;
						}
					}
				}
				hexTiles[hex] = tile;

				var hexVec = new Vector2i(x, y);
				hexData.Set(
					hexVec,
					new Color(
						tile.Level / 255f,
						((int) tile.Biome) / 255f,
						((int) tile.Terrain) / 255f,
						0f
					)
				);
			}
		}

		hexData.Update();

		GD.PrintS($"(Planet) generated world ({watch.ElapsedMilliseconds}ms)");

		foreach (var child in GetChildren()) {
			RemoveChild(child);
		}

		terrainChunkMesh = new TerrainChunkMesh(HexSize, ChunkSizeHexes, 2);

		var layout = new Layout(new Point(hexSize, hexSize), new Point(0, 0));

		watch = System.Diagnostics.Stopwatch.StartNew();

		for (int x = 0; x < ChunkGridSizeHexes.x; x++) {
			for (int y = 0; y < ChunkGridSizeHexes.y; y++) {
				var chunkID = new Vector2i(x, y);
				var chunk = SpawnChunk(chunkID);
				var chunkPosition = layout.HexToPixel(new Hex(chunk.ChunkOriginHexes.x, chunk.ChunkOriginHexes.y)).ToVector();
				chunk.ChunkPosition = chunkPosition;
				chunk.Position = new Vector3(chunkPosition.x, 0, chunkPosition.y);

				for (int i = 0; i < ChunkSizeHexes.x; i++) {
					for (int j = 0; j < ChunkSizeHexes.y; j++) {
						var hex = new Hex(chunk.ChunkOriginHexes.x + i, chunk.ChunkOriginHexes.y + j);
						chunkTiles.Add(chunkID, hexTiles[hex]);
					}
				}
			}
		}

		GD.PrintS($"(Planet) setup chunks ({watch.ElapsedMilliseconds}ms)");
	}

	public MapChunk SpawnChunk(Vector2i chunkID) {
		if (chunks.ContainsKey(chunkID)) {
			return chunks[chunkID];
		}
		// GD.PrintS("Setup chunk", chunkID);
		var terrainChunkScene = ResourceLoader.Load<PackedScene>("res://view/MapView/TerrainChunk.tscn");
		var chunk = terrainChunkScene.Instantiate<MapChunk>();
		chunk.Setup(this);

		chunk.ChunkID = chunkID;
		chunk.ChunkOriginHexes = chunkID * ChunkSizeHexes;
		chunks[chunkID] = chunk;
		AddChild(chunk);
		chunk.terrainChunk.Mesh = terrainChunkMesh;
		// chunk.OnDespawn += () => this.handleChunkDespawn(chunk);
		return chunk;
	}

	private void handleChunkDespawn(MapChunk chunk) {
		RemoveChild(chunks[chunk.ChunkID]);
	}
}
