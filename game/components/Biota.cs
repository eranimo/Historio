using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public partial class BiotaType : Def {
	public string name { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public BiotaCategory category { get; set; }

	// [JsonConverter(typeof(StringEnumConverter))]
	public HashSet<BiotaClassification> classifications { get; set; }

	// modifier for how fast this biota reproduces naturally
	public float reproductionRate { get; set; } = 0.05f;

	// space one unit takes up
	public float space { get; set; } = 1f;

	public PlantRequirements plantRequirements;
	public AnimalRequirements animalRequirements;

	// the resources extracted from this biota by Pops (per unit)
	public List<ResourceAmountDef> resources { get; set; }
}

public enum BiotaCategory {
	Plant,
	Animal,
}

[JsonConverter(typeof(StringEnumConverter))]
public enum BiotaClassification {
	FloweringPlants,
	SmallMammal,
	Predator,
}

public partial class PlantRequirements {
	// fertility required per unit
	public float fertility { get; set; }
}

public partial class AnimalRequirements {
	// how much of a certain biota category do they need (per unit)
	public List<BiotaNeedDef> nutrition { get; set; }
}


public partial class BiotaNeedDef {
	public float amount { get; set; }
	public BiotaClassification classification { get; set; }
}

// component
public partial class BiotaData {
	public BiotaType biotaType;
	public int size;

	// stats
	public int births = 0;
	public int deathsKilled = 0;

	// animal only
	public int deathsStarved = 0;
	public float needPercentFilled;
	public int migrated = 0;

	public int deaths {
		get {
			return deathsKilled + deathsStarved;
		}
	}

	public int growth {
		get {
			return births - deaths;
		}
	}

	public float spaceUsed {
		get {
			return biotaType.space * size;
		}
	}
}

// relation on Biota to Tile
public partial class BiotaTile {}

// trigger for when biota are added to a tile
public partial class BiotaAdded {
	public Entity biota;
	public Entity tile;
}
