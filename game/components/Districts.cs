using Godot;

public struct BuildRequirements {
	public DefRef<ResourceType> resource { get; set; }
	public int amount { get; set; }
}

public class DistrictType : Def {
	public string name { get; set; }
	public string spritePath { get; set; }
	public List<BuildRequirements> buildRequirements { get; set; }
}

public class DistrictData {
	public DistrictType type;
}

// relation to Country
public class DistrictOwner { }