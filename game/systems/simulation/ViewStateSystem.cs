using System.Collections.Generic;
using RelEcs;
using Godot;

public class ViewStatePlaySystem : ISystem {
	public void Run(Commands commands) {
		var gameMap = commands.GetElement<GameMap>();
		var mapViewState = commands.GetElement<MapViewState>();
		var player = commands.GetElement<Player>();

		commands.Receive((CountryAdded e) => {
			mapViewState.add(e.country);
		});

		// TODO: remove view state when country removed

		var changedCountries = new HashSet<Entity>();
		commands.Receive((ViewStateNodeUpdated e) => {
			var viewStateNode = e.entity.Get<ViewStateNode>();
			mapViewState.getViewState(viewStateNode.country).addNodeEntity(e.entity);
			changedCountries.Add(viewStateNode.country);
		});

		foreach (var country in changedCountries) {
			var countryViewState = mapViewState.getViewState(country);
			countryViewState.calculate();
			// GD.PrintS($"(ViewStateSystem) calculate view state for country {country.Get<CountryData>().name}");
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

public class ViewStateStartSystem : ISystem {
	public void Run(Commands commands) {
		var gameMap = commands.GetElement<GameMap>();

		// set view state tilemap to all unexplored
		foreach (var tile in commands.GetElement<World>().tiles) {
			var location = tile.Get<Location>();
			gameMap.viewState.SetCell(location.hex.col, location.hex.row, ViewState.Unexplored.GetTileMapTile());
		}
	}
}