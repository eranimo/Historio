using System;
using System.Linq;
using RelEcs;
using System.Collections.Generic;
using Godot;

public class Pathfinder {
	private readonly GameManager manager;
	private AStar2D aStar;
	private Dictionary<int, Entity> idToTile = new Dictionary<int, Entity>();
	private Dictionary<Entity, int> tileToID = new Dictionary<Entity, int>();


	public Pathfinder(GameManager manager) {
		this.aStar = new AStar2D();
		this.manager = manager;
		this.setup();
	}

	public void setup() {
		var watch = System.Diagnostics.Stopwatch.StartNew();
		aStar.Clear();

		foreach (var tile in manager.world.tiles) {
			var hex = tile.Get<Location>().hex;
			var id = idToTile.Count + 1;
			aStar.AddPoint(id, hex.ToVector());
			idToTile.Add(id, tile);
			tileToID.Add(tile, id);
		}
		foreach (var tile in manager.world.tiles) {
			var tileData = tile.Get<TileData>();
			var hex = tile.Get<Location>().hex;
			foreach (var neighborHex in hex.Neighbors()) {
				if (manager.world.IsValidTile(neighborHex)) {
					var neighborTile = manager.world.GetTile(neighborHex);
					var neighborTileData = neighborTile.Get<TileData>();
					if (
						tileData.IsLand == neighborTileData.IsLand
						|| !tileData.IsLand == !neighborTileData.IsLand
					) {
						aStar.ConnectPoints(tileToID[tile], tileToID[neighborTile]);
					}
				}
			}
		}
		GD.PrintS($"(Pathfinder) setup {watch.ElapsedMilliseconds}ms");
	}

	public IEnumerable<Hex> getPath(Entity from, Entity to) {
		int fromID;
		int toID;
		if (!tileToID.TryGetValue(from, out fromID)) {
			throw new Exception("Tile entity not in pathfinding map");
		}
		if (!tileToID.TryGetValue(to, out toID)) {
			throw new Exception("Tile entity not in pathfinding map");
		}
		var path = aStar.GetPointPath(fromID, toID);
		if (path.Count() == 0) {
			return null;
		}

		return path.Select(vec => Hex.FromVector(vec));
	}

	public float getMovementCost(Hex hex) {
		return manager.world.GetTile(hex).Get<TileData>().movementCost;
	}
}