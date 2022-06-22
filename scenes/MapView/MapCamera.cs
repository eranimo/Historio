using Godot;
using System;

public class MapCamera : Godot.Camera {
	const float PanningSpeed = 1.0f;
	const float ZoomSpeed = 5.0f;
	const float MoveSpeed = 25.0f;

	bool panning = false;
	bool changing_camera = false;

	public override void _Ready() {

	}

	public override void _Input(InputEvent @event) {
		if (@event.IsActionPressed("view_pan_mouse")) {
			panning = true;
		} else if (@event.IsActionReleased("view_pan_mouse")) {
			panning = false;
		}

		if (@event is InputEventMouseMotion && panning) {
			var mouseMotion = (InputEventMouseMotion) @event;
			var axis = new Vector3(0, 0, 0).Normalized();
			var newTranslation = mouseMotion.Relative.Rotated(-Rotation.y);
			Translation -= new Vector3(newTranslation.x, 0, newTranslation.y);
		}

		if (@event.IsAction("ui_left")) {
			Translation -= new Vector3(MoveSpeed, 0f, 0f);
		} else if (@event.IsAction("ui_right")) {
			Translation += new Vector3(MoveSpeed, 0f, 0f);
		} else if (@event.IsAction("ui_up")) {
			Translation -= new Vector3(0f, 0f, MoveSpeed);
		} else if (@event.IsAction("ui_down")) {
			Translation += new Vector3(0f, 0f, MoveSpeed);
		}

		if (@event.IsActionReleased("view_zoom_in")) {
			Translation -= new Vector3(0, ZoomSpeed, 0);
		} else if (@event.IsActionReleased("view_zoom_out")) {
			Translation += new Vector3(0, ZoomSpeed, 0);
		}
		// translation.y = clamp(translation.y, 1, 500)
		Translation = new Vector3(
			Translation.x,
			(float) Math.Min(Math.Max(Translation.y, 1), 500),
			Translation.z
		);

		if (@event.IsActionPressed("view_camera_change")) {
			changing_camera = true;
		} else if (@event.IsActionReleased("view_camera_change")) {
			changing_camera = false;
		}

		if (@event is InputEventMouseMotion && changing_camera) {
			var mouseMotion = (InputEventMouseMotion) @event;
			Rotation = new Vector3(
				Rotation.x - (mouseMotion.Relative.y / 100f),
				Rotation.y - (mouseMotion.Relative.x / 100f),
				Rotation.z
			);
		}
	}
}
