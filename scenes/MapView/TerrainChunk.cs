using Godot;
using Godot.Collections;
using System;

public enum TerrainType {
	Ocean,
	Coast,
	Plains,
	Hills,
	Mountains,
}

public class TerrainData {
	public TerrainType terrainType;
}

public class TerrainChunk : MeshInstance {
	private Layout layout;
	private WorldNoise terrain;
	private Image heightmap;

	private readonly int CHUNK_ROWS = 5;
	private readonly int CHUNK_COLS = 5;

	public override void _Ready() {
		GD.PrintS("(TerrainChunk) render");
		var watch = System.Diagnostics.Stopwatch.StartNew();

		// wireframe mode:
		// VisualServer.SetDebugGenerateWireframes(true);
		// GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe;

		layout = new Layout(new Point(16, 16), new Point(0, 0));
		var chunkSize = layout.HexToPixel(new Hex(CHUNK_COLS, CHUNK_ROWS)) + new Point(layout.HexSize.x, ((CHUNK_ROWS % 2 == 0) ? 1.5 : 1) * layout.HexSize.y);
		var imageWidth = (int) Math.Ceiling(chunkSize.x);
		var imageHeight = (int) Math.Ceiling(chunkSize.y);
		var mat = ResourceLoader.Load<ShaderMaterial>("res://scenes/MapView/TerrainMaterial.tres");
		var planeMesh = new PlaneMesh();
		planeMesh.Size = new Vector2(imageWidth, imageHeight);
		planeMesh.SubdivideDepth = 200;
		planeMesh.SubdivideWidth = 200;
		planeMesh.CenterOffset = new Vector3(imageWidth / 2, 0, imageHeight / 2);
		Mesh = planeMesh;
		Mesh.SurfaceSetMaterial(0, mat);
		
		heightmap = new Image();
		GD.PrintS("Chunk image size:", imageWidth, imageHeight);
		heightmap.Create(imageWidth, imageHeight, false, Image.Format.Rgb8);

		terrain = new WorldNoise(100, 100, 123);
		heightmap.Lock();
		for (int x = 0; x < imageWidth; x++) {
			for (int y = 0; y < imageHeight; y++) {
				var hex = layout.PixelToHex(new Point(x, y) - layout.HexSize / 2f);
				if (hex.col < 0 || hex.col > CHUNK_COLS || hex.row < 0 || hex.row > CHUNK_ROWS) {
					heightmap.SetPixel(x, y, new Color(0f, 0f, 0f));
				} else {
					var h = terrain.Get(hex.col, hex.row);
					heightmap.SetPixel(x, y, new Color(h, h, h));
				}
			}
		}
		heightmap.Unlock();

		var heightmapTexture = new ImageTexture();
		heightmapTexture.CreateFromImage(heightmap);
		((TextureRect) GetNode("Heightmap")).Texture = heightmapTexture;
		mat.SetShaderParam("displacement", heightmapTexture);

		GD.PrintS($"(TerrainChunk) rendered in {watch.ElapsedMilliseconds}ms");
	}
}
