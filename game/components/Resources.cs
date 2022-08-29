public class ResourceType : Def {
	public string name { get; set; }
	public string spritePath { get; set; }
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