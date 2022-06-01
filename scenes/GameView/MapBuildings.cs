using Godot;
using RelEcs;
using System;
using System.Collections.Generic;

public class MapBuildings : Node {
	private PackedScene BuildingIconScene;
	private GameMap gameMap;
	private GameManager manager;
	private ViewSystem viewSystem;

	public MapBuildings() {
		BuildingIconScene = ResourceLoader.Load<PackedScene>("res://scenes/GameView/BuildingIcon.tscn");
	}

	class ViewSystem : ISystem {
		void ISystem.Run(Commands commands) {
			var query = commands.Query<BuildingData, Hex>();
			foreach(var (building, coord) in query) {
				GD.PrintS("Building", coord);
			}
		}
	}

	public override void _Ready() {
		var gameView = (GameView) GetTree().Root.GetNode("GameView");
		manager = gameView.game.manager;
		viewSystem = new ViewSystem();
		manager.viewSystems.Add(viewSystem);
	}

	public override void _ExitTree() {
		manager.viewSystems.Remove(viewSystem);
	}

	public void RenderMap(GameMap gameMap) {
		this.gameMap = gameMap;
		GD.PrintS("MapBuildings render");
	}

	// private void onEntityAdded(Entity entity) {
	// 	GD.PrintS("Building added", entity);
	// 	var buildingIcon = (Node2D) BuildingIconScene.Instance();
	// 	AddChild(buildingIcon);
	// 	var building = (Building) entity;
	// 	buildingIcon.Position = gameMap.layout.HexToPixel(building.tile.coord).ToVector();
	// 	coordBuilding.Add(building, buildingIcon);
	// }

	// private void onEntityRemoved(Entity entity) {
	// 	GD.PrintS("Building removed", entity);
	// 	var building = (Building) entity;
	// 	RemoveChild(coordBuilding[building]);
	// }
}
