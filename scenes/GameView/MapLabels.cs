using Godot;
using System;
using System.Collections.Generic;

public class MapLabels : Node2D {
	private List<Label> labels = new List<Label>();
	private MapContext mapContext;

	public override void _Ready() {
		mapContext = (MapContext) GetTree().Root.GetNode("MapContext");
		AddLabel("Test Label", new Vector2(0, 0));

		mapContext.OnZoom.Subscribe((float zoom) => {
			foreach (Label label in labels) {
				resizeLabel(label);
			}
		});
	}

	public void AddLabel(string text, Vector2 position) {
		var label = new Label();
		label.Text = text;
		AddChild(label);
		label.Align = Label.AlignEnum.Center;
		label.Valign = Label.VAlign.Center;
		label.RectPivotOffset = label.RectSize / 2;
		label.SetPosition(position - label.RectSize / 2);
		labels.Add(label);
		resizeLabel(label);
	}

	private void resizeLabel(Label label) {
		label.RectScale = new Vector2(mapContext.zoom.Value, mapContext.zoom.Value);
	}
}
