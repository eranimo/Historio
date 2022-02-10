using Godot;
using System;

public class Camera : Camera2D {
	private bool isPanning = false;
	const float ZOOM_SPEED = 0.25f;
	const float MIN_ZOOM = 0.25f;
	const float MAX_ZOOM = 600;

	public override void _Input(InputEvent @event) {
		base._Input(@event);

		if (@event.IsActionPressed("view_zoom_in")) {
			zoomCamera(-ZOOM_SPEED, ((InputEventMouseButton) @event).Position);
		} else if (@event.IsActionReleased("view_zoom_out")) {
			zoomCamera(ZOOM_SPEED, ((InputEventMouseButton) @event).Position);
		}

		if (@event.IsActionPressed("view_pan_mouse")) {
			isPanning = true;
		} else if (@event.IsActionReleased("view_pan_mouse")) {
			isPanning = false;
		}

		if (@event is InputEventMouseMotion && isPanning) {
			var motionEvent = (InputEventMouseMotion) @event;
			Offset -= motionEvent.Relative * Zoom;
		}
	}

	private void zoomCamera(float zoomFactor, Vector2 mousePosition) {
		var viewportSize = GetViewport().Size;
		var prevZoom = Zoom;
		Zoom += Zoom * zoomFactor;
		Zoom = new Vector2(
			Math.Min(MAX_ZOOM, Math.Max(MIN_ZOOM, Zoom.x)),
			Math.Min(MAX_ZOOM, Math.Max(MIN_ZOOM, Zoom.y))
		);
		Offset += ((viewportSize * 0.5f) - mousePosition) * (Zoom - prevZoom);
	}
}
