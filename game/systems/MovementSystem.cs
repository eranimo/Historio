using RelEcs;
using Godot;

public class MovementSystem : ISystem {
	public void Run(Commands commands) {
		var layout = commands.GetElement<Layout>();
		var sprites = commands.Query<Entity, Hex, Sprite, Movement>();

		foreach (var (entity, hex, sprite, movement) in sprites) {
			if (movement.currentTarget != null) {
				var next = movement.moveQueue.Dequeue();
				if (next != null) {
					hex.Set(next);
					sprite.Position = layout.HexToPixel(next).ToVector();
					if (movement.currentTarget == hex) {
						movement.currentTarget = null;
					}
				}
			}
		}
	}
}