using System;
using System.Collections.Generic;
using RelEcs;


[Serializable]
public class UnitType : Def {
	public string name { get; set; }
	public string spritePath { get; set; }
}

[Serializable]
public class UnitData {
	public UnitType type;
}

// relation on Unit to Country
[Serializable]
public class UnitCountry {}

// Relation on entity to Country
[Serializable]
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