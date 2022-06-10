using Godot;
using RelEcs;
using System;
using System.Collections.Generic;

public class UnitPaths : TileMap {
	public HashSet<Entity> movementComponents = new HashSet<Entity>();

	public enum SelectedUnitsTilemapIndex {
		Target = 2,
		HexInterval = 3,
		DayInterval = 4,
	}

	public void render() {
		foreach (var path in movementComponents) {
			
		}
	}
}
