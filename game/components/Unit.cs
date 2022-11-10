using System;
using System.Collections.Generic;
using MessagePack;
using RelEcs;


[MessagePackObject]
public partial class UnitType : Def {
	[Key(2)]
	public string name { get; set; }

	[Key(3)]
	public string spritePath { get; set; }
}

[MessagePackObject]
public partial class UnitData {
	[Key(0)]
	public UnitType type;
}

// relation on Unit to Country
[MessagePackObject]
public partial class UnitCountry {}

// Relation on entity to Country
[MessagePackObject]
public partial class UnitCountryOwner {}


public partial class UnitAdded {
	public RelEcs.Entity unit;
}

public partial class UnitRemoved {
	public RelEcs.Entity unit;
}

/*
UNIT SELECTION
*/