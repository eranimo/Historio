using RelEcs;

public class ViewStateSystem : ISystem {
	public void Run(Commands commands) {
		var gameMap = commands.GetElement<GameMap>();
		var mapViewState = commands.GetElement<MapViewState>();

		commands.Receive((PolityAdded e) => {
			mapViewState.add(e.polity);
		});

		// TODO: remove view state when polity removed

		commands.Receive((TileViewStateUpdated e) => {
			var territory = e.tile.Get<TerritoryTile>().territory;
			var polity = territory.Get<TerritoryData>().ownerPolity;
			mapViewState.getViewState(polity).set(e.tile, e.viewState);
			// TODO: update view state tilemap if polity is player
		});
	}
}

public class ViewStateStartupSystem : ISystem {
	public void Run(Commands commands) {
		var gameMap = commands.GetElement<GameMap>();
		var mapViewState = commands.GetElement<MapViewState>();
		var player = commands.GetElement<Player>();


		// set view state tilemap to all unexplored
		foreach (var tile in commands.GetElement<World>().tiles) {
			var hex = tile.Get<Hex>();
			gameMap.viewState.SetCell(hex.col, hex.row, TileViewState.Unexplored.GetTileMapTile());
		}

		var query = commands.Query<Entity, TerritoryTile>();
		foreach (var (tile, territoryTile) in query) {
			var territoryData = territoryTile.territory.Get<TerritoryData>();
			mapViewState.getViewState(territoryData.ownerPolity).set(tile, TileViewState.Observed);
			if (player.playerPolity == territoryData.ownerPolity) {
				var hex = tile.Get<Hex>();
				var tileID = TileViewState.Observed.GetTileMapTile();
				gameMap.viewState.SetCell(hex.col, hex.row, tileID);
			}
		}
	}
}