using Godot;
using System;

public class MinimapViewport : ViewportContainer {
	private bool isMousePanning = false;

	private bool isKeyboardPanning = false;
	private const float BASE_MOVE_AMOUNT = 10.0f;
	private GameView gameView;
	private MinimapCamera minimapCamera;
	private Vector2 containerSize;
	private Vector2 mapSize;
	private bool isViewMoving = false;
	const float ZOOM_SPEED = 0.10f;
	private bool canUpdateCamera = true;

	public bool CameraEnabled { get; set; } = false;

	public override void _Ready() {
		base._Ready();
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		minimapCamera = (MinimapCamera) GetNode("Viewport/MinimapCamera");
	}

	public override void _GuiInput(InputEvent @event) {
		base._Input(@event);

		if (@event.IsActionPressed("view_select")) {
			isViewMoving = true;
			AcceptEvent();
		} else if (@event.IsActionReleased("view_select")) {
			isViewMoving = false;
			AcceptEvent();
		}

		if (@event.IsActionPressed("view_zoom_in")) {
			zoomCamera(-ZOOM_SPEED, ((InputEventMouseButton) @event).Position);
			AcceptEvent();
		} else if (@event.IsActionReleased("view_zoom_out")) {
			zoomCamera(ZOOM_SPEED, ((InputEventMouseButton) @event).Position);
			AcceptEvent();
		}

		if (@event.IsActionPressed("view_pan_mouse")) {
			isMousePanning = true;
			AcceptEvent();
		} else if (@event.IsActionReleased("view_pan_mouse")) {
			isMousePanning = false;
			AcceptEvent();
		}

		if (@event is InputEventMouseMotion && isMousePanning) {
			var motionEvent = (InputEventMouseMotion) @event;
			minimapCamera.Position -= motionEvent.Relative * minimapCamera.Zoom;
			AcceptEvent();
		}

		if ((@event is InputEventMouseMotion || @event is InputEventMouseButton) && isViewMoving) {
			var pos = (minimapCamera.GetGlobalMousePosition() / containerSize) * mapSize;
			GD.PrintS("(MinimapViewport) move to", pos);
			canUpdateCamera = false;
			gameView.camera.SetCameraCenter(pos);
			canUpdateCamera = true;
			AcceptEvent();
		}

	}

	public void UpdateMinimap() {
		if (canUpdateCamera) {
			// GD.PrintS("(MinimapViewport) update minimap viewport");
			minimapCamera.Position = (gameView.camera.Position / mapSize) * containerSize;
			minimapCamera.Zoom = gameView.camera.Zoom;
		}
	}

	public void SetupMap(Vector2 mapSize, Vector2 containerSize) {
		this.containerSize = containerSize;
		this.mapSize = mapSize;
	}

	private void zoomCamera(float zoomFactor, Vector2 position) {
		minimapCamera.setZoom(zoomFactor, position);
	}
}
