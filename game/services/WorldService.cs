using System.Collections.Generic;
using System.Linq;
using RelEcs;

public class WorldService {
	private readonly GameManager manager;
	public List<Entity> tiles = new List<Entity>();
	private Dictionary<Hex, Entity> tileByHex;
	private Point size;
	private Dictionary<Entity, Entity[]> neighbors = new Dictionary<Entity, Entity[]>();
	private Dictionary<Entity, HashSet<Entity>> entitiesByTile = new Dictionary<Entity, HashSet<Entity>>();

	public WorldService(GameManager manager) {
		this.manager = manager;
	}

	public void initWorld(Point size) {
		this.size = size;
		tileByHex = new Dictionary<Hex, Entity>();
	}

	public void AddTile(Hex coord, TileData tileData) {
		var tile = manager.state.Spawn();
		tile.Add<Location>(new Location { hex = coord });
		tile.Add<TileData>(tileData);
		tile.Add(new TileViewState());
		tileByHex.Add(coord, tile);
		tiles.Add(tile);
	}

	public Entity GetTile(Hex coord) {
		return tileByHex[coord];
	}

	public HashSet<Entity> entitiesAtTile(Hex coord) {
		var tile = GetTile(coord);
		if (!entitiesByTile.ContainsKey(tile)) {
			entitiesByTile.Add(tile, new HashSet<Entity>());
		}
		return entitiesByTile[GetTile(coord)];
	}

	public void moveEntity(Entity entity, Hex nextHex) {
		var loc = entity.Get<Location>();
		var hex = loc.hex;
		var currentTile = GetTile(hex);
		var nextTile = GetTile(nextHex);
		if (!entitiesByTile.ContainsKey(currentTile)) {
			entitiesByTile.Add(currentTile, new HashSet<Entity>());
		}
		if (!entitiesByTile.ContainsKey(nextTile)) {
			entitiesByTile.Add(nextTile, new HashSet<Entity>());
		}
		var entityList = entitiesByTile[currentTile];
		if (entityList.Contains(entity)) {
			entityList.Remove(entity);
		}
		loc.hex = nextHex;
		entitiesByTile[nextTile].Add(entity);
	}

	public Entity[] GetNeighbors(Entity tile) {
		if (neighbors.ContainsKey(tile)) {
			return neighbors[tile];
		} else {
			var tileNeighbors = tile.Get<Location>().hex
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