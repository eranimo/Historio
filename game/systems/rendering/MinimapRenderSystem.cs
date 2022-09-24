using Godot;

public class MinimapRenderSystem : ISystem {
	public void Run(Commands commands) {
		var minimapWorld = commands.GetElement<MinimapWorld>();
		var player = commands.GetElement<Player>();
		var mapViewState = commands.GetElement<MapViewState>();

		commands.Receive((ViewStateUpdated action) => {
			if (action.country == player.playerCountry) {
				// GD.PrintS("(MinimapRenderSystem) view state updated, updating minimap");
				minimapWorld.updateMap();
			}
		});
	}
}