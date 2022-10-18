using System;
using Godot;
using MessagePack;

[MessagePackObject]
public class Location {
	[Key(0)]
	public Hex hex;
}