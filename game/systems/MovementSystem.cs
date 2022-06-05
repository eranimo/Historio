using RelEcs;
using Godot;

public class MovementSystem : ISystem {
	public void Run(Commands commands) {
		var layout = commands.GetElement<Layout>();
		var sprites = commands.Query<Entity, Location, Sprite, Movement>();

		foreach (var (entity, location, sprite, movement) in sprites) {
			if (movement.currentTarget != null) {
				var next = movement.moveQueue.Dequeue();
				if (next != null) {
					location.hex = next;
					sprite.Position = layout.HexToPixel(next).ToVector();
					if (entity.Has<ViewStateNode>()) {
						var viewStateNode = entity.Get<ViewStateNode>();
						commands.Send(new ViewStateNodeUpdated { entity = entity });
					}
					if (movement.currentTarget == location.hex) {
						movement.currentTarget = null;
					}
				}
			}
		}
	}
}