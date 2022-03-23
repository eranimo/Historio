using Godot;
using System;
using System.Collections.Generic;

public class MapBuildings : Node2D {
	private Dictionary<OffsetCoord, Node> coordBuilding = new Dictionary<OffsetCoord, Node>();
	private PackedScene BuildingIconScene;
	private GameMap gameMap;

	public MapBuildings() {
		BuildingIconScene = ResourceLoader.Load<PackedScene>("res://scenes/GameView/BuildingIcon.tscn");
	}

	public void RenderMap(GameMap gameMap) {
		this.gameMap = gameMap;
		GD.PrintS("MapBuildings render");
		AddBuilding(new OffsetCoord(0, 0));
	}

	public void AddBuilding(OffsetCoord coord) {
		var buildingIcon = (Node2D) BuildingIconScene.Instance();
		Hex hex = OffsetCoord.QoffsetToCube(OffsetCoord.ODD, coord);
		AddChild(buildingIcon);
		buildingIcon.Position = gameMap.layout.HexToPixel(hex).ToVector() - gameMap.layout.origin.ToVector();
		coordBuilding.Add(coord, buildingIcon);
	}
}
