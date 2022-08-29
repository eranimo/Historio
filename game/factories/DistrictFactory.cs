using Godot;

public class DistrictFactory : Factory {
	public DistrictFactory(GameManager manager) : base(manager) {
	}

	public Entity AddDistrict(Hex hex, DistrictType districtType) {
		var districtData = new DistrictData { type = districtType };
		var district = manager.state.Spawn();
		district.Add(districtData);
		district.Add(new Location { hex = hex });

		var sprite = new Sprite();
		sprite.Centered = false;
		sprite.Texture = ResourceLoader.Load<Texture>(districtType.spritePath);
		district.Add(sprite);

		manager.state.Send(new SpriteAdded { entity = district });
		return district;
	}
}