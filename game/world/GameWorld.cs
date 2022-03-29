using Godot;
using System;
using System.Collections;
using System.Collections.Generic;


public class GameWorld : GameSystem {
	private Dictionary<Hex, Tile> tilesByCoord = new Dictionary<Hex, Tile>();

	public GameWorld(GameManager manager) : base(manager) { }

	public override bool Query(Entity entity) => entity.GetType() == typeof(Tile);

	public override void OnStart() {
		GD.PrintS("(GameWorld) start");
	}

	public IEnumerable<Tile> tiles => tilesByCoord.Values;

	public override void OnEntityAdded(Entity entity) {
		var tile = (Tile) entity;
		tilesByCoord[tile.coord] = tile;
	}

	public override void OnEntityRemoved(Entity entity) {
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
