using Godot;
using System;

public class Camera : Camera2D {
	private bool isMousePanning = false;
	const float ZOOM_SPEED = 0.25f;
	const float MIN_ZOOM = 0.25f;
	const float MAX_ZOOM = 600;

	private bool isKeyboardPanning = false;
	private const float BASE_MOVE_AMOUNT = 10.0f;
	private Vector2 moveDirection = new Vector2();
	private GameView gameView;

	public override void _Ready() {
		base._Ready();
		gameView = (GameView) GetTree().Root.GetNode("GameView");
	}

	public override void _Input(InputEvent @event) {
		base._Input(@event);

		if (!gameView.mapInputEnabled.Value) {
			return;
		}

		if (@event.IsActionPressed("view_zoom_in")) {
			zoomCamera(-ZOOM_SPEED, ((InputEventMouseButton) @event).Position);
		} else if (@event.IsActionReleased("view_zoom_out")) {
			zoomCamera(ZOOM_SPEED, ((InputEventMouseButton) @event).Position);
		}

		if (@event.IsActionPressed("view_pan_mouse")) {
			isMousePanning = true;
		} else if (@event.IsActionReleased("view_pan_mouse")) {
			isMousePanning = false;
		}

		if (@event is InputEventMouseMotion && isMousePanning) {
			var motionEvent = (InputEventMouseMotion) @event;
			Offset -= motionEvent.Relative * Zoom;
		}
	}

	public override void _PhysicsProcess(float delta) {
		if (!gameView.mapInputEnabled.Value) {
			return;
		}
		moveDirection.x = 0;
		moveDirection.y = 0;
		var moveAmount = BASE_MOVE_AMOUNT;
		if (Input.IsKeyPressed((int) Godot.KeyList.Shift)) {
			moveAmount *= 2;
		}
		if (Input.IsActionPressed("view_pan_up")) {
			moveDirection.y -= moveAmount;
		}
		if (Input.IsActionPressed("view_pan_down")) {
			moveDirection.y += moveAmount;
		}
		if (Input.IsActionPressed("view_pan_left")) {
			moveDirection.x -= moveAmount;
		}
		if (Input.IsActionPressed("view_pan_right")) {
			moveDirection.x += moveAmount;
		}
		Offset += moveDirection;
	}

	private void zoomCamera(float zoomFactor, Vector2 mousePosition) {
		var viewportSize = GetViewport().Size;
		var prevZoom = Zoom;
		Zoom += Zoom * zoomFactor;
		Zoom = new Vector2(
			Math.Min(MAX_ZOOM, Math.Max(MIN_ZOOM, Zoom.x)),
			Math.Min(MAX_ZOOM, Math.Max(MIN_ZOOM, Zoom.y))
		);
		gameView.zoom.OnNext(Zoom.x);
		Offset += ((viewportSize * 0.5f) - mousePosition) * (Zoom - prevZoom);
		Offset = new Vector2(
			(float) Math.Round(Offset.x),
			(float) Math.Round(Offset.y)
		);
	}
}
