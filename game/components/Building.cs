public struct ProductionOption {
	public List<LaborRequirement> laborRequirement { get; set; }
	public int productionCycle { get; set; }
	public List<ResourceAmountDef> input { get; set; }
	public List<ResourceAmountDef> output { get; set; }
}

public class BuildingType : Def {
	public string name { get; set; }
	// possible productions made available at this building
	public List<ProductionOption> productionOptions { get; set; }
}

public class BuildingData {
	public BuildingType buildingType;
	public Entity owner; // owner Pop
}

// relation on Building to District
public class DistrictBuilding { }