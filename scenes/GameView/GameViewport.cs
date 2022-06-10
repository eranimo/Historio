using Godot;
using System;

public class GameViewport : ViewportContainer {
	private GameView gameView;
	private bool hovered;

	public override void _Ready() {
		Connect("mouse_entered", this, nameof(onMouseEnter));
		Connect("mouse_exited", this, nameof(onMouseExited));
		Connect("focus_entered", this, nameof(onFocusEntered));
		Connect("focus_exited", this, nameof(onFocusExited));
		gameView = (GameView) GetTree().Root.GetNode("GameView");
	}

	private void onMouseEnter() {
		hovered = true;
		grabFocusAndEnable();
	}

	private void grabFocusAndEnable() {
		var focusOwner = GetFocusOwner();
		if (focusOwner == null) {
			GrabFocus();
		} else if (focusOwner == this) {
			gameView.mapInputEnabled.OnNext(true);
		}
	}

	private void onMouseExited() {
		hovered = false;
		gameView.mapInputEnabled.OnNext(false);
	}

	private void onFocusEntered() {
		gameView.mapInputEnabled.OnNext(true);
	}

	private void onFocusExited() {
		gameView.mapInputEnabled.OnNext(false);
	}

	public override void _Input(InputEvent @event) {
		base._Input(@event);

		if (hovered && @event is InputEventMouseButton) {
			var mouseEventButton = (InputEventMouseButton) @event;
			if (mouseEventButton.IsPressed()) {
				GrabFocus();
			}
		}
	}
}
