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
			var center = layout.HexToPixel(hex).ToVector() + (layout.HexSize.ToVector() / 2f) + new Vector2(0, 4f);

			foreach (var (dir, hasRiver) in tileData.streams) {
				if (hasRiver) {
					var c1 = center + layout.HexCornerOffset(dir.CornerLeft()).ToVector();
					var c2 = center + layout.HexCornerOffset(dir.CornerRight()).ToVector();
					DrawLine(c1, c2, RIVER_COLOR, RIVER_WIDTH, false);
				}
			}

			var flowDirPoint = center + layout.HexEdgeMidpoint(tileData.flowDir).ToVector();
			var p2 = center.LinearInterpolate(flowDirPoint, 0.75f);
			DrawLine(center, p2, RIVER_COLOR, RIVER_WIDTH, false);
		}
	}
}
