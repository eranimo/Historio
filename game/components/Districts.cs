using Godot;

public class DistrictType : Def {
	public string name { get; set; }
	public string spritePath { get; set; }
	public List<ResourceAmountDef> buildRequirements { get; set; }
}

public class DistrictData {
	public DistrictType type;
}

// relation on District to Country
public class DistrictOwner { }