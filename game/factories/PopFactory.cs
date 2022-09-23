public class PopFactory : Factory {
	public PopFactory(GameManager manager) : base(manager) {
	}

	private Entity AddPop(
		Entity tile,
		int size
	) {
		var pop = manager.state.Spawn();
		pop.Add(new PopData {
			size = size,
		});
		pop.Add<PopTile>(tile);
		pop.Add<Inventory>(new Inventory());
		return pop;
	}
}