using System.Collections.Generic;
using System.Linq;
using RelEcs;

public class World {
	private readonly GameManager manager;
	public List<Entity> tiles = new List<Entity>();
	private Dictionary<Hex, Entity> tileByHex;
	private Point size;
	private Dictionary<Entity, Entity[]> neighbors = new Dictionary<Entity, Entity[]>();

	public World(GameManager manager) {
		this.manager = manager;
	}

	public void initWorld(Point size) {
		this.size = size;
		tileByHex = new Dictionary<Hex, Entity>();
	}

	public void AddTile(Hex coord, TileData tileData) {
		var tile = manager.state.Spawn();
		tile.Add<Hex>(coord);
		tile.Add<TileData>(tileData);
		tileByHex.Add(coord, tile);
		tiles.Add(tile);
	}

	public Entity GetTile(Hex coord) {
		return tileByHex[coord];
	}

	public Entity[] GetNeighbors(Entity tile) {
		if (neighbors.ContainsKey(tile)) {
			return neighbors[tile];
		} else {
			var tileNeighbors = tile.Get<Hex>()
				.Neighbors()
				.Where(hex => IsValidTile(hex))
				.Select(hex => GetTile(hex))
				.ToArray();
			neighbors.Add(tile, tileNeighbors);
			return tileNeighbors;
		}
	}

	public bool IsValidTile(Hex coord) {
		return tileByHex.ContainsKey(coord);
		// return coord.col >= 0 && coord.row >= 0 && coord.col < size.x && coord.row < size.y;
	}
}