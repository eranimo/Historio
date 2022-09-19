using Godot;
using System;

public class Minimap : Control {
	private GameView gameView;
	private MinimapWorld MinimapWorld;
	private MinimapViewport MinimapViewport;
	private const float SCALE = 10f;

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		MinimapWorld = (MinimapWorld) GetNode("VBoxContainer/MinimapViewport/Viewport/MinimapWorld");
		MinimapViewport = (MinimapViewport) GetNode("VBoxContainer/MinimapViewport");
	}
	public void RenderMap(Game game) {
		var layout = game.state.GetElement<Layout>();
		var worldSize = game.state.GetElement<WorldData>().worldSize;
		var mapSize = layout.GridDimensions(worldSize.col, worldSize.row).ToVector();
		MinimapWorld.RenderMap(game);
		MinimapViewport.SetupMap(mapSize, MinimapWorld.RectSize);
		MinimapViewport.UpdateMinimap();

		gameView.pan.Subscribe((Vector2 _offset) => UpdateMinimap());
		gameView.zoom.Subscribe((float _zoom) => UpdateMinimap());
	}

	private void UpdateMinimap() {
		// GD.PrintS("(Minimap) update minimap");
		MinimapWorld.UpdateMinimap();
		MinimapViewport.UpdateMinimap();
	}
}
