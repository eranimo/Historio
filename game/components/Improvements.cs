using System.Collections.Generic;

public class ImprovementType : Def {
	public string name { get; set; }
	public string spritePath { get; set; }
}

public class ImprovementData {
	public ImprovementType type;
}
// relation to Country
public class ImprovementOwner { }