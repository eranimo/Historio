using RelEcs;

public class SpriteRenderSystem : ISystem {
	public void Run(Commands commands) {
		var gameMap = commands.GetElement<GameMap>();

		commands.Receive((SpriteAdded e) => {
			var location = e.entity.Get<Location>();
			var sprite = e.entity.Get<Godot.Sprite>();
			sprite.Position = gameMap.layout.HexToPixel(location.hex).ToVector();
			gameMap.spriteContainer.AddChild(sprite);
		});

		commands.Receive((SpriteRemoved e) => {
			var sprite = e.entity.Get<Godot.Sprite>();
			gameMap.spriteContainer.RemoveChild(sprite);
		});
	}
}