using RelEcs;
using System.Collections.Generic;

public enum TileViewState {
	Unexplored,
	Unobserved,
	Observed,
}

public static class TileViewStateMethods {
	public static int GetTileMapTile(this TileViewState tileViewState) {
		if (tileViewState == TileViewState.Unexplored) {
			return 0;
		} else if (tileViewState == TileViewState.Unobserved) {
			return 1;
		}
		return -1;
	}
}

/**

Handles a world map of view states
Values refer to how many neighboring tiles view state propagates to

*/
public class PolityViewState {
	private readonly GameManager manager;
	public Dictionary<Entity, TileViewState> state = new Dictionary<Entity, TileViewState>();
	private Dictionary<Entity, int> values = new Dictionary<Entity, int>();
	private HashSet<Entity> changedTiles = new HashSet<Entity>();

	public PolityViewState(GameManager manager) {
		this.manager = manager;
	}

	public void setTileValue(Entity tile, int value) {
		changedTiles.Add(tile);
		values[tile] = value;
	}

	private void set(Entity tile, TileViewState viewState) {
		if (state.ContainsKey(tile)) {
			state[tile] = viewState;
		} else {
			state.Add(tile, viewState);
		}
	}

	public void calculate() {
		var layout = manager.state.GetElement<Layout>();
		foreach (var tile in changedTiles) {
			var hex = tile.Get<Hex>();
			var value = values[tile];
			set(tile, TileViewState.Observed);
			foreach (var surroundingHex in hex.Ring(value)) {
				if (manager.world.IsValidTile(surroundingHex)) {
					var surroundingTile = manager.world.GetTile(surroundingHex);
					set(surroundingTile, TileViewState.Observed);
				}
			}
		}
		changedTiles.Clear();
	}
}

/*
Handles fog of war state for each polity
*/
public class MapViewState {
	private readonly GameManager manager;
	private Dictionary<Entity, PolityViewState> polityViewState = new Dictionary<Entity, PolityViewState>();

	public MapViewState(GameManager manager) {
		this.manager = manager;
	}

	public void add(Entity entity) {
		polityViewState.Add(entity, new PolityViewState(manager));
	}

	public void remove(Entity polity) {
		polityViewState.Remove(polity);
	}

	public PolityViewState getViewState(Entity entity) {
		if (!polityViewState.ContainsKey(entity)) {
			Godot.GD.PushError("Polity not added to MapViewState");
		}
		return polityViewState[entity];
	}
}