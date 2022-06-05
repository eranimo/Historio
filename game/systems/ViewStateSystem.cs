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
		commands.Receive((ViewStateNodeUpdated e) => {
			var viewStateNode = e.entity.Get<ViewStateNode>();
			mapViewState.getViewState(viewStateNode.polity).addNodeEntity(e.entity);
			changedPolities.Add(viewStateNode.polity);
		});

		foreach (var polity in changedPolities) {
			var polityViewState = mapViewState.getViewState(polity);
			polityViewState.calculate();
			if (player.playerPolity == polity) {
				foreach (var tile in polityViewState.activeTiles) {
					var location = tile.Get<Location>();
					var tileViewState = polityViewState.get(tile);
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