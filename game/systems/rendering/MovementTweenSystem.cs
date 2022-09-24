using System.Linq;
using Godot;

public class MovementTweenPlaySystem : ISystem {
	public void Run(Commands commands) {
		var layout = commands.GetElement<Layout>();
		var world = commands.GetElement<WorldService>();
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

				// TODO: determine if changing the location when tweening movement is needed
				// var nearestHex = layout.PixelToHex(Point.FromVector(unitIcon.Position + layout.HexSize.ToVector() / 2f));
				// world.moveEntity(entity, nearestHex);
				// commands.Send(new UnitMoved { unit = entity });

				// if (entity.Has<ViewStateNode>()) {
				// 	commands.Send(new ViewStateNodeUpdated { entity = entity });
				// }
			}
		} catch (System.Exception err) {
			GD.PrintErr(err);
		}
	}
}
