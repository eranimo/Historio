using Godot;

public class BiotaFactory : Factory {
	public BiotaFactory(GameManager manager) : base(manager) {
	}

	public Entity Add(Entity tile, BiotaType biotaType, int size) {
		var biota = manager.state.Spawn();
		biota.Add(new BiotaData {
			biotaType = biotaType,
			size = size
		});
		biota.Add(new BiotaTile(), tile);
		manager.state.Send(new BiotaAdded {
			biota = biota,
			tile = tile,
		});
		return biota;
	}
}