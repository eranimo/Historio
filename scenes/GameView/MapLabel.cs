using Godot;
using System;

public partial class MapLabel : Label {
	private GameView gameView;
	private MapLabelType labelType;

	public MapLabelType LabelType {
		get => labelType;
		set {
			if (value == MapLabelType.Territory) {
				this.AddThemeFontOverride("font", ResourceLoader.Load<Font>("res://assets/fonts/TerritoryLabelFont.tres"));
			} else if (value == MapLabelType.Region) {
				this.AddThemeFontOverride("font", ResourceLoader.Load<Font>("res://assets/fonts/RegionLabelFont.tres"));
			}
			labelType = value;
		}
	}

	public enum MapLabelType {
		Territory,
		Region,
	}

	public override void _Ready() {
		HorizontalAlignment = HorizontalAlignment.Center;
		VerticalAlignment = VerticalAlignment.Center;
		PivotOffset = Size / 2;
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		resize();
		gameView.OnZoom.Subscribe((float zoom) => resize());
	}

	public void SetPosition(Vector2 position) {
		base.SetPosition(position - Size / 2);
		// base.SetPosition(position);
	}

	private void resize() {
		Scale = new Vector2(gameView.zoom.Value, gameView.zoom.Value);
	}
}
