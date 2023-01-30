using Godot;
using System;

public partial class MapChunk : VisibleOnScreenNotifier3D {
	[Export] public Vector2i ChunkID;
	[Export] public Vector2 ChunkPosition;
	public Vector2i ChunkOriginHexes;
	public ShaderMaterial TerrainMaterial;

	public MeshInstance3D terrainChunk;
	public MeshInstance3D waterChunk;
	private bool hasRendered = false;
	private Planet planet;

	public event Despawn OnDespawn = delegate {};

	public delegate void Despawn();

	public override void _Ready() {
		Connect("screen_entered", Callable.From(() => render()));
		Connect("screen_exited", Callable.From(() => remove()));

		terrainChunk = GetNode<MeshInstance3D>("%TerrainChunk");
		waterChunk = GetNode<MeshInstance3D>("%WaterChunk");
	}

	public void Setup(Planet _planet) {
		planet = _planet;
	}

	private void render() {
		terrainChunk.Show();
		if (hasRendered) {
			return;
		}
		hasRendered = true;
		// GD.PrintS("Render chunk", ChunkPosition);

		Aabb = new AABB(new Vector3(0, 0, 0), new Vector3(planet.ChunkSize.x, 50, planet.ChunkSize.y));

		var material = ResourceLoader.Load<ShaderMaterial>("res://view/MapView/TerrainMaterial.tres").Duplicate() as ShaderMaterial;
		material.SetShaderParameter("chunkPosition", ChunkPosition);
		material.SetShaderParameter("chunkOriginHexes", ChunkOriginHexes);
		material.SetShaderParameter("worldSize", planet.WorldSize);
		material.SetShaderParameter("worldSizeHexes", planet.WorldSizeHexes);
		material.SetShaderParameter("chunkSize", planet.ChunkSize);
		material.SetShaderParameter("chunkSizeHexes", planet.ChunkSizeHexes);
		material.SetShaderParameter("hexSize", planet.HexSize);
		material.SetShaderParameter("hexData", planet.hexData.Texture);
		terrainChunk.MaterialOverride = material;
	}

	private void remove() {
		// GD.PrintS("Remove chunk", ChunkPosition);
		terrainChunk.Show();
		OnDespawn.Invoke();
	}
}
