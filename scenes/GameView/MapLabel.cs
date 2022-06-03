using Godot;
using System;

public class MapLabel : Label {
	private GameView gameView;
	private MapLabelType labelType;

	public MapLabelType LabelType {
		get => labelType;
		set {
			if (value == MapLabelType.Territory) {
				this.AddFontOverride("font", ResourceLoader.Load<Font>("res://assets/fonts/TerritoryLabelFont.tres"));
			} else if (value == MapLabelType.Region) {
				this.AddFontOverride("font", ResourceLoader.Load<Font>("res://assets/fonts/RegionLabelFont.tres"));
			}
			labelType = value;
		}
	}

	public enum MapLabelType {
		Territory,
		Region,
	}

	public override void _Ready() {
		Align = Label.AlignEnum.Center;
		Valign = Label.VAlign.Center;
		RectPivotOffset = RectSize / 2;
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		resize();
		gameView.OnZoom.Subscribe((float zoom) => resize());
	}

	public void SetPosition(Vector2 position) {
		base.SetPosition(position - RectSize / 2);
		// base.SetPosition(position);
	}

	private void resize() {
		RectScale = new Vector2(gameView.zoom.Value, gameView.zoom.Value);
	}
}
