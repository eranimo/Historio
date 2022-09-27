using Godot;

public class MinimapRenderSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		var minimapWorld = this.GetElement<MinimapWorld>();
		var player = this.GetElement<Player>();
		var mapViewState = this.GetElement<ViewStateService>();

		foreach (var action in this.Receive<ViewStateUpdated>()) {
			if (action.country == player.playerCountry) {
				// GD.PrintS("(MinimapRenderSystem) view state updated, updating minimap");
				minimapWorld.updateMap();
			}
		}
	}
}