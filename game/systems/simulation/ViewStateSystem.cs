using System.Collections.Generic;
using RelEcs;
using Godot;

public class ViewStatePlaySystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		var gameMap = this.GetElement<GameMap>();
		var player = this.GetElement<Player>();

		var changedCountries = new HashSet<Entity>();
		
		foreach (var e in this.Receive<ViewStateNodeUpdated>()) {
			var viewStateOwner = this.GetTarget<ViewStateOwner>(e.entity);
			changedCountries.Add(viewStateOwner);
		}

		foreach (var country in changedCountries) {
			var countryData = this.GetComponent<CountryData>(country);
			GD.PrintS($"(ViewStatePlaySystem) updating view state for Country {countryData.name}");
			
			var viewStateNodes = this.QueryBuilder().Has<ViewStateNode>().Has<ViewStateOwner>(country).Build();

			countryData.observedHexes.Clear();
			foreach (var entity in viewStateNodes) {
				var viewStateNode = this.GetComponent<ViewStateNode>(entity);
				var location = this.GetComponent<Location>(entity);
				var hexes = getTilesInRange(location.hex, viewStateNode.range);
				countryData.exploredHexes.UnionWith(hexes);
				countryData.observedHexes.UnionWith(hexes);
			}

			if (player.playerCountry == country) {
				foreach (var hex in countryData.exploredHexes) {
					gameMap.viewState.SetCell(hex.col, hex.row, ViewState.Unobserved.GetTileMapTile());
				}

				foreach (var hex in countryData.observedHexes) {
					gameMap.viewState.SetCell(hex.col, hex.row, ViewState.Observed.GetTileMapTile());
				}
			}

			this.Send(new ViewStateUpdated { country = country });
		}
	}

	public HashSet<Hex> getTilesInRange(Hex hex, int range) {
		var world = this.GetElement<WorldService>();
		var results = new HashSet<Hex> { hex };
		foreach (var surroundingHex in hex.Bubble(range)) {
			if (world.IsValidTile(surroundingHex)) {
				results.Add(surroundingHex);
			}
		}
		return results;
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