using Godot;
using System;

public enum PlanetSize {
	Small = 50,
	Medium = 150,
	Large = 250
}

[Tool]
public partial class Planet : MeshInstance3D {
	private PlanetSize planetSize = PlanetSize.Small;

	[Export(PropertyHint.Enum)]
	public PlanetSize PlanetSize {
		get => planetSize;
		set {
			planetSize = value;
			generate();
		}
	}

	public override void _Ready() {
		// RenderingServer.SetDebugGenerateWireframes(true);
		// GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe;

		var watch = System.Diagnostics.Stopwatch.StartNew();
		generate();
		GD.PrintS($"(Planet) Generate: {watch.ElapsedMilliseconds}ms");
	}

	private void generate() {
		var watch = System.Diagnostics.Stopwatch.StartNew();
		Mesh = new CubeSphere((int) PlanetSize);
		GD.PrintS($"\tBuilding Planet mesh: {watch.ElapsedMilliseconds}ms");
	}
}
