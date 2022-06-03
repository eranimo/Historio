using RelEcs;
using Godot;

public class MovementSystem : ISystem {
	public void Run(Commands commands) {
		var gameMap = commands.GetElement<GameMap>();
		var sprites = commands.Query<Hex, Sprite, Movement>();

		foreach (var (hex, sprite, movement) in sprites) {
			sprite.Position = gameMap.layout.HexToPixel(movement.currentTarget).ToVector();

			if (movement.moveQueue.Count > 0) {
				movement.currentTarget = movement.moveQueue.Dequeue();
			} else {
				movement.currentTarget = null;
			}
		}
	}
}