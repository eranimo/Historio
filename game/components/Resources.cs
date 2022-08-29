public static class ResourceConstants {
	public static Dictionary<ResourceType, string> title = new Dictionary<ResourceType, string>() {
		{ ResourceType.Wood, "Wood" },
	};
}

public enum ResourceType {
	Wood,
	Stone,
	Wheat,
	IronOre,
	Water,
}

public struct ResourceRecord {
	public ResourceType type;
	public int amount;
}

public class ResourceNode {
	public ResourceType resourceType;
	public float amountLeft;
	public float amountMax;
	public float growth;
}

public class TileResources {
	public Dictionary<ResourceType, int> resources;
}