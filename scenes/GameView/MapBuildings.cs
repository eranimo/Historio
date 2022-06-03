using Godot;
using RelEcs;
using System;
using System.Collections.Generic;

public class MapBuildings : Node {
	private PackedScene buildingIcon;
	private Dictionary<RelEcs.Entity, BuildingIcon> buildingIconNodes = new Dictionary<Entity, BuildingIcon>();
	private Layout layout;

	public MapBuildings() {
		buildingIcon = ResourceLoader.Load<PackedScene>("res://scenes/GameView/BuildingIcon.tscn");
	}

	public void InitMap(Layout layout) {
		this.layout = layout;
	}

	public void AddBuilding(RelEcs.Entity building) {
		var coord = building.Get<Hex>();
		var buildingData = building.Get<BuildingData>();
		var icon = buildingIcon.Instance<BuildingIcon>();
		icon.Position = layout.HexToPixel(coord).ToVector();
		buildingIconNodes.Add(building, icon);
		AddChild(icon);
	}

	public void RemoveBuilding(RelEcs.Entity building) {
		if (!buildingIconNodes.ContainsKey(building)) {
			throw new Exception("Building not added");
		}
		RemoveChild(buildingIconNodes[building]);
	}
}
