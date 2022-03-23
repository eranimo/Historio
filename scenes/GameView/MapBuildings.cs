using Godot;
using System;
using System.Collections.Generic;

public class MapBuildings : Node2D {
	private Dictionary<Hex, Node> coordBuilding = new Dictionary<Hex, Node>();
	private PackedScene BuildingIconScene;
	private GameMap gameMap;

	public MapBuildings() {
		BuildingIconScene = ResourceLoader.Load<PackedScene>("res://scenes/GameView/BuildingIcon.tscn");
	}

	public void RenderMap(GameMap gameMap) {
		this.gameMap = gameMap;
		GD.PrintS("MapBuildings render");
		AddBuilding(new Hex(0, 0));
	}

	public void AddBuilding(Hex hex) {
		var buildingIcon = (Node2D) BuildingIconScene.Instance();
		AddChild(buildingIcon);
		buildingIcon.Position = gameMap.layout.HexToPixel(hex).ToVector();
		coordBuilding.Add(hex, buildingIcon);
	}
}
