using System;
using Godot;
using MessagePack;

public abstract class SerializedNode<T> {
	public abstract void Serialize(T node);
	public abstract T Deserialize();
}

public partial class UnitIcon : Node2D {
	private GameView gameView;
	private TextureRect sprite;

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		sprite = GetNode<TextureRect>("Sprite2D");
		sprite.Texture = ResourceLoader.Load<Texture2D>(unitType.spritePath);

		sprite.Connect("mouse_entered",new Callable(this,nameof(onMouseEntered)));
		sprite.Connect("mouse_exited",new Callable(this,nameof(onMouseExited)));
	}

	private void onMouseEntered() {
		hovered = true;
	}

	private void onMouseExited() {
		hovered = false;
	}

	public RelEcs.Entity entity;

	private UnitType unitType;
	private bool selected = false;
	private bool hovered;

	public bool Selected {
		get { return selected; }
		set {
			selected = value;
			if (selected) {
				sprite.Material = ResourceLoader.Load<ShaderMaterial>("res://view/UnitIcon/OutlineShaderMaterial.tres");
			} else {
				sprite.Material = ResourceLoader.Load<ShaderMaterial>("res://view/UnitIcon/NoOutlineShaderMaterial.tres");
			}
		}
	}

	public UnitType UnitType {
		get => unitType;
		set {
			unitType = value;
			if (IsInsideTree()) {
				sprite.Texture = ResourceLoader.Load<Texture2D>(unitType.spritePath);
			}
		}
	}

	public override void _Input(InputEvent @event) {
		base._Input(@event);

		if (hovered && @event is InputEventMouseButton) {
			var mouseEventButton = (InputEventMouseButton) @event;
			if (mouseEventButton.IsPressed() && mouseEventButton.ButtonIndex == MouseButton.Left) {
				gameView.GameController.GamePanel.PanelSet(new GamePanelState {
					type = GamePanelType.Unit,
					entity = entity
				});
				GetViewport().SetInputAsHandled();
			}
		}
	}
}
