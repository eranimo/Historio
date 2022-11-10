
// component of Construction entity
public abstract class ConstructionSiteData {
	public List<ResourceAmount> resourceRequirements;
}

public partial class DistrictConstructionSite : ConstructionSiteData {
	public DistrictType type;
}

public partial class ImprovementConstructionSite : ConstructionSiteData {
	public ImprovementType type;
}

// relation on construction to Tile
public partial class ConstructionTile {}