using RelEcs;
using Godot;

public partial class SpriteRenderSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		var gameMap = World.GetElement<GameMap>();

		foreach (var e in World.Receive<SpriteAdded>(this)) {
			var location = World.GetComponent<Location>(e.entity);
			var sprite = World.GetComponent<Godot.Sprite2D>(e.entity);
			sprite.Position = gameMap.layout.HexToPixel(location.hex).ToVector();
			gameMap.spriteContainer.AddChild(sprite);
		}

		foreach (var e in World.Receive<SpriteRemoved>(this)) {
			var sprite = World.GetComponent<Godot.Sprite2D>(e.entity);
			gameMap.spriteContainer.RemoveChild(sprite);
		}
	}
}