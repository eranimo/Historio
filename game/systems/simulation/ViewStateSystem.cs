using System.Collections.Generic;
using RelEcs;
using Godot;

public class ViewStatePlaySystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		var gameMap = this.GetElement<GameMap>();
		var mapViewState = this.GetElement<ViewStateService>();
		var player = this.GetElement<Player>();

		foreach (var e in this.Receive<CountryAdded>()) {
			mapViewState.add(e.country);
		}

		// TODO: remove view state when country removed

		var changedCountryTiles = new Dictionary<Entity, List<Entity>>();
		
		foreach (var e in this.Receive<ViewStateNodeUpdated>()) {
			var viewStateNode = this.GetComponent<ViewStateNode>(e.entity);
			mapViewState.getViewState(viewStateNode.country).addNodeEntity(e.entity);
			if (changedCountryTiles.ContainsKey(viewStateNode.country)) {
				changedCountryTiles[viewStateNode.country].Add(e.entity);
			} else {
				changedCountryTiles.Add(viewStateNode.country, new List<Entity> { e.entity });
			}
		}

		foreach (var (country, changedTiles) in changedCountryTiles) {
			GD.PrintS($"(ViewStatePlaySystem) updating view state for Country {this.GetComponent<CountryData>(country).name}");
			var countryViewState = mapViewState.getViewState(country);
			countryViewState.CalculateChanged(changedTiles);
			if (player.playerCountry == country) {
				foreach (var tile in countryViewState.exploredTiles) {
					var location = this.GetComponent<Location>(tile);
					var tileViewState = countryViewState.get(tile);
					gameMap.viewState.SetCell(location.hex.col, location.hex.row, tileViewState.GetTileMapTile());
				}
			}
		}
	}
}

public class ViewStateStartSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		var gameMap = this.GetElement<GameMap>();

		// set view state tilemap to all unexplored
		foreach (var tile in this.GetElement<WorldService>().tiles) {
			var location = this.GetComponent<Location>(tile);
			gameMap.viewState.SetCell(location.hex.col, location.hex.row, ViewState.Unexplored.GetTileMapTile());
		}
	}
}