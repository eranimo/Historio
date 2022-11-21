using Godot;
using System;
using System.Linq;
using MIConvexHull;

public struct SphericalCoordinate {
	public float Radius;
	public float Polar;
	public float Elevation;

	public SphericalCoordinate(float radius, float polar, float elevation) {
		this.Radius = radius;
		this.Polar = polar;
		this.Elevation = elevation;
	}
}

public static class CoordinateConversion {
	public static Vector3 SphericalToCartesian(SphericalCoordinate spherical) {
		float a = spherical.Radius * Mathf.Cos(spherical.Elevation);
		var outCart = new Vector3();
		outCart.x = a * Mathf.Cos(spherical.Polar);
		outCart.y = spherical.Radius * Mathf.Sin(spherical.Elevation);
		outCart.z = a * Mathf.Sin(spherical.Polar);
		return outCart;
	}

	public static SphericalCoordinate CartesianToSpherical(Vector3 cartCoords){
		if (cartCoords.x == 0) {
			cartCoords.x = Mathf.Epsilon;
		}
		var outRadius = Mathf.Sqrt((cartCoords.x * cartCoords.x)
			+ (cartCoords.y * cartCoords.y)
			+ (cartCoords.z * cartCoords.z));
		var outPolar = Mathf.Atan(cartCoords.z / cartCoords.x);
		if (cartCoords.x < 0) {
			outPolar += Mathf.Pi;
		}
		var outElevation = Mathf.Asin(cartCoords.y / outRadius);
		return new SphericalCoordinate(outRadius, outPolar, outElevation);
	}
}

public class PlanetVertex : IVertex {
	public double[] Position { get; set; }

	public PlanetVertex(double[] location) {
		Position = location;
	}

	public PlanetVertex() {

	}

	public override int GetHashCode() {
		return Position.GetHashCode();
	}

	public override bool Equals(object obj) {
		var v = obj as PlanetVertex;
		if (
			v.Position[0].Equals(Position[0]) &&
			v.Position[1].Equals(Position[1]) &&
			v.Position[2].Equals(Position[2])
		) {
			return true;
		}
		return false;
	}

	public static bool operator ==(PlanetVertex hs1, PlanetVertex hs2) {
		if (((object) hs1) == null || ((object) hs2) == null) {
			return System.Object.Equals(hs1, hs2);
		}
		return hs1.Equals(hs2);
	}

	public static bool operator !=(PlanetVertex hs1, PlanetVertex hs2) {
		if (((object) hs1) == null || ((object) hs2) == null) {
			return System.Object.Equals(hs1, hs2);
		}
		return !hs1.Equals(hs2);
	}

	public override string ToString() {
		return string.Format("PlanetVertex({0}, {1}, {2})", Position[0], Position[1], Position[2]);
	}
}

public class PlanetFace : ConvexFace<PlanetVertex, PlanetFace> {
	public override int GetHashCode() {
		return this.Vertices.GetHashCode();
	}

	public override bool Equals(object obj) {
		var v = obj as PlanetFace;
		if (
			v.Vertices[0].Equals(Vertices[0]) &&
			v.Vertices[1].Equals(Vertices[1]) &&
			v.Vertices[2].Equals(Vertices[2])
		) {
			return true;
		}
		return false;
	}

	public static bool operator ==(PlanetFace hs1, PlanetFace hs2) {
		if (((object) hs1) == null || ((object) hs2) == null) {
			return System.Object.Equals(hs1, hs2);
		}
		return hs1.Equals(hs2);
	}

	public static bool operator !=(PlanetFace hs1, PlanetFace hs2) {
		if (((object) hs1) == null || ((object) hs2) == null) {
			return System.Object.Equals(hs1, hs2);
		}
		return !hs1.Equals(hs2);
	}

	public Vector3 Midpoint() {
		var x = (Vertices[0].Position[0] + Vertices[1].Position[0] + Vertices[2].Position[0]) / 3d;
		var y = (Vertices[0].Position[1] + Vertices[1].Position[1] + Vertices[2].Position[1]) / 3d;
		var z = (Vertices[0].Position[2] + Vertices[1].Position[2] + Vertices[2].Position[2]) / 3d;
		return new Vector3((float)x, (float)y, (float)z);
	}

	public Vector3 MidpointNormal() {
		var x = (Normal[0] + Normal[1] + Normal[2]) / 3d;
		var y = (Normal[0] + Normal[1] + Normal[2]) / 3d;
		var z = (Normal[0] + Normal[1] + Normal[2]) / 3d;
		return new Vector3((float)x, (float)y, (float)z);
	}
}

public class PlanetCellPoint {
	public Vector3 Vertex { get; set; }
	public Vector3 Normal { get; set; }

	public override string ToString() {
		return string.Format("PlanetCellPoint({0})", Vertex);
	}
}

public class PlanetCellTriangle {
	public PlanetCellPoint[] Points { get; set; }
}

public partial class PlanetMesh : MeshInstance3D {
	private int cells;
	private List<Vector3> cellCenters;
	private List<Vector3> cellMidpoints;

	public int Cells => cells;
	public List<Vector3> CellCenters { get => cellCenters; set => cellCenters = value; }
	public List<Vector3> CellMidpoints { get => cellMidpoints; set => cellMidpoints = value; }

