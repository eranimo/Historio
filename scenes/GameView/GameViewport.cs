using Godot;
using System;

public class GameViewport : ViewportContainer {
	private GameView gameView;
	private GameCamera gameCamera;
	const float ZOOM_SPEED = 0.25f;
	private bool isMousePanning = false;
	private bool isKeyboardPanning = false;
	private const float BASE_MOVE_AMOUNT = 10.0f;
	private Vector2 moveDirection = new Vector2();

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		gameCamera = (GameCamera) GetNode("Viewport/GameMap/GameCamera");
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
			if (mouseEventButton.IsPressed() && mouseEventButton.ButtonIndex != (int) ButtonList.MaskMiddle) {
				var coord = getCoordAtCursor();
				if (mouseEventButton.ButtonIndex == (int) ButtonList.MaskLeft) {
					gameView.GameController.gameMapInputSubject.OnNext(new GameMapInput {
						hex = coord,
						type = GameMapInputType.LeftClick,
						isShiftModifier = mouseEventButton.Shift
					});
					AcceptEvent();
				} else if (mouseEventButton.ButtonIndex == (int) ButtonList.MaskRight) {
					gameView.GameController.gameMapInputSubject.OnNext(new GameMapInput {
						hex = coord,
						type = GameMapInputType.RightClick,
						isShiftModifier = mouseEventButton.Shift
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

	public override void _PhysicsProcess(float delta) {
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
		gameCamera.Position += moveDirection;
		if (moveDirection.x > 0 && moveDirection.y > 0) {
			gameView.pan.OnNext(gameCamera.Position);
		}
	}
}
