public class UnitFactory {
	private readonly GameManager manager;

	public UnitFactory(GameManager manager) {
		this.manager = manager;
	}

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
		manager.state.Send(new ActionQueueAdd {
			owner = unit,
			action = new MovementAction(unit, hex.Neighbor(Direction.South, 5))
		});
		manager.state.Send(new ActionQueueAdd {
			owner = unit,
			action = new MovementAction(unit, hex.Neighbor(Direction.NorthEast, 5))
		});

		var movement = new Movement();
		// movement.currentTarget = hex.Neighbor(Direction.South, 5); // .Neighbor(Direction.SouthWest, 15);
		unit.Add(movement);
		unit.Add(new ViewStateNode { country = ownerCountry, range = 2 });
		manager.state.Send(new ViewStateNodeUpdated { entity = unit });
		manager.world.moveEntity(unit, hex);
	}
}