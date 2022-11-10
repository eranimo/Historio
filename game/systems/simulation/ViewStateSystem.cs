using System.Collections.Generic;
using RelEcs;
using Godot;

public partial class ViewStateSystem : ISystem {
	public RelEcs.World World { get; set; }

	private void recalculateCountryViewState(Entity country) {
		var countryData = World.GetComponent<CountryData>(country);
		GD.PrintS($"(ViewStateSystem) updating view state for Country {countryData.name}");

		var viewStateNodes = World.Query().Has<ViewStateNode>().Has<ViewStateOwner>(country).Build();

		countryData.observedHexes.Clear();
		foreach (var entity in viewStateNodes) {
			var viewStateNode = World.GetComponent<ViewStateNode>(entity);
			var location = World.GetComponent<Location>(entity);
			var hexes = getTilesInRange(location.hex, viewStateNode.range);
			countryData.exploredHexes.UnionWith(hexes);
			countryData.observedHexes.UnionWith(hexes);
		}

		World.Send(new ViewStateUpdated { country = country });
	}

	private void updatePlayerCountryViewState() {
		var player = World.GetElement<Player>();
		var gameMap = World.GetElement<GameMap>();

		if (player.playerCountry is null) {
			GD.PrintS("(ViewStateSystem) update view state for observer");
			foreach (var tile in World.GetElement<WorldService>().tiles) {
				var location = World.GetComponent<Location>(tile);
				gameMap.viewState.SetCell(0, new Vector2i(location.hex.col, location.hex.row), ViewState.Observed.GetTileMapTile());
			}
		} else {
			var countryData = World.GetComponent<CountryData>(player.playerCountry);
			// GD.PrintS($"(ViewStateSystem) update view state for {countryData.name}");
			gameMap.viewState.Clear();

			foreach (var tile in World.GetElement<WorldService>().tiles) {
				var location = World.GetComponent<Location>(tile);
				gameMap.viewState.SetCell(0, new Vector2i(location.hex.col, location.hex.row), ViewState.Unexplored.GetTileMapTile());
			}

			foreach (var hex in countryData.exploredHexes) {
				gameMap.viewState.SetCell(0, new Vector2i(hex.col, hex.row), ViewState.Unobserved.GetTileMapTile());
			}

			foreach (var hex in countryData.observedHexes) {
				gameMap.viewState.SetCell(0, new Vector2i(hex.col, hex.row), ViewState.Observed.GetTileMapTile());
			}
		}
	}

	public void Run() {
		var player = World.GetElement<Player>();
		var changedCountries = new HashSet<Entity>();
		
		foreach (var e in World.Receive<ViewStateNodeUpdated>(this)) {
			var viewStateOwner = World.GetTarget<ViewStateOwner>(e.entity);
			changedCountries.Add(viewStateOwner);
		}

		foreach (var country in changedCountries) {
			recalculateCountryViewState(country);

			if (player.playerCountry == country) {
				updatePlayerCountryViewState();
			}
		}

		foreach (var e in World.Receive<PlayerChanged>(this)) {
			updatePlayerCountryViewState();
		}
	}

	public HashSet<Hex> getTilesInRange(Hex hex, int range) {
		var world = World.GetElement<WorldService>();
		var results = new HashSet<Hex> { hex };
		foreach (var surroundingHex in hex.Bubble(range)) {
			if (world.IsValidTile(surroundingHex)) {
				results.Add(surroundingHex);
			}
		}
		return results;
	}
}