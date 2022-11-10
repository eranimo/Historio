using Godot;

public partial class DistrictFactory : Factory {
	public DistrictFactory(GameManager manager) : base(manager) {
	}

	public Entity AddDistrict(Hex hex, DistrictType districtType) {
		var districtData = new DistrictData { type = districtType };

		var sprite = new Sprite2D();
		sprite.Centered = false;
		sprite.Texture = ResourceLoader.Load<Texture2D>(districtType.spritePath);

		var district = Spawn()
			.Add(districtData)
			.Add(new Location { hex = hex })
			.Add(sprite)
			.Id();

		manager.state.Send(new SpriteAdded { entity = district });
		return district;
	}
}