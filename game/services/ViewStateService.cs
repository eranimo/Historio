using MessagePack;
using RelEcs;
using System;
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
	public HashSet<Entity> exploredTiles = new HashSet<Entity>();
	public HashSet<Entity> observedTiles = new HashSet<Entity>();
	public HashSet<Entity> unobservedTiles = new HashSet<Entity>();
	public Dictionary<Entity, HashSet<Entity>> viewStateEntityTiles = new Dictionary<Entity, HashSet<Entity>>();
	public Dictionary<Entity, HashSet<Entity>> tileViewStateEntities = new Dictionary<Entity, HashSet<Entity>>();

	public CountryViewState(GameManager manager, Entity country) {
		this.manager = manager;
		this.country = country;
	}

	public void addNodeEntity(Entity entityWithNode) {
		nodeEntities.Add(entityWithNode);
	}

	public ViewState get(Entity tile) {
		if (!has(tile)) {
			set(tile, ViewState.Unexplored);
			return ViewState.Unexplored;
		}
		return manager.Get<TileViewState>(tile).countriesToViewStates[country];
	}

	public bool has(Entity tile) {
		return manager.Get<TileViewState>(tile).countriesToViewStates.ContainsKey(country);
	}

	public void set(Entity tile, ViewState viewState) {
		manager.Get<TileViewState>(tile).countriesToViewStates[country] = viewState;
	}

	public HashSet<Entity> getTilesInRange(Entity tile, int range) {
		var hex = manager.Get<Location>(tile).hex;
		var results = new HashSet<Entity> { tile };
		foreach (var surroundingHex in hex.Bubble(range)) {
			if (manager.world.IsValidTile(surroundingHex)) {
				var surroundingTile = manager.world.GetTile(surroundingHex);
				results.Add(surroundingTile);
			}
		}
		return results;
	}

	public void CalculateChanged(List<Entity> changedEntities) {
		var watch = System.Diagnostics.Stopwatch.StartNew();
		var layout = manager.state.GetElement<Layout>();

		foreach (var nodeEntity in changedEntities) {
			// set all tiles previously calculated for this view state entity to unobserved
			// unless they belong to another view state entity
			if (viewStateEntityTiles.ContainsKey(nodeEntity)) {
				foreach (var tile in viewStateEntityTiles[nodeEntity]) {
					if (tileViewStateEntities[tile].Count == 1) {
						set(tile, ViewState.Unobserved);
					}
					tileViewStateEntities[tile].Remove(nodeEntity);
				}
			}

			var hex = manager.Get<Location>(nodeEntity).hex;
			var viewStateNode = manager.Get<ViewStateNode>(nodeEntity);
			var currentTile = manager.world.GetTile(hex);
			var tiles = getTilesInRange(currentTile, viewStateNode.range);

			exploredTiles.UnionWith(tiles);
			
			foreach(var tile in tiles) {
				if (tileViewStateEntities.ContainsKey(tile)) {
					tileViewStateEntities[tile].Add(nodeEntity);
				} else {
					tileViewStateEntities[tile] = new HashSet<Entity> { nodeEntity };
				}
				set(tile, ViewState.Observed);
			}

			viewStateEntityTiles[nodeEntity] = tiles;
		}

		manager.state.Send(new ViewStateUpdated { country = country });
		watch.Stop();
		// Godot.GD.PrintS($"(ViewStateSystem) CalculateChanged ({changedEntities.Count} changed) in {watch.ElapsedMilliseconds} ms");
	}
}

/*
Handles fog of war state for each country
*/
public class ViewStateService {
	private readonly GameManager manager;
	private Dictionary<Entity, CountryViewState> countryViewState = new Dictionary<Entity, CountryViewState>();

	public ViewStateService(GameManager manager) {
		this.manager = manager;
	}

	public void add(Entity country) {
		countryViewState.Add(country, new CountryViewState(manager, country));
	}

	public void remove(Entity country) {
		countryViewState.Remove(country);
	}

	public bool has(Entity country) {
		return countryViewState.ContainsKey(country);
	}

	public CountryViewState getViewState(Entity country) {
		if (!countryViewState.ContainsKey(country)) {
			Godot.GD.PushError("Country not added to MapViewState");
		}
		return countryViewState[country];
	}
}