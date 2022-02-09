using Godot;
using System;

public class Game : Node2D {
	public override void _Ready() {
		var terrain = (TileMap) GetNode<TileMap>("Terrain");
		var features = (TileMap) GetNode<TileMap>("Features");
		var grid = (TileMap) GetNode<TileMap>("Grid");
		terrain.Clear();
		features.Clear();

		var generator = new WorldGenerator();
		GameWorld world = generator.Generate();
		
		foreach(Tile tile in world.tiles) {
			grid.SetCell(tile.coord.Col, tile.coord.Row, 1);
			terrain.SetCell(tile.coord.Col, tile.coord.Row, 1);
		}
	}
}
