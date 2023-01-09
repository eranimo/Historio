using Godot;
using System;

public partial class MapChunk : VisibleOnScreenNotifier3D {
	[Export] public Vector2i ChunkID;
	[Export] public Vector2 ChunkPosition;
	[Export] public PlanetData PlanetData;
	public ShaderMaterial TerrainMaterial;

	public MeshInstance3D terrainChunk;
	private MeshInstance3D waterChunk;
	private bool hasRendered = false;

	public event Despawn OnDespawn = delegate {};

	public delegate void Despawn();

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
		OnDespawn.Invoke();
	}

	private void setup() {
		Aabb = new AABB(new Vector3(0, 0, 0), new Vector3(PlanetData.ChunkSize.x, 50, PlanetData.ChunkSize.y));
		(terrainChunk.Mesh as PlaneMesh).Size = PlanetData.ChunkSize;
		(terrainChunk.Mesh as PlaneMesh).CenterOffset = new Vector3(PlanetData.ChunkSize.x / 2f, 0, PlanetData.ChunkSize.y / 2f);
		(waterChunk.Mesh as PlaneMesh).Size = PlanetData.ChunkSize;
		(waterChunk.Mesh as PlaneMesh).CenterOffset = new Vector3(PlanetData.ChunkSize.x / 2f, 0, PlanetData.ChunkSize.y / 2f);
		var material = ResourceLoader.Load<ShaderMaterial>("res://view/MapView/TerrainMaterial.tres").Duplicate() as ShaderMaterial;
		material.SetShaderParameter("chunkPosition", ChunkPosition);
		material.SetShaderParameter("worldSize", PlanetData.WorldSize);
		material.SetShaderParameter("chunkSize", PlanetData.ChunkSize);
		material.SetShaderParameter("heightmap", PlanetData.Heightmap);
		material.SetShaderParameter("splatmap", PlanetData.Splatmap);
		terrainChunk.MaterialOverride = material;
	}
}
