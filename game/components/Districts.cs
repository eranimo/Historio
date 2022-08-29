using Godot;

public class DistrictType : Def {
	public string name { get; set; }
	public string spritePath { get; set; }
}

public class DistrictData {
	public DistrictType type;
}

// relation to Country
public class DistrictOwner { }