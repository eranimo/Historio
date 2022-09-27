using Godot;

public class BiotaFactory : Factory {
	public BiotaFactory(GameManager manager) : base(manager) {
	}

	public Entity Add(Entity tile, BiotaType biotaType, int size) {
		var biota = Spawn()
			.Add<BiotaData>(new BiotaData {
				biotaType = biotaType,
				size = size
			})
			.Add(new BiotaTile(), tile)
			.Id();
		manager.state.Send(new BiotaAdded {
			biota = biota,
			tile = tile,
		});
		return biota;
	}
}