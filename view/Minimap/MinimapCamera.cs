using Godot;
using System;

public partial class MinimapCamera : Camera2D {
	const float MIN_ZOOM = 0.25f;
	const float START_ZOOM = 0.5f;
	const float MAX_ZOOM = 1f;

	public void setZoom(float zoomFactor, Vector2 mousePosition) {
		var viewportSize = GetViewport().GetTexture().GetSize();
		var prevZoom = Zoom;
		Zoom += Zoom * zoomFactor;
		Zoom = new Vector2(
			Math.Min(MAX_ZOOM, Math.Max(MIN_ZOOM, Zoom.x)),
			Math.Min(MAX_ZOOM, Math.Max(MIN_ZOOM, Zoom.y))
		);
		Position += ((viewportSize * 0.5f) - mousePosition) * (Zoom - prevZoom);
		Position = new Vector2(
			(float) Math.Round(Position.x),
			(float) Math.Round(Position.y)
		);
	}
}
