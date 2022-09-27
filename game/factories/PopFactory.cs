public class PopFactory : Factory {
	public PopFactory(GameManager manager) : base(manager) {
	}

	private Entity AddPop(
		Entity tile,
		int size
	) {
		return Spawn()
			.Add(new PopData {
				size = size,
			})
			.Add<PopTile>(tile)
			.Add<Inventory>(new Inventory())
			.Id();
	}
}