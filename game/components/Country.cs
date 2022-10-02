using System;
using System.Collections.Generic;
using Godot;

[Serializable]
public class CountryData {
	public string name;
	public Color color;
}

public class CountryAdded {
	public RelEcs.Entity country;
}

// relationship on Tile to owner Polity
public class CountryTile { }