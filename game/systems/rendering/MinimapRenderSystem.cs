using Godot;

public partial class MinimapRenderSystem : ISystem {
	public RelEcs.World World { get; set; }

	public void Run() {
		var minimapWorld = World.GetElement<MinimapWorld>();
		var player = World.GetElement<Player>();

		foreach (var action in World.Receive<ViewStateUpdated>(this)) {
			if (action.country == player.playerCountry) {
				updateMap(minimapWorld, player);
			}
		}

		foreach (var action in World.Receive<PlayerChanged>(this)) {
			updateMap(minimapWorld, player);
		}
	}

	private static void updateMap(MinimapWorld minimapWorld, Player player) {
		GD.PrintS("(MinimapRenderSystem) updating minimap");
		minimapWorld.updateMap();
	}
}