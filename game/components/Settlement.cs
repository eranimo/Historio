using System.Collections.Generic;

public class SettlementData {
	public string name;
	public RelEcs.Entity ownerPolity;
}

// component on Tile defining relationship to owner Settlement
public class SettlementTile {
	public RelEcs.Entity settlement;
}

// action sent when Settlement tiles update
public class TileBorderUpdate {
	public RelEcs.Entity tile;
	public RelEcs.Entity settlement;
}