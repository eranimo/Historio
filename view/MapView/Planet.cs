using Godot;
using System;

public partial class Planet : MeshInstance3D {
	public override void _Ready() {
		// RenderingServer.SetDebugGenerateWireframes(true);
		// GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe;

		var watch = System.Diagnostics.Stopwatch.StartNew();
		Mesh = new CubeSphere(250);
		GD.PrintS($"Building Planet mesh: {watch.ElapsedMilliseconds}ms");
	}
}
