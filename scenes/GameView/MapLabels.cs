using Godot;
using System;
using System.Collections.Generic;

public class MapLabels : Node2D {
	private HashSet<Label> labels = new HashSet<Label>();

	public void AddLabel(Label label) {
		if (!labels.Contains(label)) {
			AddChild(label);
		}
		labels.Add(label);
	}
}
