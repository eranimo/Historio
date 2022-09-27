public class PlayerStartSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		var gameMap = this.GetElement<GameMap>();
		// center the game map on the player's capital settlement
		var player = this.GetElement<Player>();
		var capitals = this.QueryBuilder<Entity, SettlementData>().Has<CapitalSettlement>(player.playerCountry).Build();
		foreach (var (capital, settlementData) in capitals) {
			var tilesInSettlement = this.QueryBuilder<Location>().Has<SettlementTile>(capital).Build();
			var hexes = new List<Hex>();
			foreach (var location in tilesInSettlement) {
				hexes.Add(location.hex);
			}
			gameMap.centerCamera(gameMap.layout.Centroid(hexes).ToVector());
		}
	}
}