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
}
