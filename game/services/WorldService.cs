using System.Collections.Generic;
using RelEcs;

public class WorldService {
	private readonly GameManager manager;
	public List<Entity> tiles = new List<RelEcs.Entity>();
	private Dictionary<Hex, Entity> tileByHex = new Dictionary<Hex, Entity>();
	private Point size;

	public WorldService(GameManager manager) {
		this.manager = manager;
	}

	public void initWorld(Point size) {
		this.size = size;
	}

	public void AddTile(Hex coord, TileData tileData) {
		var tile = manager.state.Spawn();
		tileByHex.Add(coord, tile);
		tile.Add<Hex>(coord);
		tile.Add<TileData>(tileData);
		tiles.Add(tile);
	}

	public Entity GetTile(Hex coord) {
		return tileByHex[coord];
	}

	public bool IsValidTile(Hex coord) {
		return coord.col >= 0 && coord.row >= 0 && coord.col < size.x && coord.row < size.y;
	}
}