	public void Generate(int cells) {
		var rng = new Random(123);
		var jitterAmount = 0.01f;

		// RenderingServer.SetDebugGenerateWireframes(true);
		// GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe;

		this.cells = cells;
		cellCenters = new List<Vector3>();
		cellMidpoints = new List<Vector3>();
		cellCenters.Clear();
		cellMidpoints.Clear();

		// find cell centers
		var phi = Math.PI * (3.0 - Math.Sqrt(5.0));
		for(int p = 0; p < Cells; p++) {
			var y = 1 - (p / ((double) (Cells - 1))) * 2;
			var radius = Math.Sqrt(1 - y * y);
			var theta = phi * p;
			var x = Math.Cos(theta) * radius;
			var z = Math.Sin(theta) * radius;
			var centerCart = new Vector3((float)x, (float)y, (float)z);
			var centerSphere = CoordinateConversion.CartesianToSpherical(centerCart);
			var jitterX = rng.NextSingle() * jitterAmount;
			var jitterY = rng.NextSingle() * jitterAmount;
			centerSphere.Polar += jitterX;
			centerSphere.Elevation += jitterY;
			var center = CoordinateConversion.SphericalToCartesian(centerSphere);
			CellCenters.Add(center);
		}
		
		// perform Delaunay triangulation
		var centers = new List<PlanetVertex>();
		foreach (var cell in cellCenters) {
			centers.Add(new PlanetVertex() { Position = new Double[] {
				(double) cell.x,
				(double) cell.y,
				(double) cell.z
			}});
		}
		var st = new SurfaceTool();
		st.Begin(Mesh.PrimitiveType.Triangles);


		var convexHull = ConvexHull.Create<PlanetVertex, PlanetFace>(centers, 1E-10);
		var faces = convexHull.Result.Faces;
		GD.PrintS("Cells:", cellCenters.Count);
		GD.PrintS("Faces", Enumerable.Count(faces));

		// create a mapping of cell centers to faces
		var pointsToFaces = new MultiMap<PlanetVertex, PlanetFace>();
		foreach (var face in faces) {
			foreach (var v in face.Vertices) {
				pointsToFaces.Add(v, face);
			}
		}

		// calculate midpoint of each face
		var cellPoints = new Dictionary<PlanetFace, PlanetCellPoint>();
		foreach (var face in faces) {
			cellPoints.Add(face, new PlanetCellPoint{
				Vertex = face.Midpoint(),
				Normal = face.MidpointNormal()
			});
		}

		// create triangles for each cell
		var newFacePoints = new List<PlanetCellPoint>();
		var t = 0;
		foreach (var (center, pointFaces) in pointsToFaces.Take(1)) {
			var thisCellPoints = new List<PlanetCellPoint>();
			var centerNormal = new Vector3();
			foreach (var face in pointFaces) {
				thisCellPoints.Add(cellPoints[face]);
				cellMidpoints.Add(cellPoints[face].Vertex);
				centerNormal += cellPoints[face].Normal;
			}
			centerNormal /= pointFaces.Count;

			var p0 = new PlanetCellPoint {
				Vertex = new Vector3((float)center.Position[0], (float)center.Position[1], (float)center.Position[2]),
				Normal = centerNormal,
			};

			for (int i = 0; i < thisCellPoints.Count; i++) {
				var p1 = thisCellPoints[i];
				var p2 = i + 1 == thisCellPoints.Count ? thisCellPoints[0] : thisCellPoints[i + 1];

				GD.PrintS("i", i);
				GD.PrintS("\tp1", p1);
				GD.PrintS("\tp2", p2);

				newFacePoints.Clear();
				newFacePoints.Add(p0);
				newFacePoints.Add(p1);
				newFacePoints.Add(p2);

				var faceCenter = (p0.Vertex + p1.Vertex + p2.Vertex) / 3f;
				var faceNormal = (p0.Normal + p1.Normal + p2.Normal) / 3f;
				var faceCenterSphere = CoordinateConversion.CartesianToSpherical(faceCenter);

				// newFacePoints.Sort((a, b) => sortPoints(faceCenter, a.Vertex, b.Vertex));
				// newFacePoints = newFacePoints.OrderBy(item => {
				// 		var p = CoordinateConversion.CartesianToSpherical(item.Vertex);
				// 		return Math.Atan2(p.Elevation - faceCenterSphere.Elevation, p.Polar - faceCenterSphere.Polar);
				// 	})
				// 	.ToList();
				foreach (var p in newFacePoints) {
					st.SetNormal(p.Normal);
					st.AddVertex(p.Vertex);
				}
				t++;

				// st.SetNormal(centerNormal);
				// st.AddVertex(p0);
			
				// st.SetNormal(p1.Normal);
				// st.AddVertex(p1.Vertex);

				// st.SetNormal(p2.Normal);
				// st.AddVertex(p2.Vertex);
			}
			
		}
		Mesh = st.Commit();
	}

	// From https://www.baeldung.com/cs/sort-points-clockwise
	private int sortPoints(Vector3 center, Vector3 a, Vector3 b) {
		var origin = new Vector3(0, 0, 0);
		var angle1 = getAngle(origin, a);
		var angle2 = getAngle(origin, b);
		if (angle1 < angle2) {
			return 1;
		}

		var d1 = origin.DistanceTo(a);
		var d2 = origin.DistanceTo(b);

		if (angle1 == angle2 && d1 < d2) {
			return 1;
		}

		return 0;
	}

	private double getAngle(Vector3 center, Vector3 point) {
		var diff = point - center;
		var angle = Math.Atan2(point.y, point.x);
		if (angle <= 0) {
			angle = 2 * Math.PI + angle;
		}
		return angle;
	}
}
