using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class ResourceType : Def {
	public string name { get; set; }
	public float basePrice { get; set; }
	public float nutrition { get; set; } = 0f;

	[JsonConverter(typeof(StringEnumConverter))]
	public ResourceCategory category { get; set; }
}

// struct used in Defs for tuple of resource type and amount
public struct ResourceAmountDef {
	public DefRef<ResourceType> resource { get; set; }
	public float amount { get; set; }
}

public struct ResourceAmount {
	public ResourceType resource;
	public float amount;
}

public enum ResourceCategory {
	Food,
	RawMaterials,
}

public class ResourceNode {
	public ResourceType resourceType;
	public float amountLeft;
	public float amountMax;
	public float growth;
}
