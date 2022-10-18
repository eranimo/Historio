using System;
using System.Collections.Generic;
using Godot;
using MessagePack;
using MessagePack.Formatters;

[MessagePackObject]
public class CountryData {
	[Key(0)]
	public string name;

	[MessagePackFormatter(typeof(ColorFormatter))]
	[Key(1)]
	public Color color;
}

public class CountryAdded {
	public RelEcs.Entity country;
}

// relationship on Tile to owner Polity
public class CountryTile { }