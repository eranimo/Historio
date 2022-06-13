using RelEcs;
using Godot;
using System.Linq;

public class MovementSystem : ISystem {
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

public class MovementTweenSystem : ISystem {
	public void Run(Commands commands) {
		var layout = commands.GetElement<Layout>();
		var world = commands.GetElement<World>();
		var sprites = commands.Query<Entity, Location, UnitIcon, Movement>();
		var delta = commands.GetElement<PhysicsDelta>().delta;
		var game = commands.GetElement<Game>();
		
		try {
			foreach (var (entity, location, unitIcon, movement) in sprites) {
				if (movement.tweenHexes.Count == 0) {
					continue;
				}
				float speed = movement.movementPointsLeft == 0
					? 1.0f
					: movement.movementPoints / movement.movementPointsLeft;
				float percentInTween = Mathf.Clamp(game.percentInDay * speed, 0f, 1f);
				float currentTweenIndex = (((float) movement.tweenHexes.Count) - 1) * percentInTween;
				var tweenFromIndex = (int) Mathf.Floor(currentTweenIndex);
				var tweenToIndex = (int) Mathf.Ceil(currentTweenIndex);
				var tweenSegmentPercent = currentTweenIndex - (float) tweenFromIndex;
				// GD.PrintS("Tween:", game.dayTicks, game.percentInDay, percentInTween, tweenSegmentPercent);
				var tweenFromHex = movement.tweenHexes[tweenFromIndex];
				var tweenToHex = movement.tweenHexes[tweenToIndex];
				var tweenFromVec = layout.HexToPixel(tweenFromHex).ToVector();
				var tweenToVec = layout.HexToPixel(tweenToHex).ToVector();
				unitIcon.Position = tweenFromVec.LinearInterpolate(tweenToVec, tweenSegmentPercent);

				if (game.isLastDayTick && movement.currentTarget == null) {
					unitIcon.Position = layout.HexToPixel(movement.tweenHexes.Last()).ToVector();
					movement.tweenHexes.Clear();
				}

				var nearestHex = layout.PixelToHex(Point.FromVector(unitIcon.Position + layout.HexSize.ToVector() / 2f));
				world.moveEntity(entity, nearestHex);
				commands.Send(new UnitMoved { unit = entity });

				if (entity.Has<ViewStateNode>()) {
					commands.Send(new ViewStateNodeUpdated { entity = entity });
				}
			}
		} catch (System.Exception err) {
			GD.PrintErr(err);
		}
	}
}
