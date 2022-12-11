using System;
using Godot;

public partial class CubeSphere : ArrayMesh {
	private int gridSize;
	public float radius = 1.0f;

	private Vector3[] vertices;
	private Vector3[] normals;
	private Color[] cubeUV;

	public int GridSize {
		get => gridSize;
		set {
			gridSize = value;
			generate();
		}
	}

	public CubeSphere(int _gridSize) {
		gridSize = _gridSize;
		this.generate();
	}

	private void generate() {
		this.createVertices();
		this.createTriangles();

		GD.PrintS("Vertices count:", vertices.Length);
		GD.PrintS("Normals count:", normals.Length);
		GD.PrintS("UV count:", cubeUV.Length);

	}

	private void createVertices() {
		int cornerVertices = 8;
		int edgeVertices = 12 * this.GridSize - 12;
		int faceVertices = (this.GridSize - 1) * (this.GridSize - 1) * 6;

		this.vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
		this.normals = new Vector3[this.vertices.Length];
		this.cubeUV = new Color[this.vertices.Length];

		int v = 0;
		for (int y = 0; y <= this.GridSize; y++) {
			for (int x = 0; x <= this.GridSize; x++) {
				this.setVertex(v++, x, y, 0);
			}
			for (int z = 1; z <= this.GridSize; z++) {
				this.setVertex(v++, this.GridSize, y, z);
			}
			for (int x = this.GridSize - 1; x >= 0; x--) {
				this.setVertex(v++, x, y, this.GridSize);
			}
			for (int z = this.GridSize - 1; z > 0; z--) {
				this.setVertex(v++, 0, y, z);
			}
		}

		for (int z = 1; z < this.GridSize; z++) {
			for (int x = 1; x < this.GridSize; x++) {
				this.setVertex(v++, x, this.GridSize, z);
			}
		}
		for (int z = 1; z < this.GridSize; z++) {
			for (int x = 1; x < this.GridSize; x++) {
				this.setVertex(v++, x, 0, z);
			}
		}
	}

	private void setVertex(int i, int x, int y, int z) {
		var v = new Vector3(x, y, z) * 2f / this.GridSize - new Vector3(1, 1, 1);
		float x2 = v.x * v.x;
		float y2 = v.y * v.y;
		float z2 = v.z * v.z;
		Vector3 s;

		s.x = v.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
		s.y = v.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
		s.z = v.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);

		this.normals[i] = v.Normalized();
		this.normals[i] = new Vector3(this.normals[i].x, -this.normals[i].y, this.normals[i].z);
		this.vertices[i] = this.normals[i] * this.radius;
		this.cubeUV[i] = Color.Color8((byte) x, (byte) y, (byte) z);
	}

	private void createTriangles() {
		var size = (GridSize * GridSize) * 12;
		var trianglesZ = new int[size];
		var trianglesX = new int[size];
		var trianglesY = new int[size];

		int ring = this.GridSize * 4;

		int tX = 0;
		int tY = 0;
		int tZ = 0;
		int v = 0;

		for (int y = 0; y < GridSize; y++, v++) {
			for (int q = 0; q < GridSize; q++, v++) {
				tZ = setQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
			}
			for (int q = 0; q < GridSize; q++, v++) {
				tX = setQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
			}
			for (int q = 0; q < GridSize; q++, v++) {
				tZ = setQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
			}
			for (int q = 0; q < GridSize - 1; q++, v++) {
				tX = setQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
			}
			tX = setQuad(trianglesX, tX, v, v - ring + 1, v + ring, v + 1);
		}

		tY = this.createTopFace(trianglesY, tY, ring);
		tY = this.createBottomFace(trianglesY, tY, ring);

		createSurface(trianglesX);
		createSurface(trianglesY);
		createSurface(trianglesZ);
	}

	private void createSurface(int[] indices) {
		GD.PrintS("Creating surface with indices count", indices.Length);

		var arrays = new Godot.Collections.Array();
		arrays.Resize((int) Mesh.ArrayType.Max);
		arrays[(int) Mesh.ArrayType.Vertex] = vertices.AsSpan();
		arrays[(int) Mesh.ArrayType.Normal] = normals.AsSpan();
		arrays[(int) Mesh.ArrayType.Color] = cubeUV.AsSpan();
		arrays[(int) Mesh.ArrayType.Index] = indices.AsSpan();
		AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
	}

	private static int setQuad(int[] triangles, int i, int v00, int v10, int v01, int v11) {
		triangles[i] = v00;
		triangles[i + 1] = v01;
		triangles[i + 2] = v10;
		triangles[i + 3] = v10;
		triangles[i + 4] = v01;
		triangles[i + 5] = v11;
		return i + 6;
	}

	private int createTopFace(int[] triangles, int t, int ring) {
		int v = ring * this.GridSize;
		for (int x = 0; x < this.GridSize - 1; x++, v++) {
			t = setQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
		}
		t = setQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

		int vMin = ring * (this.GridSize + 1) - 1;
		int vMid = vMin + 1;
		int vMax = v + 2;

		for (int z = 1; z < this.GridSize - 1; z++, vMin--, vMid++, vMax++) {
			t = setQuad(triangles, t, vMin, vMid, vMin - 1, vMid + this.GridSize - 1);
			for (int x = 1; x < this.GridSize - 1; x++, vMid++) {
				t = setQuad(triangles, t, vMid, vMid + 1, vMid + this.GridSize - 1, vMid + this.GridSize);
			}
			t = setQuad(triangles, t, vMid, vMax, vMid + this.GridSize - 1, vMax + 1);
		}

		int vTop = vMin - 2;
		t = setQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
		for (int x = 1; x < this.GridSize - 1; x++, vTop--, vMid++) {
			t = setQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
		}
		t = setQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);

		return t;
	}

	private int createBottomFace(int[] triangles, int t, int ring) {
		int v = 1;
		int vMid = this.vertices.Length - (this.GridSize - 1) * (this.GridSize - 1);
		t = setQuad(triangles, t, ring - 1, vMid, 0, 1);
		for (int x = 1; x < this.GridSize - 1; x++, v++, vMid++) {
			t = setQuad(triangles, t, vMid, vMid + 1, v, v + 1);
		}
		t = setQuad(triangles, t, vMid, v + 2, v, v + 1);

		int vMin = ring - 2;
		vMid -= this.GridSize - 2;
		int vMax = v + 2;

		for (int z = 1; z < this.GridSize - 1; z++, vMin--, vMid++, vMax++) {
			t = setQuad(triangles, t, vMin, vMid + this.GridSize - 1, vMin + 1, vMid);
			for (int x = 1; x < this.GridSize - 1; x++, vMid++) {
				t = setQuad(triangles, t, vMid + this.GridSize - 1, vMid + this.GridSize, vMid, vMid + 1);
			}
			t = setQuad(triangles, t, vMid + this.GridSize - 1, vMax + 1, vMid, vMax);
		}

		int vTop = vMin - 1;
		t = setQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
		for (int x = 1; x < this.GridSize - 1; x++, vTop--, vMid++) {
			t = setQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
		}
		t = setQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

		return t;
	}
}