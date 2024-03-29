public partial class UnitFactory : Factory {
	public UnitFactory(GameManager manager) : base(manager) {}

	public void NewUnit(
		Entity ownerCountry,
		Hex hex,
		UnitType unitType
	) {
		var unit = Spawn()
			.Add(new UnitData { type = unitType })
			.Add(new Location { hex = hex })
			.Add(new ActionQueue())
			.Add(new Movement())
			.Add(new ViewStateNode { range = 2 })
			.Add<ViewStateOwner>(ownerCountry)
			.Add<UnitCountry>(ownerCountry)
			.Add<Persisted>()
			.Id();

		manager.state.Send(new UnitAdded { unit = unit });
		manager.state.Send(new ViewStateNodeUpdated { entity = unit });

		manager.world.moveEntity(unit, hex);
	}
}