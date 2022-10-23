using Godot;

public class MinimapRenderSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		var minimapWorld = this.GetElement<MinimapWorld>();
		var player = this.GetElement<Player>();

		foreach (var action in this.Receive<ViewStateUpdated>()) {
			if (action.country == player.playerCountry) {
				updateMap(minimapWorld, player);
			}
		}

		foreach (var action in this.Receive<PlayerChanged>()) {
			updateMap(minimapWorld, player);
		}
	}

	private static void updateMap(MinimapWorld minimapWorld, Player player) {
		GD.PrintS("(MinimapRenderSystem) updating minimap");
		minimapWorld.updateMap();
	}
}