using RelEcs;
using System.Collections.Generic;

public enum ViewState {
	Unexplored,
	Unobserved,
	Observed,
}

public static class TileViewStateMethods {
	public static int GetTileMapTile(this ViewState tileViewState) {
		if (tileViewState == ViewState.Unexplored) {
			return 0;
		} else if (tileViewState == ViewState.Unobserved) {
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
	private readonly Entity polity;
	private HashSet<Entity> nodeEntities = new HashSet<Entity>();
	public HashSet<Entity> activeTiles = new HashSet<Entity>();

	public PolityViewState(GameManager manager, Entity polity) {
		this.manager = manager;
		this.polity = polity;
	}

	public void addNodeEntity(Entity entityWithNode) {
		nodeEntities.Add(entityWithNode);
	}

	public ViewState get(Entity tile) {
		return tile.Get<TileViewState>().politiesToViewStates[polity];
	}

	public void set(Entity tile, ViewState viewState) {
		tile.Get<TileViewState>().politiesToViewStates[polity] = viewState;
	}

	public void calculate() {
		var layout = manager.state.GetElement<Layout>();

		// set all previously explored tiles to Unobserved
		foreach (var tile in activeTiles) {
			set(tile, ViewState.Unobserved);
		}

		// mark all tiles within range of nodes as observed
		foreach(var nodeEntity in nodeEntities) {
			var hex = nodeEntity.Get<Hex>();
			var viewStateNode = nodeEntity.Get<ViewStateNode>();
			var tile = manager.world.GetTile(hex);

			set(tile, ViewState.Observed);
			foreach (var surroundingHex in hex.Bubble(viewStateNode.range)) {
				if (manager.world.IsValidTile(surroundingHex)) {
					var surroundingTile = manager.world.GetTile(surroundingHex);
					activeTiles.Add(surroundingTile);
					set(surroundingTile, ViewState.Observed);
				}
			}
		}
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

	public void add(Entity polity) {
		polityViewState.Add(polity, new PolityViewState(manager, polity));
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