using Godot;
using System;

public class MinimapIndicator : ColorRect {
	private Rect2 indicatorRect;

	public void updateIndicator(Rect2 rect) {
		indicatorRect = rect;
		Update();
	}

	public override void _Draw() {
		DrawRect(indicatorRect, new Color("#FFFFFF"), false);
	}
}
