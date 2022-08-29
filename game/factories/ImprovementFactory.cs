using Godot;

public class ImprovementFactory : Factory {
	public ImprovementFactory(GameManager manager) : base(manager) {
	}

	public Entity AddImprovement(Hex hex, ImprovementType improvementType) {
		var improvementData = new ImprovementData { type = improvementType };
		var improvement = manager.state.Spawn();
		improvement.Add(improvementData);
		improvement.Add(new Location { hex = hex });

		var sprite = new Sprite();
		sprite.Centered = false;
		sprite.Texture = ResourceLoader.Load<Texture>(improvementType.spritePath);
		improvement.Add(sprite);

		manager.state.Send(new SpriteAdded { entity = improvement });
		return improvement;
	}
}