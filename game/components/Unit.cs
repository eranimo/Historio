using System.Collections.Generic;
using RelEcs;

public static class Unit {
	public enum UnitType {
		Scout,
	}

	public static Dictionary<UnitType, string> unitTypeSpritePath = new Dictionary<UnitType, string> () {
		{ UnitType.Scout, "res://assets/sprites/units/scout.tres" },
	};

	public static Dictionary<UnitType, string> unitNames = new Dictionary<UnitType, string>() {
		{ UnitType.Scout, "Scout" },
	};
}

public class UnitData {
	public Unit.UnitType type;
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