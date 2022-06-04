using System.Collections.Generic;
using RelEcs;

public static class Unit {
	public enum UnitType {
		Scout,
	}

	public static Dictionary<UnitType, string> unitTypeSpritePath = new Dictionary<UnitType, string> () {
		{ UnitType.Scout, "res://assets/sprites/units/scout.tres" },
	};
}

public class UnitData {
	public Unit.UnitType type;
}

public class UnitPolityOwner {}