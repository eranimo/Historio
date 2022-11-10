using System;
using System.Collections.Generic;
using MessagePack;

[MessagePackObject]
public partial class Movement {
	[Key(0)]
	// hex we are moving to
	public Hex currentTarget = null;

	// queue of hexes we are moving to next after our current location
	[Key(1)]
	public List<Hex> path = new List<Hex>();

	[Key(2)]
	public int currentPathIndex;

	/* Movement points
		e.g. 10 MP means moving 1 hex per day on terrain costing 10 MP
			 20 MP means moving 2 hex per day on terrain costing 10 MP
	*/
	[Key(3)]
	public float movementPoints = 10.0f;

	// List of hexes to tween between 
	[Key(4)]
	public List<Hex> tweenHexes = new List<Hex>();

	[Key(5)]
	public float movementPointsLeft;

	[Key(6)]
	public Action movementAction;

	public void Reset() {
		currentTarget = null;
		path.Clear();
	}
}

public partial class UnitMovementPathUpdated {
	public Entity unit;
}

public partial class UnitMoved {
	public RelEcs.Entity unit;
}