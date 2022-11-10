using System.Linq;
using Godot;

public partial class MovementTweenPlaySystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		var layout = World.GetElement<Layout>();
		var world = World.GetElement<WorldService>();
		var delta = World.GetElement<PhysicsDelta>().delta;
		var game = World.GetElement<Game>();
		var sprites = World.Query<Entity, Location, UnitIcon, Movement>().Build();

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
				unitIcon.Position = tweenFromVec.Lerp(tweenToVec, tweenSegmentPercent);

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
