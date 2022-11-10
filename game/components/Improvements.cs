using System.Collections.Generic;

public partial class ImprovementType : Def {
	public string name { get; set; }
	public string spritePath { get; set; }
}

public partial class ImprovementData {
	public ImprovementType type;
}
// relation to Country
public partial class ImprovementOwner { }