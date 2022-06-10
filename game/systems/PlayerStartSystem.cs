using Godot;
using RelEcs;
using System.Collections.Generic;
using System.Linq;

public class PlayerStartSystem : ISystem {
	public void Run(Commands commands) {
		var gameMap = commands.GetElement<GameMap>();
		// center the game map on the player's capital settlement
		var player = commands.GetElement<Player>();
		var capitals = commands.Query<Entity, SettlementData>().Has<CapitalSettlement>(player.playerPolity);
		foreach (var (capital, settlementData) in capitals) {
			var tilesInSettlement = commands.Query<Location>().Has<SettlementTile>(capital);
			var hexes = new List<Hex>();
			foreach (var location in tilesInSettlement) {
				hexes.Add(location.hex);
			}
			gameMap.centerCamera(gameMap.layout.Centroid(hexes).ToVector());
		}
	}
}