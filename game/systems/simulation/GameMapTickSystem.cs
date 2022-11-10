// center the game map on the player's capital settlement
public partial class GameMapTickSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		foreach (var e in World.Receive<GameStart>(this)) {
			centerOnPlayerCountry();
		}

		foreach (var e in World.Receive<PlayerChanged>(this)) {
			centerOnPlayerCountry();
		}
	}

	private void centerOnPlayerCountry() {
		var gameMap = World.GetElement<GameMap>();
		var player = World.GetElement<Player>();
		var playerOwnedTiles = World.Query<Location>()
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