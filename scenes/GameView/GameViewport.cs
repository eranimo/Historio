using Godot;
using System;

public partial class GameViewport : SubViewportContainer {
	private GameView gameView;
	private GameCamera gameCamera;
	const float ZOOM_SPEED = 0.25f;
	private bool isMousePanning = false;
	private bool isKeyboardPanning = false;
	private const float BASE_MOVE_AMOUNT = 10.0f;
	private Vector2 moveDirection = new Vector2();

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		gameCamera = (GameCamera) GetNode("SubViewport/GameMap/GameCamera");
		gameView.zoom.OnNext(gameCamera.Zoom.x);
		gameView.pan.OnNext(gameCamera.Position);
	}

	public override void _GuiInput(InputEvent @event) {
		base._GuiInput(@event);

		if (@event.IsActionPressed("view_zoom_in")) {
			gameCamera.ZoomCamera(-ZOOM_SPEED, ((InputEventMouseButton) @event).Position);
			AcceptEvent();
		} else if (@event.IsActionReleased("view_zoom_out")) {
			gameCamera.ZoomCamera(ZOOM_SPEED, ((InputEventMouseButton) @event).Position);
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
			gameCamera.Position -= motionEvent.Relative * gameCamera.Zoom;
			gameView.pan.OnNext(gameCamera.Position);
			AcceptEvent();
		}

		if (@event is InputEventMouseButton) {
			var mouseEventButton = (InputEventMouseButton) @event;
			if (mouseEventButton.IsPressed() && mouseEventButton.ButtonIndex != MouseButton.MaskMiddle) {
				var coord = getCoordAtCursor();
				if (mouseEventButton.ButtonIndex == MouseButton.MaskLeft) {
					gameView.GameController.gameMapInputSubject.OnNext(new GameMapInput {
						hex = coord,
						type = GameMapInputType.LeftClick,
						isShiftModifier = mouseEventButton.ShiftPressed
					});
					AcceptEvent();
				} else if (mouseEventButton.ButtonIndex == MouseButton.MaskRight) {
					gameView.GameController.gameMapInputSubject.OnNext(new GameMapInput {
						hex = coord,
						type = GameMapInputType.RightClick,
						isShiftModifier = mouseEventButton.ShiftPressed
					});
					AcceptEvent();
				}
			}
		}
	}

	public override void _Input(InputEvent @event) {
		if (@event is InputEventMouseMotion) {
			var coord = getCoordAtCursor();
			if (gameView.game.manager.world.IsValidTile(coord)) {
				gameView.GameController.gameMapInputSubject.OnNext(new GameMapInput {
					hex = coord,
					type = GameMapInputType.Hovered,
					isShiftModifier = false
				});
			}
		}
	}

	private Hex getCoordAtCursor() {
		var cursorPos = gameView.GameController.GameMap.GetLocalMousePosition();
		return gameView.game.state.GetElement<Layout>().PixelToHex(new Point(cursorPos.x, cursorPos.y));
	}

	public void _PhysicsProcess(float delta) {
		if (gameView.isConsoleToggled) {
			return;
		}
		moveDirection.x = 0;
		moveDirection.y = 0;
		var moveAmount = BASE_MOVE_AMOUNT;
		if (Input.IsKeyPressed(Key.Shift)) {
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
		gameCamera.Position += moveDirection;
		if (moveDirection.x > 0 && moveDirection.y > 0) {
			gameView.pan.OnNext(gameCamera.Position);
		}
	}
}
