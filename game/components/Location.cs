using System;
using Godot;
using MessagePack;

[MessagePackObject]
public partial class Location {
	[Key(0)]
	public Hex hex;
}