using Godot;
using System.Linq;

public partial class Planet : Node3D {
	public override void _Ready(){
		var planetMesh = GetNode<PlanetMesh>("PlanetMesh");
		planetMesh.Generate(10_000);
		renderPoints(GetNode<MultiMeshInstance3D>("CellCenters"), planetMesh.CellCenters);
		renderPoints(GetNode<MultiMeshInstance3D>("CellMidpoints"), planetMesh.CellMidpoints);
	}

	private void renderPoints(MultiMeshInstance3D mesh, List<Vector3> points) {
		var multiMesh = (MultiMesh) mesh.Multimesh;
		multiMesh.InstanceCount = points.Count;
		int i = 0;
		foreach (var cell in points) {
			var position = new Transform3D(Basis.Identity, cell);
			multiMesh.SetInstanceTransform(i, position);
			i++;
		}
	}
}