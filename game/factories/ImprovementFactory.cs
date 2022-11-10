using Godot;

public partial class ImprovementFactory : Factory {
	public ImprovementFactory(GameManager manager) : base(manager) {
	}

	public Entity AddImprovement(Hex hex, ImprovementType improvementType) {
		var improvementData = new ImprovementData { type = improvementType };

		var sprite = new Sprite2D();
		sprite.Centered = false;
		sprite.Texture = ResourceLoader.Load<Texture2D>(improvementType.spritePath);

		var improvement = Spawn()
			.Add(improvementData)
			.Add(new Location { hex = hex })
			.Add(sprite)
			.Id();

		manager.state.Send(new SpriteAdded { entity = improvement });
		return improvement;
	}
}