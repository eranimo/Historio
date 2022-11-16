using Godot;
using System;



public partial class PlanetMeshFace : MeshInstance3D {
	[Export]
	public Vector3 Normal = new Vector3();

	public override void _Ready() {
	}

	public void GenerateMesh(PlanetData planetData) {
		int resolution = 50;
		var num_vertices = resolution * resolution;
		var num_indices = (resolution - 1) * (resolution - 1) * 6;

		var vertex_array = new Vector3[num_vertices];
		var uv_array = new Vector3[num_vertices];
		var normal_array = new Vector3[num_vertices];
		var index_array = new int[num_indices];

		var tri_index = 0;
		var axisA = new Vector3(Normal.y, Normal.z, Normal.x);
		var axisB = Normal.Cross(axisA);
		for (int y = 0; y < resolution; y++) {
			for (int x = 0; x < resolution; x++) {
				var i = x + y * resolution;
				var percent = new Vector2(x, y) / (resolution-1);
				var pointOnUnitCube = Normal + (percent.x-0.5f) * 2.0f * axisA + (percent.y-0.5f) * 2.0f * axisB;
				var pointOnUnitSphere = pointOnUnitCube.Normalized();
				var pointOnPlanet = planetData.GetPoint(pointOnUnitSphere);
				vertex_array[i] = pointOnPlanet;
				
				if (x != (resolution-1) && y != (resolution-1)) {
					index_array[tri_index+2] = i;
					index_array[tri_index+1] = i+resolution+1;
					index_array[tri_index] = i+resolution;

					index_array[tri_index+5] = i;
					index_array[tri_index+4] = i+1;
					index_array[tri_index+3] = i+resolution+1;
					tri_index += 6;
				}
			}
		}

		// compute normals
		for (int a = 0; a < index_array.Length; a += 3) {
			int b = a + 1;
			int c = a + 2;
			Vector3 ab = vertex_array[index_array[b]] - vertex_array[index_array[a]];
			Vector3 bc = vertex_array[index_array[c]] - vertex_array[index_array[b]];
			Vector3 ca = vertex_array[index_array[a]] - vertex_array[index_array[c]];
			Vector3 cross_ab_bc = ab.Cross(bc) * -1.0f;
			Vector3 cross_bc_ca = bc.Cross(ca) * -1.0f;
			Vector3 cross_ca_ab = ca.Cross(ab) * -1.0f;
			normal_array[index_array[a]] += cross_ab_bc + cross_bc_ca + cross_ca_ab;
			normal_array[index_array[b]] += cross_ab_bc + cross_bc_ca + cross_ca_ab;
			normal_array[index_array[c]] += cross_ab_bc + cross_bc_ca + cross_ca_ab;
		}
		for (var i = 0; i < normal_array.Length; i++) {
			normal_array[i] = normal_array[i].Normalized();
		}

		var arrays = new Godot.Collections.Array();
		arrays.Resize((int)Mesh.ArrayType.Max);

		arrays[(int)Mesh.ArrayType.Vertex] = vertex_array.AsSpan();
		arrays[(int)Mesh.ArrayType.Normal] = normal_array.AsSpan();
		arrays[(int)Mesh.ArrayType.TexUv] = uv_array.AsSpan();
		arrays[(int)Mesh.ArrayType.Index] = index_array.AsSpan();

		// TODO: call with CallDeferred
		updateMesh(arrays, planetData);
	}

	private void updateMesh(Godot.Collections.Array arrays, PlanetData planetData) {
		var mesh = new ArrayMesh();
		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
		GD.PrintS("Created mesh face");
		this.Mesh = mesh;
	}
}
