using Godot;
using System;

public partial class SettlementLabel : PanelContainer {
	public ColorRect flag;
	public Label label;
	private GameView gameView;

	public override void _Ready() {
		flag = GetNode<ColorRect>("HBoxContainer/Control/Control/ColorRect");
		flag.Color = _color;
		label = GetNode<Label>("HBoxContainer/MarginContainer/Label");
		label.Text = _text;

		gameView = (GameView) GetTree().Root.GetNode("GameView");
		resize();
		gameView.OnZoom.Subscribe((float zoom) => resize());

		Connect("mouse_entered",new Callable(this,nameof(onMouseEntered)));
		Connect("mouse_exited",new Callable(this,nameof(onMouseExited)));
	}

	private bool _hovered;
	private bool hovered {
		get { return _hovered; }
		set {
			_hovered = value;
			StyleBoxFlat style;
			if (_hovered) {
				style = ResourceLoader.Load<StyleBoxFlat>("res://assets/styles/UnitLabelHovered.tres");
			} else {
				style = ResourceLoader.Load<StyleBoxFlat>("res://assets/styles/UnitLabelNormal.tres");
			}
			AddThemeStyleboxOverride("panel", style);
		}
	}

	private void onMouseEntered() {
		hovered = true;
	}

	private void onMouseExited() {
		hovered = false;
	}

	public Vector2 _position;
	public Vector2 position {
		get { return _position; }
		set {
			_position = value;
			centerLabel();
		}
	}
	public void centerLabel() {
		SetPosition(position - Size / 2 * gameView.zoom.Value);
	}

	private void resize() {
		Scale = new Vector2(gameView.zoom.Value, gameView.zoom.Value);
		centerLabel();
	}

	public string _text;
	public string text {
		get { return _text; }
		set {
			_text = value;
			if (IsInsideTree()) {
				label.Text = value;
				resize();
			}
		}
	}

	public Color _color;
	public Color color {
		get { return _color; }
		set {
			_color = value;
			if (IsInsideTree()) {
				flag.Color = value;
			}
		}
	}

	public override void _Input(InputEvent @event) {		
		// if event is InputEventMouseButton and event.pressed and event.button_index == BUTTON_RIGHT:
		if (hovered && @event is InputEventMouseButton) {
			var mouseEventButton = (InputEventMouseButton) @event;
			if (mouseEventButton.IsPressed() && mouseEventButton.ButtonIndex == MouseButton.MaskLeft) {
				GD.PrintS("Clicked on settlement label");
				GetViewport().SetInputAsHandled();
			}
		}
	}
}
