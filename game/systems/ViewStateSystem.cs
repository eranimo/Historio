using System.Collections.Generic;
using RelEcs;
using Godot;

public class ViewStateSystem : ISystem {
	public void Run(Commands commands) {
		var gameMap = commands.GetElement<GameMap>();
		var mapViewState = commands.GetElement<MapViewState>();
		var player = commands.GetElement<Player>();

		commands.Receive((CountryAdded e) => {
			mapViewState.add(e.country);
		});

		// TODO: remove view state when country removed

		var changedcountries = new HashSet<Entity>();
		commands.Receive((ViewStateNodeUpdated e) => {
			var viewStateNode = e.entity.Get<ViewStateNode>();
			mapViewState.getViewState(viewStateNode.country).addNodeEntity(e.entity);
			changedcountries.Add(viewStateNode.country);
		});

		foreach (var country in changedcountries) {
			var countryViewState = mapViewState.getViewState(country);
			countryViewState.calculate();
			if (player.playerCountry == country) {
				foreach (var tile in countryViewState.activeTiles) {
					var location = tile.Get<Location>();
					var tileViewState = countryViewState.get(tile);
					gameMap.viewState.SetCell(location.hex.col, location.hex.row, tileViewState.GetTileMapTile());
				}
			}
		}
	}
}

public class ViewStateStartupSystem : ISystem {
	public void Run(Commands commands) {
		var gameMap = commands.GetElement<GameMap>();

		// set view state tilemap to all unexplored
		foreach (var tile in commands.GetElement<World>().tiles) {
			var location = tile.Get<Location>();
			gameMap.viewState.SetCell(location.hex.col, location.hex.row, ViewState.Unexplored.GetTileMapTile());
		}
	}
}