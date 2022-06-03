using System.Collections.Generic;

public static class Building {
	public enum BuildingType {
		Village,
		Fort,
		Farm,
		Woodcutter,
		Mine,
	}

	public static Dictionary<BuildingType, string> buildingTypeSpritePath = new Dictionary<BuildingType, string> () {
		{ BuildingType.Village, "res://assets/sprites/buildings/village.tres" },
		{ BuildingType.Fort, "res://assets/sprites/buildings/fort.tres" },
		{ BuildingType.Farm, "res://assets/sprites/buildings/farm.tres" },
		{ BuildingType.Woodcutter, "res://assets/sprites/buildings/woodcutter.tres" },
		{ BuildingType.Mine, "res://assets/sprites/buildings/mine.tres" },
	};
}

public class BuildingData {
	public Building.BuildingType type;
}
public class BuildingOwner {}

public class SpriteAdded {
	public RelEcs.Entity entity;
}

public class SpriteRemoved {
	public RelEcs.Entity entity;
}