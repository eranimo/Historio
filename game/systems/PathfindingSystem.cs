using Godot;
using RelEcs;
using System;
using System.Linq;
using System.Collections.Generic;

public class PathfindingSystem : ISystem {
	public void Run(Commands commands) {
		var world = commands.GetElement<World>();
		var pathfinder = commands.GetElement<Pathfinder>();
		var query = commands.Query<Location, Movement>();

		foreach (var (location, movement) in query) {
			if (movement.currentTarget != null && movement.path.Count == 0) {
				var fromTile = world.GetTile(location.hex);
				var toTile = world.GetTile(movement.currentTarget);
				GD.PrintS("From:", location.hex, "To:", movement.currentTarget);
				var path = pathfinder.getPath(fromTile, toTile);
				if (path == null) {
					GD.PrintS("No path to target, removing target");
					movement.currentTarget = null;
				} else {
					GD.PrintS("Found path:", String.Join(", ", path));
					movement.path = new List<Hex>(path);
					movement.currentPathIndex = 0;
				}
			}
		}
	}
}