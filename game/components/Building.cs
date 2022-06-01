using System.Collections.Generic;

public static class Building {
	public enum BuildingType {
		Village,
		Fort,
		Farm,
		Woodcutter,
		Mine,
	}

	public static Dictionary<BuildingType, int> BuildingTypeIconIndex = new Dictionary<BuildingType, int> () {
		{ BuildingType.Village, 1 },
		{ BuildingType.Fort, 2 },
		{ BuildingType.Farm, 3 },
		{ BuildingType.Woodcutter, 4 },
		{ BuildingType.Mine, 5 },
	};
}

public class BuildingData {
	public Building.BuildingType type;
}
public class BuildingOwner {}

public class BuildingAdded {
	RelEcs.Entity building;
}