// center the game map on the player's capital settlement
public class GameMapTickSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		foreach (var e in this.Receive<GameStart>()) {
			centerOnPlayerCountry();
		}

		foreach (var e in this.Receive<PlayerChanged>()) {
			centerOnPlayerCountry();
		}
	}

	private void centerOnPlayerCountry() {
		var gameMap = this.GetElement<GameMap>();
		var player = this.GetElement<Player>();
		var playerOwnedTiles = this.QueryBuilder<Location>()
			.Has<CountryTile>(player.playerCountry)
			.Build();
		var hexes = new List<Hex>();
		foreach (var location in playerOwnedTiles) {
			hexes.Add(location.hex);
		}
		Godot.GD.PrintS("(GameMapTickSystem) Center on player country");
		gameMap.centerCamera(gameMap.layout.Centroid(hexes).ToVector());
	}
}