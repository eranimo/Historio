using Godot;
using System;

public enum PlanetSize {
	Small = 50,
	Medium = 150,
	Large = 250
}

[Tool]
public partial class Planet : MeshInstance3D {
	private int seed = 123;
	private PlanetSize planetSize = PlanetSize.Small;
	private float radius = 1.0f;
	private float waterHeight = 0.5f;
	private MeshInstance3D water;
	private PlanetShaderMaterial material;

	[Export(PropertyHint.Enum)]
	public PlanetSize PlanetSize {
		get => planetSize;
		set {
			planetSize = value;
			generateMesh();
		}
	}

	[Export]
	public int Seed {
		get => seed;
		set {
			seed = value;
			generateMesh();
		}
	}

	[Export(PropertyHint.Range, "1.0,10.0")]
	public float Radius { get => radius; set { radius = value; generateMesh(); } }

	[Export(PropertyHint.Range, "0.0,1.0")]
	public float WaterHeight { get => waterHeight; set { waterHeight = value; generateWater(); } }

	public override void _Ready() {
		// RenderingServer.SetDebugGenerateWireframes(true);
		// GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe;

		var watch = System.Diagnostics.Stopwatch.StartNew();
		GD.PrintS($"(Planet) Generate: {watch.ElapsedMilliseconds}ms");
		generateMesh();
		generateWater();
	}

	private void generateMesh() {
		var watch = System.Diagnostics.Stopwatch.StartNew();
		Mesh = new CubeSphere((int) PlanetSize, radius);
		material = ResourceLoader.Load<PlanetShaderMaterial>("res://view/MapView/PlanetShaderMaterial.tres");
		var height = (int) PlanetSize;
		var width = height * 2;
		material.GenerateTerrain(width, height, seed, waterHeight);
		material.GenerateTexture(width, height, seed, waterHeight);
		MaterialOverride = material;
		GD.PrintS($"\tBuilding Planet mesh: {watch.ElapsedMilliseconds}ms");
	}

	private void generateWater() {
		var watch = System.Diagnostics.Stopwatch.StartNew();
		water = GetNode<MeshInstance3D>("Water");
		var height = (int) PlanetSize;
		var width = height * 2;
		material.GenerateTexture(width, height, seed, waterHeight);
		var waterMesh = new SphereMesh();
		var waterHeightFixed = radius + waterHeight;
		waterMesh.Radius = waterHeightFixed;
		waterMesh.Height = waterHeightFixed * 2;
		water.Mesh = waterMesh;

		GD.PrintS($"\tBuilding Planet water: {watch.ElapsedMilliseconds}ms");
	}
}
