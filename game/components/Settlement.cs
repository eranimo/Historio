using System.Collections.Generic;
using MessagePack;

[MessagePackObject]
public class SettlementData {
	[Key(0)]
	public string name;
}

public class SettlementOwner {};

public class CountryTileSettlement {};

// action sent when Settlement tiles update
public class SettlementBorderUpdated {
	public RelEcs.Entity countryTile;
	public RelEcs.Entity settlement;
}

// relation to country on settlement
public class CapitalSettlement {}