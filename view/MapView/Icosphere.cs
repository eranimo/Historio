using System;
using Godot;

public class ProceduralShape {
	public struct Face {
		public int v1;
		public int v2;
		public int v3;

		public Face(int v1, int v2, int v3) {
			this.v1 = v1;
			this.v2 = v2;
			this.v3 = v3;
		}
	}

	public class GeometryData {
		public List<Vector3> Vertices = new List<Vector3>();
		public List<int> FaceIndices = new List<int>();
		public List<Face> Faces = new List<Face>();
	}

	private GeometryData geometry = new GeometryData();
	private int index = 0;

	public GeometryData Geometry { get => geometry; set => geometry = value; }

	// add vertex to mesh, fix position to be on unit sphere, return index
	protected int addVertex(Vector3 p) {
		var length = Mathf.Sqrt(p.x * p.x + p.y * p.y + p.z * p.z);
		Geometry.Vertices.Add(new Vector3(p.x / length, p.y / length, p.z / length));
		return index++;
	}

	// return index of point in the middle of p1 and p2
	protected int getMiddlePoint(int p1, int p2) {
		Vector3 point1 = this.Geometry.Vertices[p1];
		Vector3 point2 = this.Geometry.Vertices[p2];
		Vector3 middle = new Vector3(
			(point1.x + point2.x) / 2f,
			(point1.y + point2.y) / 2f,
			(point1.z + point2.z) / 2f
		);
		int i = addVertex(middle);
		return i;
	}
}

// adapted from http://blog.andreaskahler.com/2009/06/creating-icosphere-mesh-in-code.html
public class Icosphere : ProceduralShape {
	public Icosphere(int recursionLevel) {
		// create 12 vertices of a icosahedron
		var t = (1f + Mathf.Sqrt(5f)) / 2f;

		addVertex(new Vector3(-1, t, 0));
		addVertex(new Vector3(1, t, 0));
		addVertex(new Vector3(-1, -t, 0));
		addVertex(new Vector3(1, -t, 0));

		addVertex(new Vector3(0, -1, t));
		addVertex(new Vector3(0, 1, t));
		addVertex(new Vector3(0, -1, -t));
		addVertex(new Vector3(0, 1, -t));

		addVertex(new Vector3(t, 0, -1));
		addVertex(new Vector3(t, 0, 1));
		addVertex(new Vector3(-t, 0, -1));
		addVertex(new Vector3(-t, 0, 1));


		// create 20 triangles of the icosahedron
		Geometry.Faces = new List<Face>();

		// 5 faces around point 0
		Geometry.Faces.Add(new Face(0, 11, 5));
		Geometry.Faces.Add(new Face(0, 5, 1));
		Geometry.Faces.Add(new Face(0, 1, 7));
		Geometry.Faces.Add(new Face(0, 7, 10));
		Geometry.Faces.Add(new Face(0, 10, 11));

		// 5 adjacent faces 
		Geometry.Faces.Add(new Face(1, 5, 9));
		Geometry.Faces.Add(new Face(5, 11, 4));
		Geometry.Faces.Add(new Face(11, 10, 2));
		Geometry.Faces.Add(new Face(10, 7, 6));
		Geometry.Faces.Add(new Face(7, 1, 8));

		// 5 faces around point 3
		Geometry.Faces.Add(new Face(3, 9, 4));
		Geometry.Faces.Add(new Face(3, 4, 2));
		Geometry.Faces.Add(new Face(3, 2, 6));
		Geometry.Faces.Add(new Face(3, 6, 8));
		Geometry.Faces.Add(new Face(3, 8, 9));

		// 5 adjacent faces 
		Geometry.Faces.Add(new Face(4, 9, 5));
		Geometry.Faces.Add(new Face(2, 4, 11));
		Geometry.Faces.Add(new Face(6, 2, 10));
		Geometry.Faces.Add(new Face(8, 6, 7));
		Geometry.Faces.Add(new Face(9, 8, 1));


		// refine triangles
		for (int i = 0; i < recursionLevel; i++) {
			var faces2 = new List<Face>();
			foreach (var tri in Geometry.Faces) {
				// replace triangle by 4 triangles
				int a = getMiddlePoint(tri.v1, tri.v2);
				int b = getMiddlePoint(tri.v2, tri.v3);
				int c = getMiddlePoint(tri.v3, tri.v1);

				faces2.Add(new Face(tri.v1, a, c));
				faces2.Add(new Face(tri.v2, b, a));
				faces2.Add(new Face(tri.v3, c, b));
				faces2.Add(new Face(a, b, c));
			}
			Geometry.Faces = faces2;
		}

		// done, now add triangles to mesh
		foreach (var tri in Geometry.Faces) {
			Geometry.FaceIndices.Add(tri.v1);
			Geometry.FaceIndices.Add(tri.v2);
			Geometry.FaceIndices.Add(tri.v3);
		}
	}
}

