using Godot;
using Godot.Collections;
using System;

public class MeshBuilder {
	public List<Vector3> vertices;
	public List<Vector2> uvs;
	public List<Vector3> normals;
	public List<Color> colors;
	public List<int> triangles;

	public readonly bool useUVCoordinates;

	public Godot.Collections.Array surface;

	public MeshBuilder(bool useUVCoordinates = false) {
		Clear();
		this.useUVCoordinates = useUVCoordinates;
	}

	public void Clear() {
		vertices = new List<Vector3>();
		uvs = new List<Vector2>();
		normals = new List<Vector3>();
		colors = new List<Color>();
		triangles = new List<int>();
	}

	public void CreateSurface() {
		var indexArray = triangles.ToArray();
		var uvArray = uvs.ToArray();
		var colorArray = colors.ToArray();
		var vertexArray = vertices.ToArray();

		var arrays = new Godot.Collections.Array();
		arrays.Resize((int) ArrayMesh.ArrayType.Max);
		arrays[(int) ArrayMesh.ArrayType.Index] = indexArray;
		arrays[(int) ArrayMesh.ArrayType.Vertex] = vertexArray;
		if (useUVCoordinates) {
			arrays[(int) ArrayMesh.ArrayType.TexUv] = uvArray;
		}
		if (colors.Count > 0) {
			arrays[(int) ArrayMesh.ArrayType.Color] = colorArray;
		}

		surface = arrays;
	}

	public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
		int vertexIndex = vertices.Count;
		vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
	}

	public void AddTriangleColor(Color c1, Color c2, Color c3) {
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c3);
	}

	public void AddTriangleColor(Color color) {
		colors.Add(color);
		colors.Add(color);
		colors.Add(color);
	}

	public void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
		int vertexIndex = vertices.Count;
		vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);
		vertices.Add(v4);
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 2);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
		triangles.Add(vertexIndex + 3);
	}

	public void AddQuadColor(Color c1, Color c2) {
		colors.Add(c1);
		colors.Add(c1);
		colors.Add(c2);
		colors.Add(c2);
	}

	public void AddQuadColor(Color color) {
		colors.Add(color);
		colors.Add(color);
		colors.Add(color);
		colors.Add(color);
	}

	public void AddTriangleUV(Vector2 uv1, Vector2 uv2, Vector2 uv3) {
		uvs.Add(uv1);
		uvs.Add(uv2);
		uvs.Add(uv3);
	}

	public void AddQuadUV(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4) {
		uvs.Add(uv1);
		uvs.Add(uv2);
		uvs.Add(uv3);
		uvs.Add(uv4);
	}

	public void AddQuadUV(float uMin, float uMax, float vMin, float vMax) {
		uvs.Add(new Vector2(uMin, vMin));
		uvs.Add(new Vector2(uMax, vMin));
		uvs.Add(new Vector2(uMin, vMax));
		uvs.Add(new Vector2(uMax, vMax));
	}
}

public class TerrainChunk : MeshInstance {
	private MeshBuilder meshBuilder;

	public override void _Ready() {
		GD.PrintS("(TerrainChunk) render");
		var watch = System.Diagnostics.Stopwatch.StartNew();

		VisualServer.SetDebugGenerateWireframes(true);
		GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe;

		var layout = new Layout(new Point(16.666, 16.165), new Point(16 + .5, 18 + .5));

		meshBuilder = new MeshBuilder();
		var withHeight = (Vector2 vec, float height) => new Vector3(vec.x, height, vec.y);

		var center = new Vector3((float) layout.HexSize.x / 2, 0, (float) layout.HexSize.y / 2);
		for (int i = 0; i < 6; i++) {
			var c0 = center + withHeight(layout.HexCornerOffset(i).ToVector(), 0);
			var c1 = center + withHeight(layout.HexCornerOffset(i + 1).ToVector(), 0);
			subdivide(6, c0, c1, center);
		}
		meshBuilder.CreateSurface();

		var arrayMesh = new ArrayMesh();
		arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, meshBuilder.surface);
		Mesh = arrayMesh;
		GD.PrintS($"(TerrainChunk) Triangles: {meshBuilder.triangles.Count}");
		GD.PrintS($"(TerrainChunk) rendered in {watch.ElapsedMilliseconds}ms");
	}

	private void subdivide(int levels, Vector3 p1, Vector3 p2, Vector3 p3) {
		if (levels == 0) {
			meshBuilder.AddTriangle(p1, p2, p3);
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
