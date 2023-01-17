using Godot;
using System;
using System.Linq;


public partial class TerrainChunkMesh : ArrayMesh {
	private readonly float hexSize;
	private readonly Vector2i chunkSizeHexes;
	private Layout layout;
	private List<Vector3> vertices;
	private List<Vector2> uv2;
	private List<float[]> custom0;
	private List<Color> colors;

	public TerrainChunkMesh(
		float hexSize,
		Vector2i chunkSizeHexes,
		int subdivisions = 1
	) {
		var watch = System.Diagnostics.Stopwatch.StartNew();
		this.hexSize = hexSize;
		this.chunkSizeHexes = chunkSizeHexes;

		layout = new Layout(new Point(hexSize, hexSize), new Point(0, 0));

		vertices = new List<Vector3>();
		uv2 = new List<Vector2>();
		custom0 = new List<float[]>();
		colors = new List<Color>();

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
						hex,
						dir,
						center3,

						center3, // center vertex
						new Vector3(PR.x, 0, PR.y), // left adjacent corner vertex
						new Vector3(PL.x, 0, PL.y), // right adjacent corner vertex

						// in the form (Left, Right, Center)
						new Vector3(0, 0, 1), // center point
						new Vector3(1, 0, 0), // Right
						new Vector3(0, 1, 0) // Left
					);
				}
			}
		}

		var arrays = new Godot.Collections.Array();
		arrays.Resize((int) Mesh.ArrayType.Max);
		arrays[(int) Mesh.ArrayType.Vertex] = vertices.ToArray();
		arrays[(int) Mesh.ArrayType.TexUv2] = uv2.ToArray();
		arrays[(int) Mesh.ArrayType.Color] = colors.ToArray();
		arrays[(int) Mesh.ArrayType.Custom0] = custom0.SelectMany(i => i).ToArray();

		AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays, null, null, (int) Mesh.ArrayCustomFormat.RgbaFloat << (int) Mesh.ArrayFormat.FormatCustom0Shift);
		
		GD.PrintS($"(TerrainChunkMesh) generated terrain mesh in {watch.ElapsedMilliseconds}ms");
	}

	public void addTriangle(
		int subdivision,
		Hex hex,
		HexDirection dir,
		Vector3 center,
		Vector3 p1,
		Vector3 p2,
		Vector3 p3,
		Vector3 c1,
		Vector3 c2,
		Vector3 c3
	) {
		var current = new Vector2i(hex.col, hex.row);
		if (subdivision == 1) {
			var opposite = VectorConvert.ToVector3(layout.HexToPixel(hex.Neighbor(dir)).ToVector());
			var adj_left = VectorConvert.ToVector3(layout.HexToPixel(hex.Neighbor(dir.AdjacentLeft())).ToVector());
			var adj_right = VectorConvert.ToVector3(layout.HexToPixel(hex.Neighbor(dir.AdjacentRight())).ToVector());
			vertices.Add(p3);
			vertices.Add(p2);
			vertices.Add(p1);
			uv2.Add(current);
			uv2.Add(current);
			uv2.Add(current);
			var d3 = p3.DistanceTo(center) / hexSize;
			var d2 = p2.DistanceTo(center) / hexSize;
			var d1 = p1.DistanceTo(center) / hexSize;
			colors.Add(new Color(d3, 0, 0, 0));
			colors.Add(new Color(d2, 0, 0, 0));
			colors.Add(new Color(d1, 0, 0, 0));
			var dir_value = (((int) dir) / 5f);
			custom0.Add(new float[] { c3.x, c3.y, c3.z, dir_value });
			custom0.Add(new float[] { c2.x, c2.y, c2.z, dir_value });
			custom0.Add(new float[] { c1.x, c1.y, c1.z, dir_value });

		} else {
			var p1_2 = p1.Lerp(p2, 0.5f);
			var p1_3 = p1.Lerp(p3, 0.5f);
			var p2_3 = p2.Lerp(p3, 0.5f);

			var c1_2 = c1.Lerp(c2, 0.5f);
			var c1_3 = c1.Lerp(c3, 0.5f);
			var c2_3 = c2.Lerp(c3, 0.5f);
			addTriangle(subdivision - 1, hex, dir, center, p1, p1_3, p1_2, c1, c1_3, c1_2);
			addTriangle(subdivision - 1, hex, dir, center, p1_2, p1_3, p2_3, c1_2, c1_3, c2_3);
			addTriangle(subdivision - 1, hex, dir, center, p1_2, p2_3, p2, c1_2, c2_3, c2);
			addTriangle(subdivision - 1, hex, dir, center, p1_3, p3, p2_3, c1_3, c3, c2_3);
		}
	}
}