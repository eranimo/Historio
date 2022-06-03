using RelEcs;

public class SpriteRenderSystem : ISystem {
	public void Run(Commands commands) {
		var gameMap = commands.GetElement<GameMap>();

		commands.Receive((SpriteAdded e) => {
			var hex = e.entity.Get<Hex>();
			var sprite = e.entity.Get<Godot.Sprite>();
			sprite.Position = gameMap.layout.HexToPixel(hex).ToVector();
			gameMap.spriteContainer.AddChild(sprite);
		});

		commands.Receive((SpriteRemoved e) => {
			var sprite = e.entity.Get<Godot.Sprite>();
			gameMap.spriteContainer.RemoveChild(sprite);
		});
	}
}