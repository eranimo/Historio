using Godot;
using System;

public partial class MinimapIndicator : ColorRect {
	private Rect2 indicatorRect;

	public void updateIndicator(Rect2 rect) {
		indicatorRect = rect;
		QueueRedraw();
	}

	public override void _Draw() {
		DrawRect(indicatorRect, new Color("#FFFFFF"), false);
	}
}
