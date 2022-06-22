using Godot;
using Godot.Collections;
using System;

public class TerrainChunk : MultiMeshInstance {
	public override void _Ready() {
		GD.PrintS("(TerrainChunk) render");
		var watch = System.Diagnostics.Stopwatch.StartNew();

		// wireframe mode:
		// VisualServer.SetDebugGenerateWireframes(true);
		// GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe;

		var layout = new Layout(new Point(32, 32), new Point(0, 0));
		var hexMesh = new HexMesh(layout, 6);
		
		Multimesh = new MultiMesh();
		Multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3d;
		Multimesh.CustomDataFormat = MultiMesh.CustomDataFormatEnum.Float;
		Multimesh.Mesh = hexMesh;
		Multimesh.InstanceCount = 100;
		Multimesh.VisibleInstanceCount = 100;
	
		int i = 0;
		for (int x = 0; x < 10; x++) {
			for (int y = 0; y < 10; y++) {
				var p = layout.HexToPixel(new Hex(x, y)).ToVector();
				Multimesh.SetInstanceTransform(i, new Transform(Basis.Identity, new Vector3(p.x, 10, p.y)));
				Multimesh.SetInstanceCustomData(i, new Color(p.x, p.y, 0, 0));
				GD.PrintS(i, p);
				i++;
			}
		}
		GD.PrintS($"(TerrainChunk) rendered in {watch.ElapsedMilliseconds}ms");
	}
}
