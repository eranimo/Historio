
// component of Construction entity
public abstract class ConstructionSiteData {
	public List<ResourceAmount> resourceRequirements;
}

public class DistrictConstructionSite : ConstructionSiteData {
	public DistrictType type;
}

public class ImprovementConstructionSite : ConstructionSiteData {
	public ImprovementType type;
}

// relation on construction to Tile
public class ConstructionTile {}