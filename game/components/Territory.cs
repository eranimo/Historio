using System.Collections.Generic;

public class TerritoryData {
	public string name;
	public Godot.Color color;
}
public class TerritoryPolityOwner {};
public class TerritoryTileUpdate {
	public RelEcs.Entity tile;
	public RelEcs.Entity territory;
}