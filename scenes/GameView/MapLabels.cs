using Godot;
using System;
using System.Collections.Generic;

public partial class MapLabels : Node2D {
	private PackedScene mapLabelScene;
	private Dictionary<Entity, MapLabel> entityMapLabels = new Dictionary<Entity, MapLabel>();

	public override void _Ready() {
		base._Ready();
		mapLabelScene = ResourceLoader.Load<PackedScene>("res://scenes/GameView/MapLabel.tscn");
	}

	public void SetLabel(Entity entity, MapLabel.MapLabelType type, string text, Vector2 position) {
		MapLabel label = GetLabel(entity);
		label.LabelType = type;
		label.Text = text;
		label.SetPosition(position);
	}

	public MapLabel GetLabel(Entity entity) {
		MapLabel label;
		if (entityMapLabels.ContainsKey(entity)) {
			label = entityMapLabels[entity];
		} else {
			label = mapLabelScene.Instantiate<MapLabel>();
			AddChild(label);
		}

		return label;
	}
}
