using Godot;
using System;

public partial class MapChunk : VisibleOnScreenNotifier3D {
	[Export] public Vector2 ChunkPosition;
	[Export] public PlanetData PlanetData;
	public ShaderMaterial TerrainMaterial;

	public MeshInstance3D terrainChunk;
	private MeshInstance3D waterChunk;
	private bool hasRendered = false;

	public override void _Ready() {
		Connect("screen_entered", Callable.From(() => render()));
		Connect("screen_exited", Callable.From(() => remove()));

		terrainChunk = GetNode<MeshInstance3D>("%TerrainChunk");
		waterChunk = GetNode<MeshInstance3D>("%WaterChunk");
	}

	private void render() {
		terrainChunk.Show();
		if (hasRendered) {
			return;
		}
		hasRendered = true;
		// GD.PrintS("Render chunk", ChunkPosition);

		setup();
	}

	private void remove() {
		// GD.PrintS("Remove chunk", ChunkPosition);
		terrainChunk.Show();
	}

	private void setup() {
		(terrainChunk.Mesh as PlaneMesh).Size = PlanetData.ChunkSize;
		(waterChunk.Mesh as PlaneMesh).Size = PlanetData.ChunkSize;
		var material = ResourceLoader.Load<ShaderMaterial>("res://view/MapView/TerrainMaterial.tres").Duplicate() as ShaderMaterial;
		material.SetShaderParameter("chunkPosition", ChunkPosition);
		material.SetShaderParameter("worldSize", PlanetData.WorldSize);
		material.SetShaderParameter("chunkSize", PlanetData.ChunkSize);
		material.SetShaderParameter("heightmap", PlanetData.Heightmap);
		material.SetShaderParameter("splatmap", PlanetData.Splatmap);
		terrainChunk.MaterialOverride = material;
	}
}
