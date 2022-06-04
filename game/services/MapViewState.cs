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

public class PolityViewState {
	private readonly GameManager manager;
	private Dictionary<Entity, TileViewState> state = new Dictionary<Entity, TileViewState>();

	public PolityViewState(GameManager manager) {
		this.manager = manager;
	}

	public void set(Entity tile, TileViewState viewState) {
		state[tile] = viewState;
	}

	public TileViewState get(Entity tile) {
		if (state.ContainsKey(tile)) {
			return state[tile];
		}
		return TileViewState.Unexplored;
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