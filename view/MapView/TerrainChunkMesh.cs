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
	private List<float[]> custom1;
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
		custom1 = new List<float[]>();
		colors = new List<Color>();

		for (int x = 0; x < chunkSizeHexes.x; x++) {
			for (int y = 0; y < chunkSizeHexes.y; y++) {
				var hex = new Hex(x, y);
				var center = layout.HexToPixel(hex).ToVector();

				foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection))) {
					var PL = VectorConvert.ToVector3(center + layout.HexCornerOffset(dir.CornerLeft()).ToVector());
					var PR = VectorConvert.ToVector3(center + layout.HexCornerOffset(dir.CornerRight()).ToVector());
					var p1 = VectorConvert.ToVector3(center);
					var p2 = PL;
					var p3 = PR;
					var p1_2 = p1.Lerp(p2, 0.5f);
					var p1_3 = p1.Lerp(p3, 0.5f);
					var p5 = p2.Lerp(p3, 0.5f);
					var p4 = p5.Lerp(p1, 0.5f);
					var p3_5 = p3.Lerp(p5, 0.5f);
					var p2_5 = p2.Lerp(p5, 0.5f);

					var c1 = new Vector3(0, 0, 1);
					var c1_3 = new Vector3(0, 0, 1);
					var c1_2 = new Vector3(0, 0, 1);
					var c2 = new Vector3(1, 0, 0);
					var c3 = new Vector3(1, 0, 0);
					var c5 = new Vector3(0, 1, 0);
					var c4 = new Vector3(0, 0, 1);
					var c3_5 = c3.Lerp(c5, 0.5f);
					var c2_5 = c2.Lerp(c5, 0.5f);

					addTriangle(subdivisions, hex, dir, p1,    p3, p3_5, p1_3,   c3, c3_5, c1_3,  1);
					addTriangle(subdivisions, hex, dir, p1,    p3_5, p5, p1_3,   c3_5, c5, c1_3,  2);
					addTriangle(subdivisions, hex, dir, p1,    p5, p4, p1_3,     c5, c4, c1_3,    3);
					addTriangle(subdivisions, hex, dir, p1,    p1_3, p4, p1,     c1_3, c4, c1,    4);

					addTriangle(subdivisions, hex, dir, p1,    p2_5, p2, p1_2,   c2_5, c2, c1_2,  5);
					addTriangle(subdivisions, hex, dir, p1,    p5, p2_5, p1_2,   c5, c2_5, c1_2,  6);
					addTriangle(subdivisions, hex, dir, p1,    p5, p1_2, p4,     c5, c1_2, c4,    7);
					addTriangle(subdivisions, hex, dir, p1,    p4, p1_2, p1,     c4, c1_2, c1,    8);
				}
			}
		}

		var arrays = new Godot.Collections.Array();
		arrays.Resize((int) Mesh.ArrayType.Max);
		arrays[(int) Mesh.ArrayType.Vertex] = vertices.ToArray();
		arrays[(int) Mesh.ArrayType.TexUv2] = uv2.ToArray();
		arrays[(int) Mesh.ArrayType.Color] = colors.ToArray();
		arrays[(int) Mesh.ArrayType.Custom0] = custom0.SelectMany(i => i).ToArray();
		arrays[(int) Mesh.ArrayType.Custom1] = custom1.SelectMany(i => i).ToArray();

		AddSurfaceFromArrays(
			Mesh.PrimitiveType.Triangles,
			arrays, 
			null,
			null,
			(int) Mesh.ArrayCustomFormat.RgbaFloat << (int) Mesh.ArrayFormat.FormatCustom0Shift
			| (int) Mesh.ArrayCustomFormat.RgbaFloat << (int) Mesh.ArrayFormat.FormatCustom1Shift
		);
		
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
		Vector3 c3,
		int triangle_id
	) {
		var current = new Vector2i(hex.col, hex.row);
		if (subdivision == 1) {
			// var opposite = VectorConvert.ToVector3(layout.HexToPixel(hex.Neighbor(dir)).ToVector());
			// var adj_left = VectorConvert.ToVector3(layout.HexToPixel(hex.Neighbor(dir.AdjacentLeft())).ToVector());
			// var adj_right = VectorConvert.ToVector3(layout.HexToPixel(hex.Neighbor(dir.AdjacentRight())).ToVector());
			vertices.Add(p3);
			vertices.Add(p2);
			vertices.Add(p1);
			uv2.Add(current);
			uv2.Add(current);
			uv2.Add(current);
			var d3 = p3.DistanceTo(center) / hexSize;
			var d2 = p2.DistanceTo(center) / hexSize;
			var d1 = p1.DistanceTo(center) / hexSize;
			colors.Add(new Color(1f - d3, 0, 0, 0));
			colors.Add(new Color(1f - d2, 0, 0, 0));
			colors.Add(new Color(1f - d1, 0, 0, 0));
			var dir_value = (((int) dir) / 5f);
			custom0.Add(new float[] { c3.x, c3.y, c3.z, dir_value });
			custom0.Add(new float[] { c2.x, c2.y, c2.z, dir_value });
			custom0.Add(new float[] { c1.x, c1.y, c1.z, dir_value });
			custom1.Add(new float[] { triangle_id / 6f, 0f, 0f, 0f });
			custom1.Add(new float[] { triangle_id / 6f, 0f, 0f, 0f });
			custom1.Add(new float[] { triangle_id / 6f, 0f, 0f, 0f });
		} else {
			var p1_2 = p1.Lerp(p2, 0.5f);
			var p1_3 = p1.Lerp(p3, 0.5f);
			var p2_3 = p2.Lerp(p3, 0.5f);

			var c1_2 = c1.Lerp(c2, 0.5f);
			var c1_3 = c1.Lerp(c3, 0.5f);
			var c2_3 = c2.Lerp(c3, 0.5f);
			addTriangle(subdivision - 1, hex, dir, center,  p1, p1_3, p1_2,    c1, c1_3, c1_2,   triangle_id);
			addTriangle(subdivision - 1, hex, dir, center,  p1_2, p1_3, p2_3,  c1_2, c1_3, c2_3, triangle_id);
			addTriangle(subdivision - 1, hex, dir, center,  p1_2, p2_3, p2,    c1_2, c2_3, c2,   triangle_id);
			addTriangle(subdivision - 1, hex, dir, center,  p1_3, p3, p2_3,    c1_3, c3, c2_3,   triangle_id);
		}
	}
}