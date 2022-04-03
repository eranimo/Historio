using Godot;
using System;
using System.Collections;
using System.Collections.Generic;


public class GameWorld {
	private Dictionary<Hex, Tile> tilesByCoord = new Dictionary<Hex, Tile>();
	private readonly GameManager manager;

	public GameWorld(GameManager manager) {
		this.manager = manager;

		GD.PrintS("(GameWorld) start");
		manager.Query<Tile>().Execute(onEntityAdded, onEntityRemoved);
	}

	public IEnumerable<Tile> tiles => tilesByCoord.Values;

	private void onEntityAdded(Entity entity) {
		var tile = (Tile) entity;
		tilesByCoord[tile.coord] = tile;
	}

	private void onEntityRemoved(Entity entity) {
		var tile = (Tile) entity;
		tilesByCoord.Remove(tile.coord);
	}

	public IEnumerable<Tile> GetTiles() {
		foreach (Tile tile in tilesByCoord.Values) {
			yield return tile;
		}
	}

	public Tile GetTile(Hex coord) {
		return tilesByCoord[coord];
	}

	public bool IsValidTile(Hex coord) {
		return coord.col >= 0 && coord.row >= 0 && coord.col < manager.state.worldSize.col && coord.row < manager.state.worldSize.row;
	}
}
