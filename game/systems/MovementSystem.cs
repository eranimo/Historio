using RelEcs;
using Godot;

public class MovementSystem : ISystem {
	public void Run(Commands commands) {
		var layout = commands.GetElement<Layout>();
		var sprites = commands.Query<Hex, Sprite, Movement>();

		foreach (var (hex, sprite, movement) in sprites) {
			if (movement.currentTarget != null) {
				sprite.Position = layout.HexToPixel(movement.currentTarget).ToVector();
			}
			if (movement.moveQueue.Count > 0) {
				movement.currentTarget = movement.moveQueue.Dequeue();
			} else {
				movement.currentTarget = null;
			}
		}
	}
}