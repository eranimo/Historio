using Godot;
using System;

public partial class Rivers : Node2D {
	private readonly Color STREAM_COLOR = new Color("#0d64b9");
	private readonly Color ARROW_COLOR = new Color("#043b70");
	
	private GameView gameView;
	private Layout layout;
	private bool shouldRender = false;

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		layout = gameView.game.state.GetElement<Layout>();
	}

	public void RenderRivers() {
		shouldRender = true;
		QueueRedraw();
	}

	public override void _Draw() {
		base._Draw();

		if (!shouldRender) {
			return;
		}

		foreach (Hex hex in gameView.game.manager.world.Hexes) {
			var tileData = gameView.game.manager.world.GetTileData(hex);
			var center = layout.HexToPixel(hex).ToVector() + (layout.HexSize.ToVector() / 2f) + new Vector2(0, 4f);

			foreach (var (dir, flow) in tileData.streamFlow) {
				var c1 = center + layout.HexCornerOffset(dir.CornerLeft()).ToVector();
				var c2 = center + layout.HexCornerOffset(dir.CornerRight()).ToVector();
				var color = new Color(0, 0, Math.Clamp(flow / 250f, 0, 1));
				DrawLine(c1, c2, color, 6f, false);
			}

			// foreach (var (dir, hasRiver) in tileData.streams) {
			// 	if (hasRiver) {
			// 		var c1 = center + layout.HexCornerOffset(dir.CornerLeft()).ToVector();
			// 		var c2 = center + layout.HexCornerOffset(dir.CornerRight()).ToVector();
			// 		DrawLine(c1, c2, STREAM_COLOR, 3f, false);
			// 	}
			// }

			var flowDirPoint = center + layout.HexEdgeMidpoint(tileData.flowDir).ToVector();
			var p2 = center.Lerp(flowDirPoint, 0.75f);
			DrawLine(center, p2, ARROW_COLOR, 2f, false);
		}
	}
}
