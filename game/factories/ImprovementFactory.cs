using Godot;

public class ImprovementFactory : Factory {
	public ImprovementFactory(GameManager manager) : base(manager) {
	}

	public Entity AddImprovement(Hex hex, ImprovementType improvementType) {
		var improvementData = new ImprovementData { type = improvementType };

		var sprite = new Sprite();
		sprite.Centered = false;
		sprite.Texture = ResourceLoader.Load<Texture>(improvementType.spritePath);

		var improvement = Spawn()
			.Add(improvementData)
			.Add(new Location { hex = hex })
			.Add(sprite)
			.Id();

		manager.state.Send(new SpriteAdded { entity = improvement });
		return improvement;
	}
}