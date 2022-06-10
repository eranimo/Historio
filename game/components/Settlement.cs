using System.Collections.Generic;

public class SettlementData {
	public string name;
	public RelEcs.Entity ownerPolity;
}

// relationship on Tile to owner Settlement
public class SettlementTile {}

// action sent when Settlement tiles update
public class TileBorderUpdate {
	public RelEcs.Entity tile;
	public RelEcs.Entity settlement;
}

// relation to polity on settlement
public class CapitalSettlement {}