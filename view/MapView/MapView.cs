using Godot;
using System;

public partial class MapView : Node3D {
	private Planet planet;
	private Camera3D camera;
	private OmniLight3D light;

	public override void _Ready() {
		planet = GetNode<Planet>("Planet");
		camera = GetNode<Camera3D>("Camera");
		light = GetNode<OmniLight3D>("Light");

		planet.Generate();

		var center = planet.PlanetData.WorldSize / 2;
		camera.Position = new Vector3(center.x, camera.Position.y, center.y);
		light.Position = new Vector3(center.x, light.Position.y, center.y);
	}

	public override void _Process(double delta) {
		base._Process(delta);

		if (!(planet is null)) {
			var cameraVec = GetViewport().GetCamera3d().Position;
			var cameraPos = new Vector2(cameraVec.x, cameraVec.z);
			if (cameraPos.x < 0) {
				cameraPos = planet.PlanetData.WorldSize - (cameraPos.Abs() % planet.PlanetData.WorldSize);
			} else {
				cameraPos = cameraPos % planet.PlanetData.WorldSize;
			}
			var chunkAtCamera = (cameraPos / planet.PlanetData.WorldSize) * planet.PlanetData.ChunkGridSize;
			chunkAtCamera = chunkAtCamera.Floor();
			if (
				chunkAtCamera.y >= 0 && chunkAtCamera.y < planet.PlanetData.ChunkGridSize.y
				&& chunkAtCamera.x >= 0 && chunkAtCamera.x < planet.PlanetData.ChunkGridSize.x
			) {
				GD.PrintS(cameraPos, chunkAtCamera);
			} else {
				GD.PrintS("Not over chunk");
			}
		}
	}
}
