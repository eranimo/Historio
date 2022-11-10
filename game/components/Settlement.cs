using System.Collections.Generic;
using MessagePack;

[MessagePackObject]
public partial class SettlementData {
	[Key(0)]
	public string name;
}

public partial class SettlementOwner {};

public partial class CountryTileSettlement {};

// action sent when Settlement tiles update
public partial class SettlementBorderUpdated {
	public RelEcs.Entity countryTile;
	public RelEcs.Entity settlement;
}

// relation to country on settlement
public partial class CapitalSettlement {}