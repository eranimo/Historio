using RelEcs;
using Godot;
using System.Linq;

public class MovementDaySystem : ISystem {
	public void Run(Commands commands) {
		var layout = commands.GetElement<Layout>();
		var world = commands.GetElement<World>();
		var pathfinder = commands.GetElement<Pathfinder>();
		var sprites = commands.Query<Entity, Location, Movement>();

		foreach (var (entity, location, movement) in sprites) {
			if (movement.currentTarget != null) {
				
				// calculate hex in path to go to
				float mpLeft = movement.movementPoints;
				int pathIndex = movement.currentPathIndex;
				movement.tweenHexes.Clear();
				while (mpLeft > 0) {
					if (pathIndex < movement.path.Count - 1) {
						var hex = movement.path[pathIndex];
						movement.tweenHexes.Add(hex);
						mpLeft -= pathfinder.getMovementCost(hex);
						pathIndex++;
					} else {
						break;
					}
				}
				movement.movementPointsLeft = mpLeft;
				var next = movement.path[pathIndex];
				movement.tweenHexes.Add(next);
				GD.PrintS("(MovementSystem) Move to", next);
				movement.currentPathIndex = pathIndex;

				if (next != null) {
					world.moveEntity(entity, next);
					commands.Send(new UnitMoved { unit = entity });

					if (entity.Has<ViewStateNode>()) {
						commands.Send(new ViewStateNodeUpdated { entity = entity });
					}

					if (movement.currentTarget == location.hex) {
						movement.path.Clear();
						GD.PrintS("(MovementSystem) Movement ended");
						movement.currentTarget = null;
						movement.movementAction.status = ActionStatus.Finished;
					}
				}
			}
		}
	}
}