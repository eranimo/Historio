using RelEcs;

public class SpriteRenderSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		var gameMap = this.GetElement<GameMap>();

		foreach (var e in this.Receive<SpriteAdded>()) {
			var location = this.GetComponent<Location>(e.entity);
			var sprite = this.GetComponent<Godot.Sprite>(e.entity);
			sprite.Position = gameMap.layout.HexToPixel(location.hex).ToVector();
			gameMap.spriteContainer.AddChild(sprite);
		}

		foreach (var e in this.Receive<SpriteRemoved>()) {
			var sprite = this.GetComponent<Godot.Sprite>(e.entity);
			gameMap.spriteContainer.RemoveChild(sprite);
		}
	}
}