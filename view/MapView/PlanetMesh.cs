using Godot;
using System;

public partial class PlanetMesh : Node3D {
	public override void _Ready(){
		var planetData = new PlanetData(123, 100, 50);

		foreach (var child in GetChildren()) {
			if (child is PlanetMeshFace face) {
				face.GenerateMesh(planetData);
			}
		}
	}
}
