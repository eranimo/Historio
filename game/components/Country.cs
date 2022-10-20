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

	[Key(2)]
	public HashSet<Hex> exploredHexes = new HashSet<Hex>();

	[Key(3)]
	public HashSet<Hex> observedHexes = new HashSet<Hex>();
}

public class CountryAdded {
	public RelEcs.Entity country;
}

// tag on entity with Location and ViewStateNode
public class CountryTile { }