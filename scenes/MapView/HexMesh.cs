using System;
using Godot;

public class HexMesh : ArrayMesh {
	private MeshBuilder meshBuilder;

	public HexMesh(Layout layout, int subdivisions = 1) {
		var watch = System.Diagnostics.Stopwatch.StartNew();
		meshBuilder = new MeshBuilder();
		var withHeight = (Vector2 vec, float height) => new Vector3(vec.x, height, vec.y);

		for (int x = 0; x < 10; x++) {
			for (int y = 0; y < 10; y++) {
				var origin = layout.HexToPixel(new Hex(x, y)).ToVector();
				var center = new Vector3(origin.x, 0, origin.y) + new Vector3((float) layout.HexSize.x / 2, 0, (float) layout.HexSize.y / 2);
				for (int i = 0; i < 6; i++) {
					var c0 = center + withHeight(layout.HexCornerOffset((HexCorner) i).ToVector(), 0);
					var c1 = center + withHeight(layout.HexCornerOffset((HexCorner) i + 1).ToVector(), 0);
					subdivide(subdivisions, c1, c0, center);
				}
			}
		}
		meshBuilder.CreateSurface();
		AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, meshBuilder.surface);
		GD.PrintS($"(HexMesh) triangles count: {meshBuilder.triangles.Count}");
		GD.PrintS($"(HexMesh) built in {watch.ElapsedMilliseconds}ms");
	}

	private void subdivide(int levels, Vector3 p1, Vector3 p2, Vector3 p3) {
		if (levels == 0) {
			meshBuilder.AddTriangle(p1, p2, p3);
			meshBuilder.AddTriangleColor(new Color("#f55442"));
			meshBuilder.AddTriangleUV(new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0));
			return;
		}
		var c1 = p1.LinearInterpolate(p2, 0.5f);
		var c2 = p1.LinearInterpolate(p3, 0.5f);
		var c3 = p2.LinearInterpolate(p3, 0.5f);
		subdivide(levels - 1, p1, c1, c2);
		subdivide(levels - 1, c1, c3, c2);
		subdivide(levels - 1, c1, p2, c3);
		subdivide(levels - 1, c2, c3, p3);
	}
}
