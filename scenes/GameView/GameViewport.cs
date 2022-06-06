using Godot;
using System;

public class GameViewport : ViewportContainer {
	private GameView gameView;

	public override void _Ready() {
		Connect("mouse_entered", this, nameof(onMouseEnter));
		Connect("mouse_exited", this, nameof(onMouseExited));
		Connect("focus_entered", this, nameof(onFocusEntered));
		Connect("focus_exited", this, nameof(onFocusExited));
		gameView = (GameView) GetTree().Root.GetNode("GameView");
	}

	private void onMouseEnter() {
		var focusOwner = GetFocusOwner();
		if (focusOwner == null) {
			GrabFocus();
		} else if (focusOwner == this) {
			gameView.mapInputEnabled.OnNext(true);
		}
	}

	private void onMouseExited() {
		gameView.mapInputEnabled.OnNext(false);
	}

	private void onFocusEntered() {
		gameView.mapInputEnabled.OnNext(true);
	}

	private void onFocusExited() {
		gameView.mapInputEnabled.OnNext(false);
	}
}
