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

		var center = planet.WorldSize / 2;
		camera.Position = new Vector3(center.x, camera.Position.y, center.y);
		light.Position = new Vector3(center.x, light.Position.y, center.y);
	}

	public override void _Process(double delta) {
		base._Process(delta);

		// if (!(planet is null)) {
		// 	var cameraVec = GetViewport().GetCamera3d().Position;
		// 	var cameraPos = new Vector2(cameraVec.x, cameraVec.z);
		// 	if (cameraPos.x < 0) {
		// 		cameraPos = planet.WorldSize - (cameraPos.Abs() % planet.WorldSize);
		// 	} else {
		// 		cameraPos = cameraPos % planet.WorldSize;
		// 	}
		// 	var chunkAtCamera = (cameraPos / planet.WorldSize) * planet.ChunkGridSizeHexes;
		// 	chunkAtCamera = chunkAtCamera.Floor();
		// 	if (
		// 		chunkAtCamera.y >= 0 && chunkAtCamera.y < planet.ChunkGridSizeHexes.y
		// 		&& chunkAtCamera.x >= 0 && chunkAtCamera.x < planet.ChunkGridSizeHexes.x
		// 	) {
		// 		GD.PrintS(cameraPos, chunkAtCamera);
		// 	} else {
		// 		GD.PrintS("Not over chunk");
		// 	}
		// }
	}
}
