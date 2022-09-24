using Godot;
using System;

public class MinimapWorld : Control  {
	private Game game;
	private ColorRect MinimapCanvas;
	private MinimapIndicator MinimapIndicator;
	private GameView gameView;
	private Image hexColorsImage;
	private ImageTexture hexColors;

	private readonly Color COLOR_UNEXPLORED = new Color("#333333");
	private readonly float UNOBSERVED_DARKEN_AMOUNT = 0.3f;

	public override void _Ready() {
		MinimapCanvas = (ColorRect) GetNode("MinimapCanvas");
		MinimapIndicator = (MinimapIndicator) GetNode("MinimapIndicator");
		gameView = (GameView) GetTree().Root.GetNode("GameView");

		gameView.game.state.AddElement<MinimapWorld>(this);
	}

	public void UpdateMinimap() {
		if (game is null) {
			return;
		}
		var viewportSize = gameView.camera.GetViewportRect().Size;
		Viewport viewport = gameView.GameController.GameViewport;
		if (viewport is null) {
			return;
		}
		Transform2D cameraTransform = viewport.CanvasTransform;

		var layout = game.state.GetElement<Layout>();
		var worldSize = game.state.GetElement<WorldData>().worldSize;
		var mapSize = layout.GridDimensions(worldSize.col, worldSize.row).ToVector();

		var topLeft = cameraTransform.AffineInverse() * new Vector2(0, 0);
		var topLeftScreen = ((topLeft / mapSize) * RectSize).Round();
		var bottomRight = cameraTransform.AffineInverse() * viewportSize;
		var bottomRightScreen = ((bottomRight / mapSize) * RectSize).Round();
		var size = bottomRightScreen - topLeftScreen;
		Rect2 indicator = new Rect2(topLeftScreen, size);
		MinimapIndicator.updateIndicator(indicator);
	}

	private ShaderMaterial shader {
		get { return (MinimapCanvas.Material as ShaderMaterial); }
	}

	public void RenderMap(Game game) {
		// GD.PrintS("(MinimapWorld) render map");
		this.game = game;
		UpdateMinimap();

		var layout = game.state.GetElement<Layout>();
		var worldSize = game.state.GetElement<WorldData>().worldSize;
		var mapSize = layout.GridDimensions(worldSize.col, worldSize.row).ToVector();

		shader.SetShaderParam("gridSize", worldSize.ToVector());
		shader.SetShaderParam("mapSize", mapSize);
		shader.SetShaderParam("containerSize", RectSize);

		hexColorsImage = new Image();
		hexColorsImage.Create(worldSize.col, worldSize.row, false, Image.Format.Rgbaf);
		hexColors = new ImageTexture();
	}

	public void updateMap() {
		hexColorsImage.Lock();

		var player = game.manager.state.GetElement<Player>();
		var mapViewState = game.manager.state.GetElement<ViewStateService>();
		var playerViewState = mapViewState.getViewState(player.playerCountry);

		foreach (Entity tile in game.manager.world.tiles) {
			var hex = tile.Get<Location>().hex;
			var color = tile.Get<TileData>().GetMinimapColor();
			var tileViewState = playerViewState.get(tile);
			if (tileViewState == ViewState.Unobserved) {
				color = color.Darkened(UNOBSERVED_DARKEN_AMOUNT);
			} else if (tileViewState == ViewState.Unexplored) {
				color = COLOR_UNEXPLORED;
			}
			hexColorsImage.SetPixel(hex.col, hex.row, color);
		}

		hexColorsImage.Unlock();
		hexColors.CreateFromImage(hexColorsImage);
		shader.SetShaderParam("hexColors", hexColors);
	}
}
