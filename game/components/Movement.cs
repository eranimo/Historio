using System.Collections.Generic;

public class Movement {
	// hex we are moving to
	public Hex currentTarget = null; 

	// queue of hexes we are moving to next after our current location
	public List<Hex> path = new List<Hex>();
	public int currentPathIndex;

	/* Movement points
		e.g. 10 MP means moving 1 hex per day on terrain costing 10 MP
			 20 MP means moving 2 hex per day on terrain costing 10 MP
	*/
	public float movementPoints = 10.0f;

	// List of hexes to tween between 
	public List<Hex> tweenHexes = new List<Hex>();

	public float movementPointsLeft;
	public Action movementAction;

	public void Reset() {
		currentTarget = null;
		path.Clear();
	}
}

public class UnitMoved {
	public RelEcs.Entity unit;
}