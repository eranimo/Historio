using System.Collections.Generic;
using RelEcs;


public class UnitType : Def {
	public string name { get; set; }
	public string spritePath { get; set; }
}

public class UnitData {
	public UnitType type;
	public Entity ownerCountry;
}

// Relation on entity to Country
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

// element
public class SelectedUnit {
	public Entity unit = null;
}

// triggers:

public class SelectedUnitUpdate {
	public Entity unit;
}