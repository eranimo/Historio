public class UnitFactory : Factory {
	public UnitFactory(GameManager manager) : base(manager) {}

	public void NewUnit(
		Entity ownerCountry,
		Hex hex,
		UnitType unitType
	) {
		var unitData = new UnitData { type = unitType, ownerCountry = ownerCountry };
		var unit = Spawn()
			.Add(unitData)
			.Add(new Location { hex = hex })
			.Add(new ActionQueue())
			.Add(new Movement())
			.Add(new ViewStateNode { country = ownerCountry, range = 2 })
			.Id();

		manager.state.Send(new UnitAdded { unit = unit });
		manager.state.Send(new ViewStateNodeUpdated { entity = unit });

		manager.world.moveEntity(unit, hex);
	}
}