public class Hexsphere : ProceduralShape {
	public class Cell {
		public List<Face> Faces = new List<Face>();
		public HashSet<Cell> Neighbors = new HashSet<Cell>();
		public List<Vector3> Edges = new List<Vector3>();
	}

	public List<Cell> Cells = new List<Cell>();

	public Hexsphere(Icosphere icosphere) {
		var vertexToFaces = new Dictionary<Vector3, List<Face>>();
		var faceCentroids = new Dictionary<Face, int>();
		var cellsOnPoint = new MultiSet<Vector3, Cell>();
		foreach (var vertex in icosphere.Geometry.Vertices) {
			vertexToFaces[vertex] = new List<Face>();
		}
		foreach (var face in icosphere.Geometry.Faces) {
			var A = icosphere.Geometry.Vertices[face.v1];
			var B = icosphere.Geometry.Vertices[face.v2];
			var C = icosphere.Geometry.Vertices[face.v3];
			vertexToFaces[A].Add(face);
			vertexToFaces[B].Add(face);
			vertexToFaces[C].Add(face);
			var centroid = (A + B + C) / 3f;
			faceCentroids[face] = addVertex(centroid);
		}

		foreach (var vertex in icosphere.Geometry.Vertices) {
			var vertexIndex = addVertex(vertex);
			var cell = new Cell();
			foreach (var face in vertexToFaces[vertex]) {
				Vector3 B;
				Vector3 C;

				if (vertex == icosphere.Geometry.Vertices[face.v1]) {
					B = icosphere.Geometry.Vertices[face.v2];
					C = icosphere.Geometry.Vertices[face.v3];
				} else if (vertex == icosphere.Geometry.Vertices[face.v2]) {
					B = icosphere.Geometry.Vertices[face.v1];
					C = icosphere.Geometry.Vertices[face.v3];
				} else {
					B = icosphere.Geometry.Vertices[face.v1];
					C = icosphere.Geometry.Vertices[face.v2];
				}
				
				var midAB = vertex.Lerp(B, 0.5f);
				var midAC = vertex.Lerp(C, 0.5f);
				var midABi = addVertex(midAB);
				var midACi = addVertex(midAC);
				var centroidIndex = faceCentroids[face];
				var centroid = Geometry.Vertices[centroidIndex];
				cellsOnPoint.Add(centroid, cell);
				var f1 = new Face(vertexIndex, midABi, centroidIndex);
				var f2 = new Face(vertexIndex, centroidIndex, midACi);
				Geometry.Faces.Add(f1);
				Geometry.Faces.Add(f2);
				cell.Edges.Add(centroid);
				cell.Faces.Add(f1);
				cell.Faces.Add(f2);
			}
			Cells.Add(cell);
		}

		foreach (var tri in Geometry.Faces) {
			Geometry.FaceIndices.Add(tri.v1);
			Geometry.FaceIndices.Add(tri.v2);
			Geometry.FaceIndices.Add(tri.v3);
		}

		// find neighbors
		foreach (var cell in Cells) {
			foreach (var edge in cell.Edges) {
				foreach (var c in cellsOnPoint[edge]) {
					cell.Neighbors.Add(c);
				}
			}
			cell.Neighbors.Remove(cell);
		}
	}
}