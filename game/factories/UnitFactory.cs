public class UnitFactory : Factory {
	public UnitFactory(GameManager manager) : base(manager) {}

	public void NewUnit(
		Entity ownerCountry,
		Hex hex,
		Unit.UnitType unitType
	) {
		var unitData = new UnitData { type = unitType, ownerCountry = ownerCountry };
		var unit = manager.state.Spawn();
		unit.Add(unitData);
		unit.Add(new Location { hex = hex });

		manager.state.Send(new UnitAdded { unit = unit });

		var actionQueue = new ActionQueue();
		unit.Add(actionQueue);

		var movement = new Movement();
		unit.Add(movement);
		unit.Add(new ViewStateNode { country = ownerCountry, range = 2 });
		manager.state.Send(new ViewStateNodeUpdated { entity = unit });
		manager.world.moveEntity(unit, hex);
	}
}