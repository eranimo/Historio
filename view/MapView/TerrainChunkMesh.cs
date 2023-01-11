using Godot;
using System;
using System.Linq;


public partial class TerrainChunkMesh : ArrayMesh {
	private readonly float hexSize;
	private readonly Vector2i chunkSizeHexes;
	private List<Vector3> vertices;
	private List<Vector2> uv2;

	public TerrainChunkMesh(
		float hexSize,
		Vector2i chunkSizeHexes,
		int subdivisions = 1
	) {
		var watch = System.Diagnostics.Stopwatch.StartNew();
		this.hexSize = hexSize;
		this.chunkSizeHexes = chunkSizeHexes;

		var layout = new Layout(new Point(hexSize, hexSize), new Point(0, 0));

		vertices = new List<Vector3>();
		uv2 = new List<Vector2>();

		for (int x = 0; x < chunkSizeHexes.x; x++) {
			for (int y = 0; y < chunkSizeHexes.y; y++) {
				var hex = new Hex(x, y);
				var center = layout.HexToPixel(hex).ToVector();

				foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection))) {
					var PL = center + layout.HexCornerOffset(dir.CornerLeft()).ToVector();
					var PR = center + layout.HexCornerOffset(dir.CornerRight()).ToVector();
					var center3 = new Vector3(center.x, 0, center.y);
					addTriangle(
						subdivisions,
						new Vector2i(x, y),
						center3,
						new Vector3(PR.x, 0, PR.y),
						new Vector3(PL.x, 0, PL.y)
					);
				}
			}
		}

		var arrays = new Godot.Collections.Array();
		arrays.Resize((int) Mesh.ArrayType.Max);
		arrays[(int) Mesh.ArrayType.Vertex] = vertices.ToArray();
		arrays[(int) Mesh.ArrayType.TexUv2] = uv2.ToArray();

		AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

		GD.PrintS($"(TerrainChunkMesh) generated terrain mesh in {watch.ElapsedMilliseconds}ms");
	}

	public void addTriangle(int subdivision, Vector2i offset, Vector3 p1, Vector3 p2, Vector3 p3) {
		if (subdivision == 1) {
			vertices.Add(p3);
			vertices.Add(p2);
			vertices.Add(p1);
			uv2.Add(offset);
			uv2.Add(offset);
			uv2.Add(offset);
		} else {
			var p1_2 = p1.Lerp(p2, 0.5f);
			var p1_3 = p1.Lerp(p3, 0.5f);
			var p2_3 = p2.Lerp(p3, 0.5f);
			addTriangle(subdivision - 1, offset, p1, p1_3, p1_2);
			addTriangle(subdivision - 1, offset, p1_2, p1_3, p2_3);
			addTriangle(subdivision - 1, offset, p1_2, p2_3, p2);
			addTriangle(subdivision - 1, offset, p1_3, p3, p2_3);
		}
	}
}