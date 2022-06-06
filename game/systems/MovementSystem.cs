using RelEcs;
using Godot;

public class MovementSystem : ISystem {
	public void Run(Commands commands) {
		var layout = commands.GetElement<Layout>();
		var pathfinder = commands.GetElement<Pathfinder>();
		var sprites = commands.Query<Entity, Location, Sprite, Movement>();

		foreach (var (entity, location, sprite, movement) in sprites) {
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
				GD.PrintS("MP left", mpLeft);
				var next = movement.path[pathIndex];
				movement.tweenHexes.Add(next);
				GD.PrintS("Set tween:", System.String.Join(", ", movement.tweenHexes));
				GD.PrintS("Move to", next);
				movement.currentPathIndex = pathIndex;

				if (next != null) {
					location.hex = next;
					if (entity.Has<ViewStateNode>()) {
						var viewStateNode = entity.Get<ViewStateNode>();
						commands.Send(new ViewStateNodeUpdated { entity = entity });
					}
					if (movement.currentTarget == location.hex) {
						movement.path.Clear();
						GD.PrintS("Movement ended");
						movement.currentTarget = null;
					}
				}
			}
		}
	}
}

public class MovementTweenSystem : ISystem {
	public void Run(Commands commands) {
		var layout = commands.GetElement<Layout>();
		var sprites = commands.Query<Location, Sprite, Movement>();
		var delta = commands.GetElement<PhysicsDelta>().delta;
		var game = commands.GetElement<Game>();
		
		try {
			foreach (var (location, sprite, movement) in sprites) {
				if (movement.tweenHexes.Count == 0) {
					continue;
				}
				float speed = movement.movementPointsLeft == 0
					? 1.0f
					: movement.movementPoints / movement.movementPointsLeft;
				float percentInTween = Mathf.Clamp(game.percentInDay * speed, 0, 1);
				float currentTweenIndex = (((float) movement.tweenHexes.Count) - 1) * percentInTween;
				var tweenFromIndex = (int) Mathf.Floor(currentTweenIndex);
				var tweenToIndex = (int) Mathf.Ceil(currentTweenIndex);
				var tweenSegmentPercent = currentTweenIndex - (float) tweenFromIndex;
				// GD.PrintS("Tween:", game.dayTicks, game.percentInDay, percentInTween, tweenSegmentPercent);
				var tweenFromHex = movement.tweenHexes[tweenFromIndex];
				var tweenToHex = movement.tweenHexes[tweenToIndex];
				var tweenFromVec = layout.HexToPixel(tweenFromHex).ToVector();
				var tweenToVec = layout.HexToPixel(tweenToHex).ToVector();
				sprite.Position = tweenFromVec.LinearInterpolate(tweenToVec, tweenSegmentPercent);
				// TODO: for each hex in the tween list, update view state
				if (game.isLastDayTick && movement.currentTarget == null) {
					movement.tweenHexes.Clear();
				}
			}
		} catch (System.Exception err) {
			GD.PrintErr(err);
		}
	}
}