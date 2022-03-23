using System.Collections.Generic;

public class Building : Entity {
	public BuildingType type;
	public Tile tile;

	public Building(BuildingType type, Tile tile) {
		this.type = type;
		this.tile = tile;
	}

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
