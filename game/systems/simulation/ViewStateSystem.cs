using System.Collections.Generic;
using RelEcs;
using Godot;

public class ViewStatePlaySystem : ISystem {
	public void Run(Commands commands) {
		var gameMap = commands.GetElement<GameMap>();
		var mapViewState = commands.GetElement<ViewStateService>();
		var player = commands.GetElement<Player>();

		commands.Receive((CountryAdded e) => {
			mapViewState.add(e.country);
		});

		// TODO: remove view state when country removed

		var changedCountryTiles = new Dictionary<Entity, List<Entity>>();
		commands.Receive((ViewStateNodeUpdated e) => {
			var viewStateNode = e.entity.Get<ViewStateNode>();
			mapViewState.getViewState(viewStateNode.country).addNodeEntity(e.entity);
			if (changedCountryTiles.ContainsKey(viewStateNode.country)) {
				changedCountryTiles[viewStateNode.country].Add(e.entity);
			} else {
				changedCountryTiles.Add(viewStateNode.country, new List<Entity> { e.entity });
			}
		});

		foreach (var (country, changedTiles) in changedCountryTiles) {
			GD.PrintS($"(ViewStatePlaySystem) updating view state for Country {country.Get<CountryData>().name}");
			var countryViewState = mapViewState.getViewState(country);
			countryViewState.CalculateChanged(changedTiles);
			if (player.playerCountry == country) {
				foreach (var tile in countryViewState.exploredTiles) {
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
		foreach (var tile in commands.GetElement<WorldService>().tiles) {
			var location = tile.Get<Location>();
			gameMap.viewState.SetCell(location.hex.col, location.hex.row, ViewState.Unexplored.GetTileMapTile());
		}
	}
}