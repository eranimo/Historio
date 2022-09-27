using Godot;

public class DistrictFactory : Factory {
	public DistrictFactory(GameManager manager) : base(manager) {
	}

	public Entity AddDistrict(Hex hex, DistrictType districtType) {
		var districtData = new DistrictData { type = districtType };

		var sprite = new Sprite();
		sprite.Centered = false;
		sprite.Texture = ResourceLoader.Load<Texture>(districtType.spritePath);

		var district = Spawn()
			.Add(districtData)
			.Add(new Location { hex = hex })
			.Add(sprite)
			.Id();

		manager.state.Send(new SpriteAdded { entity = district });
		return district;
	}
}