
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

	public Vector2 ToLatLong() {
		return new Vector2(Polar, Elevation);
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

	public Vector3 ToVector() {
		return new Vector3((float)Position[0], (float)Position[1], (float)Position[2]);
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

public partial class PlanetMesh : MeshInstance3D {
	private List<Vector3> cellCenters;
	private List<Vector3> cellMidpoints;

	public List<Vector3> CellCenters { get => cellCenters; set => cellCenters = value; }
	public List<Vector3> CellMidpoints { get => cellMidpoints; set => cellMidpoints = value; }

	public void Generate() {
		var rng = new Random(123);

		// RenderingServer.SetDebugGenerateWireframes(true);
		// GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe;

		cellCenters = new List<Vector3>();
		cellMidpoints = new List<Vector3>();

		var st = new SurfaceTool();
		st.Begin(Mesh.PrimitiveType.Triangles);

		var watch = System.Diagnostics.Stopwatch.StartNew();
		var icosphere = new Icosphere(7);
		Godot.GD.PrintS($"\tBuilding Icosphere: {watch.ElapsedMilliseconds}ms");
		watch = System.Diagnostics.Stopwatch.StartNew();
		var hexsphere = new Hexsphere(icosphere);

		Godot.GD.PrintS($"\tBuilding Hexsphere: {watch.ElapsedMilliseconds}ms");
		GD.PrintS($"\t\tHexsphere cells:", hexsphere.Cells.Count);
		watch = System.Diagnostics.Stopwatch.StartNew();

		var cellColors = new Dictionary<Hexsphere.Cell, Color>();
		foreach (var cell in hexsphere.Cells) {
			var color = new Color(rng.NextSingle(), rng.NextSingle(), rng.NextSingle());
			// var color = new Color("white");
			cellColors[cell] = color;
		}
		// var first = hexsphere.Cells[0];
		// cellColors[first] = new Color("red");
		// foreach (var neighbor in first.Neighbors) {
		// 	cellColors[neighbor] = new Color("blue");
		// }

		foreach (var cell in hexsphere.Cells) {
			st.SetColor(cellColors[cell]);
			foreach (var face in cell.Faces) {
				st.AddVertex(hexsphere.Geometry.Vertices[face.v1]);
				st.AddVertex(hexsphere.Geometry.Vertices[face.v2]);
				st.AddVertex(hexsphere.Geometry.Vertices[face.v3]);
			}
		}

		Godot.GD.PrintS($"\tBuilding mesh: {watch.ElapsedMilliseconds}ms");

		// st.GenerateNormals();
		watch = System.Diagnostics.Stopwatch.StartNew();
		Mesh = st.Commit();
		Godot.GD.PrintS($"\tCommit mesh: {watch.ElapsedMilliseconds}ms");
	}
}
