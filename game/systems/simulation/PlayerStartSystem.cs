public class PlayerStartSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		var gameMap = this.GetElement<GameMap>();
		// center the game map on the player's capital settlement
		var player = this.GetElement<Player>();
		var playerOwnedTiles = this.QueryBuilder<Location>()
			.Has<CountryTile>(player.playerCountry)
			.Build();
		var hexes = new List<Hex>();
		foreach (var location in playerOwnedTiles) {
			hexes.Add(location.hex);
		}
		Godot.GD.PrintS("(PlayerStartSystem) Center on player country territory");
		gameMap.centerCamera(gameMap.layout.Centroid(hexes).ToVector());
	}
}