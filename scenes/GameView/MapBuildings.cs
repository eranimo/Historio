using Godot;
using System;
using System.Collections.Generic;

public class MapBuildings : EntityView {
	private Dictionary<Building, Node> coordBuilding = new Dictionary<Building, Node>();
	private PackedScene BuildingIconScene;
	private GameMap gameMap;

	public MapBuildings() : base(typeof(Building)) {
		BuildingIconScene = ResourceLoader.Load<PackedScene>("res://scenes/GameView/BuildingIcon.tscn");
	}

	public void RenderMap(GameMap gameMap) {
		this.gameMap = gameMap;
		GD.PrintS("MapBuildings render");
	}

	public override void OnEntityAdded(Entity entity) {
		GD.PrintS("Building added", entity);
		var buildingIcon = (Node2D) BuildingIconScene.Instance();
		AddChild(buildingIcon);
		var building = (Building) entity;
		buildingIcon.Position = gameMap.layout.HexToPixel(building.tile.coord).ToVector();
		coordBuilding.Add(building, buildingIcon);
	}

	public override void OnEntityRemoved(Entity entity) {
		GD.PrintS("Building removed", entity);
		var building = (Building) entity;
		RemoveChild(coordBuilding[building]);
	}
}
