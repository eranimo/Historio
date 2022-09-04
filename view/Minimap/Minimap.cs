using Godot;
using System;

public class Minimap : Control {
	private Game game;
	private ColorRect MinimapCanvas;
	private MinimapIndicator MinimapIndicator;
	private GameView gameView;
	private Image hexColorsImage;
	private ImageTexture hexColors;

	public override void _Ready() {
		MinimapCanvas = (ColorRect) GetNode("MinimapCanvas");
		MinimapIndicator = (MinimapIndicator) GetNode("MinimapIndicator");
		gameView = (GameView) GetTree().Root.GetNode("GameView");

		gameView.pan.Subscribe((Vector2 _offset) => updateIndicator());
		gameView.zoom.Subscribe((float _zoom) => updateIndicator());
	}

	private void updateIndicator() {
		if (game is null) {
			return;
		}
		var viewportSize = gameView.camera.GetViewportRect().Size;
		Viewport viewport = gameView.camera.GetViewport();
		Transform2D cameraTransform = viewport.CanvasTransform;
		var topLeft = cameraTransform.AffineInverse() * new Vector2(0, 0);
		var bottomRight = cameraTransform.AffineInverse() * viewportSize;
		var layout = game.state.GetElement<Layout>();
		var worldSize = game.state.GetElement<WorldData>().worldSize;
		var mapSize = layout.GridDimensions(worldSize.col, worldSize.row).ToVector();
		MinimapIndicator.updateIndicator(new Rect2(
			(topLeft / mapSize) * RectSize,
			((bottomRight - topLeft) / mapSize) * RectSize
		));
		Update();
	}

	private ShaderMaterial shader {
		get { return (MinimapCanvas.Material as ShaderMaterial); }
	}

	public void RenderMap(Game game) {
		GD.PrintS("(Minimap) render map");
		this.game = game;
		var layout = game.state.GetElement<Layout>();
		var worldSize = game.state.GetElement<WorldData>().worldSize;
		var mapSize = layout.GridDimensions(worldSize.col, worldSize.row).ToVector();

		shader.SetShaderParam("gridSize", worldSize.ToVector());
		shader.SetShaderParam("mapSize", mapSize);
		shader.SetShaderParam("containerSize", RectSize);

		hexColorsImage = new Image();
		hexColorsImage.Create(worldSize.col, worldSize.row, false, Image.Format.Rgbaf);
		hexColors = new ImageTexture();

		updateMap();
	}

	public void updateMap() {
		hexColorsImage.Lock();

		foreach (Entity tile in game.manager.world.tiles) {
			var hex = tile.Get<Location>().hex;
			hexColorsImage.SetPixel(hex.col, hex.row, tile.Get<TileData>().GetMinimapColor());
		}

		hexColorsImage.Unlock();
		hexColors.CreateFromImage(hexColorsImage);
		shader.SetShaderParam("hexColors", hexColors);
	}
}
