
// component of Construction entity
public abstract class ConstructionSite {}

public class DistrictConstructionSite : ConstructionSite {
	public DistrictType type;
}

public class ImprovementConstructionSite : ConstructionSite {
	public ImprovementType type;
}

// relation on construction to Tile
public class ConstructionTile {}