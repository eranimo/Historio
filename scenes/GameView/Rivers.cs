using Godot;
using System;

public class Rivers : Node2D {
	private readonly Color RIVER_COLOR = new Color("#0d64b9");
	private readonly float RIVER_WIDTH = 3.0f;
	
	private GameView gameView;
	private Layout layout;

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		layout = gameView.game.state.GetElement<Layout>();
	}

	public void RenderRivers() {
		Update();
	}

	public override void _Draw() {
		base._Draw();

		foreach (Hex hex in gameView.game.manager.world.Hexes) {
			var tileData = gameView.game.manager.world.GetTileData(hex);

			foreach (var (dir, hasRiver) in tileData.riverSegments) {
				if (hasRiver) {
					var center = layout.HexToPixel(hex);
					var side = hex.Side(dir);
					var p1 = (center + layout.HexCornerOffset(side.LeftCorner) + layout.origin).ToVector();
					var p2 = (center + layout.HexCornerOffset(side.RightCorner) + layout.origin).ToVector();
					DrawLine(p1, p2, RIVER_COLOR, RIVER_WIDTH, false);
				}
			}
		}
	}
}
