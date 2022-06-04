using System.Collections.Generic;
using RelEcs;
using Godot;

public class ViewStateSystem : ISystem {
	public void Run(Commands commands) {
		var gameMap = commands.GetElement<GameMap>();
		var mapViewState = commands.GetElement<MapViewState>();
		var player = commands.GetElement<Player>();

		commands.Receive((PolityAdded e) => {
			mapViewState.add(e.polity);
		});

		// TODO: remove view state when polity removed

		var changedPolities = new HashSet<Entity>();
		commands.Receive((TerritoryTileUpdate e) => {
			var territoryData = e.territory.Get<TerritoryData>();
			mapViewState.getViewState(territoryData.ownerPolity).setTileValue(e.tile, 3);
			changedPolities.Add(territoryData.ownerPolity);
		});

		foreach (var polity in changedPolities) {
			var polityViewState = mapViewState.getViewState(polity);
			polityViewState.calculate();
			if (player.playerPolity == polity) {
				foreach (var (tile, tileViewState) in polityViewState.state) {
					var hex = tile.Get<Hex>();
					gameMap.viewState.SetCell(hex.col, hex.row, tileViewState.GetTileMapTile());
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
			var hex = tile.Get<Hex>();
			gameMap.viewState.SetCell(hex.col, hex.row, TileViewState.Unexplored.GetTileMapTile());
		}
	}
}