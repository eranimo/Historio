using Godot;
using System;

public class UnitIcon : Node2D {
	private Sprite sprite;

	public override void _Ready() {
		sprite = GetNode<Sprite>("Sprite");
		sprite.Texture = ResourceLoader.Load<Texture>(Unit.unitTypeSpritePath[unitType]);
	}

	private Unit.UnitType unitType;
	private bool selected = false;

	public bool Selected {
		get { return selected; }
		set {
			selected = value;
			if (selected) {
				(sprite.Material as ShaderMaterial).SetShaderParam("width", 2);
			} else {
				(sprite.Material as ShaderMaterial).SetShaderParam("width", 0);
			}
		}
	}

	public Unit.UnitType UnitType {
		get => unitType;
		set {
			unitType = value;
			if (IsInsideTree()) {
				sprite.Texture = ResourceLoader.Load<Texture>(Unit.unitTypeSpritePath[unitType]);
			}
		}
	}
}
