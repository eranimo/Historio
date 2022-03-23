using Godot;
using System;
using System.Collections;
using System.Collections.Generic;


public class GameWorld : GameSystem {
	public readonly List<Tile> tiles;
	private Dictionary<Hex, Tile> tilesByCoord = new Dictionary<Hex, Tile>();

	public GameWorld(GameManager manager) : base(manager) { }

	public override void OnStart() {
		GD.PrintS("(GameWorld) start");
		foreach (Tile tile in manager.GetEntitiesByType(typeof(Tile))) {
			tilesByCoord[tile.coord] = tile;
		}
		manager.OnEntityAdded.Subscribe((Entity entity) => {
			if (entity is Tile tile) {
				tilesByCoord[tile.coord] = tile;
			}
		});
		manager.OnEntityRemoved.Subscribe((Entity entity) => {
			if (entity is Tile tile) {
				tilesByCoord.Remove(tile.coord);
			}
		});
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
