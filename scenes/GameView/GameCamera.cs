using Godot;
using System;

public partial class GameCamera : Camera2D {
	const float MIN_ZOOM = 0.25f;
	const float START_ZOOM = 0.5f;
	const float MAX_ZOOM = 600;
	private GameView gameView;
	private bool positionChanged = false;

	public override void _Ready() {
		base._Ready();
		Zoom = new Vector2(START_ZOOM, START_ZOOM);
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		gameView.camera = this;
	}

	public void SetCameraCenter(Vector2 pos) {
		Position = pos;
		positionChanged = true;
	}

	public void ZoomCamera(float zoomFactor, Vector2 mousePosition) {
		var viewportSize = GetViewport().GetTexture().GetSize();
		var prevZoom = Zoom;
		Zoom += Zoom * zoomFactor;
		Zoom = new Vector2(
			Math.Min(MAX_ZOOM, Math.Max(MIN_ZOOM, Zoom.x)),
			Math.Min(MAX_ZOOM, Math.Max(MIN_ZOOM, Zoom.y))
		);
		gameView.zoom.OnNext(Zoom.x);
		Position += ((viewportSize * 0.5f) - mousePosition) * (Zoom - prevZoom);
		Position = new Vector2(
			(float) Math.Round(Position.x),
			(float) Math.Round(Position.y)
		);
		positionChanged = true;
	}

	public override void _Process(double delta) {
		base._Process(delta);

		if (positionChanged) {
			gameView.pan.OnNext(Position);
			positionChanged = false;
		}
	}
}
