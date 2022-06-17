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
public class CountryViewState {
	private readonly GameManager manager;
	private readonly Entity country;
	private HashSet<Entity> nodeEntities = new HashSet<Entity>();
	public HashSet<Entity> activeTiles = new HashSet<Entity>();

	public CountryViewState(GameManager manager, Entity country) {
		this.manager = manager;
		this.country = country;
	}

	public void addNodeEntity(Entity entityWithNode) {
		nodeEntities.Add(entityWithNode);
	}

	public ViewState get(Entity tile) {
		return tile.Get<TileViewState>().countriesToViewStates[country];
	}

	public void set(Entity tile, ViewState viewState) {
		tile.Get<TileViewState>().countriesToViewStates[country] = viewState;
	}

	public void exploreAt(Entity tile, int range) {
		var hex = tile.Get<Location>().hex;
		set(tile, ViewState.Observed);
		activeTiles.Add(tile);
		foreach (var surroundingHex in hex.Bubble(range)) {
			if (manager.world.IsValidTile(surroundingHex)) {
				var surroundingTile = manager.world.GetTile(surroundingHex);
				activeTiles.Add(surroundingTile);
				set(surroundingTile, ViewState.Observed);
			}
		}
	}

	public void calculate() {
		var layout = manager.state.GetElement<Layout>();

		// set all previously explored tiles to Unobserved
		foreach (var tile in activeTiles) {
			set(tile, ViewState.Unobserved);
		}

		// mark all tiles within range of nodes as observed
		foreach(var nodeEntity in nodeEntities) {
			var hex = nodeEntity.Get<Location>().hex;
			var viewStateNode = nodeEntity.Get<ViewStateNode>();
			var tile = manager.world.GetTile(hex);
			exploreAt(tile, viewStateNode.range);
		}
	}
}

/*
Handles fog of war state for each country
*/
public class MapViewState {
	private readonly GameManager manager;
	private Dictionary<Entity, CountryViewState> countryViewState = new Dictionary<Entity, CountryViewState>();

	public MapViewState(GameManager manager) {
		this.manager = manager;
	}

	public void add(Entity country) {
		countryViewState.Add(country, new CountryViewState(manager, country));
	}

	public void remove(Entity country) {
		countryViewState.Remove(country);
	}

	public CountryViewState getViewState(Entity entity) {
		if (!countryViewState.ContainsKey(entity)) {
			Godot.GD.PushError("Country not added to MapViewState");
		}
		return countryViewState[entity];
	}
}