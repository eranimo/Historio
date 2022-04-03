using Godot;
using System;
using System.Collections.Generic;

public class MapBuildings : Node {
	private Dictionary<Building, Node> coordBuilding = new Dictionary<Building, Node>();
	private PackedScene BuildingIconScene;
	private GameMap gameMap;
	private EntityQuery<Building> query;

	public MapBuildings() {
		BuildingIconScene = ResourceLoader.Load<PackedScene>("res://scenes/GameView/BuildingIcon.tscn");
	}

	public override void _Ready() {
		var gameContext = (GameContext) GetTree().Root.GetNode("GameContext");
		var manager = gameContext.game.manager;
		query = manager.Query<Building>().Execute(onEntityAdded, onEntityRemoved);
	}

	public override void _ExitTree() {
		query.Dispose();
	}

	public void RenderMap(GameMap gameMap) {
		this.gameMap = gameMap;
		GD.PrintS("MapBuildings render");
	}

	private void onEntityAdded(Entity entity) {
		GD.PrintS("Building added", entity);
		var buildingIcon = (Node2D) BuildingIconScene.Instance();
		AddChild(buildingIcon);
		var building = (Building) entity;
		buildingIcon.Position = gameMap.layout.HexToPixel(building.tile.coord).ToVector();
		coordBuilding.Add(building, buildingIcon);
	}

	private void onEntityRemoved(Entity entity) {
		GD.PrintS("Building removed", entity);
		var building = (Building) entity;
		RemoveChild(coordBuilding[building]);
	}
}
