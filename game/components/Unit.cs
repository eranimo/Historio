using System;
using System.Collections.Generic;
using MessagePack;
using RelEcs;


[MessagePackObject]
public class UnitType : Def {
	[Key(2)]
	public string name { get; set; }

	[Key(3)]
	public string spritePath { get; set; }
}

[MessagePackObject]
public class UnitData {
	[Key(0)]
	public UnitType type;
}

// relation on Unit to Country
[MessagePackObject]
public class UnitCountry {}

// Relation on entity to Country
[MessagePackObject]
public class UnitCountryOwner {}


public class UnitAdded {
	public RelEcs.Entity unit;
}

public class UnitRemoved {
	public RelEcs.Entity unit;
}

/*
UNIT SELECTION
*/