using Godot;

public partial class DistrictType : Def {
	public string name { get; set; }
	public string spritePath { get; set; }
	public List<ResourceAmountDef> buildRequirements { get; set; }
}

public partial class DistrictData {
	public DistrictType type;
}

// relation on District to Country
public partial class DistrictOwner { }