public partial class PopProfessionType : Def {
	public string name { get; set; }
	public bool isFree { get; set; }
}

// relation on Pop to Tile
public partial class PopTile {}

public partial class PopData {
	public int size;
	public PopProfessionType profession;
	public Entity ownerPop; // Pop that owns this Pop, if the Profession allows it

	public float laborPower => size;
}