using RelEcs;

public class BuildingRenderSystem : ISystem {
	public void Run(Commands commands) {
		Godot.GD.PrintS("(BuildingRenderSystem) run");
		var gameMap = commands.GetElement<GameMap>();
		
		commands.Receive((BuildingAdded e) => {
			Godot.GD.PrintS("(BuildingRenderSystem) Add building", e.building.Get<Hex>());
			gameMap.mapBuildings.AddBuilding(e.building);
		});

		commands.Receive((BuildingRemoved e) => {
			Godot.GD.PrintS("(BuildingRenderSystem) Remove building");
			gameMap.mapBuildings.RemoveBuilding(e.building);
		});
	}
}