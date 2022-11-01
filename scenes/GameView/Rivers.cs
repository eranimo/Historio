using Godot;
using System;

public class Rivers : Node2D {
	private readonly Color RIVER_COLOR = new Color("#0d64b9");
	private readonly float RIVER_WIDTH = 3.0f;
	
	private GameView gameView;
	private Layout layout;
	private bool shouldRender = false;

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		layout = gameView.game.state.GetElement<Layout>();
	}

	public void RenderRivers() {
		shouldRender = true;
		Update();
	}

	public override void _Draw() {
		base._Draw();

		if (!shouldRender) {
			return;
		}

		foreach (Hex hex in gameView.game.manager.world.Hexes) {
			var tileData = gameView.game.manager.world.GetTileData(hex);

			// foreach (var (dir, hasRiver) in tileData.riverSegments) {
			// 	if (hasRiver) {
			// 		var center = layout.HexToPixel(hex);
			// 		var side = hex.Side(dir);
			// 		var p1 = (center + layout.HexCornerOffset(side.LeftCorner) + layout.origin).ToVector();
			// 		var p2 = (center + layout.HexCornerOffset(side.RightCorner) + layout.origin).ToVector();
			// 		DrawLine(p1, p2, RIVER_COLOR, RIVER_WIDTH, false);
			// 	}
			// }

			var p1 = layout.HexToPixel(hex).ToVector() + (layout.HexSize.ToVector() / 2f) + new Vector2(0, 4f);
			// DrawRect(new Rect2(p1 - new Vector2(1, 1), new Vector2(2, 2)), RIVER_COLOR);
			var flowDirPoint = p1 + layout.HexEdgeMidpoint(tileData.flowDir).ToVector();
			var p2 = p1.LinearInterpolate(flowDirPoint, 0.75f);
			DrawLine(p1, p2, RIVER_COLOR, RIVER_WIDTH, false);
		}
	}
}
