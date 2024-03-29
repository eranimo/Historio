using Godot;
using System;

public partial class MinimapWorld : Control  {
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
		MinimapCanvas.Material = ResourceLoader.Load<ShaderMaterial>("res://view/Minimap/MinimapCanvasShader.tres");
		MinimapIndicator = (MinimapIndicator) GetNode("MinimapIndicator");
		gameView = (GameView) GetTree().Root.GetNode("GameView");

		gameView.game.state.AddElement<MinimapWorld>(this);
	}

	public override void _ExitTree() {
		base._ExitTree();
		MinimapCanvas.Material.Dispose();
	}

	public void UpdateMinimap() {
		if (game is null) {
			return;
		}
		var viewportSize = gameView.camera.GetViewportRect().Size;
		SubViewport viewport = gameView.GameController.GameViewport;
		if (viewport is null) {
			return;
		}
		Transform2D cameraTransform = viewport.CanvasTransform;

		var layout = game.state.GetElement<Layout>();
		var worldSize = game.state.GetElement<WorldData>().worldSize;
		var mapSize = layout.GridDimensions(worldSize.col, worldSize.row).ToVector();

		var topLeft = cameraTransform.AffineInverse() * new Vector2(0, 0);
		var topLeftScreen = ((topLeft / mapSize) * Size).Round();
		var bottomRight = cameraTransform.AffineInverse() * viewportSize;
		var bottomRightScreen = ((bottomRight / mapSize) * Size).Round();
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

		shader.SetShaderParameter("gridSize", worldSize.ToVector());
		shader.SetShaderParameter("mapSize", mapSize);
		shader.SetShaderParameter("containerSize", Size);

		hexColorsImage = Image.Create(worldSize.col, worldSize.row, false, Image.Format.Rgbaf);
	}

	public void updateMap() {
		var player = game.manager.state.GetElement<Player>();
		
		if (player.playerCountry is null) {
			foreach (Entity tile in game.manager.world.tiles) {
				var hex = gameView.game.manager.Get<Location>(tile).hex;
				var color = gameView.game.manager.Get<TileData>(tile).GetMinimapColor();
				hexColorsImage.SetPixel(hex.col, hex.row, color);
			}
		} else {
			var playerCountry = game.manager.Get<CountryData>(player.playerCountry);
			foreach (Entity tile in game.manager.world.tiles) {
				var hex = gameView.game.manager.Get<Location>(tile).hex;
				var color = gameView.game.manager.Get<TileData>(tile).GetMinimapColor();
				if (playerCountry.observedHexes.Contains(hex)) {
				} else if (playerCountry.exploredHexes.Contains(hex)) {
					color = color.Darkened(UNOBSERVED_DARKEN_AMOUNT);
				} else {
					color = COLOR_UNEXPLORED;
				}
				hexColorsImage.SetPixel(hex.col, hex.row, color);
			}
		}

		hexColors = ImageTexture.CreateFromImage(hexColorsImage);
		shader.SetShaderParameter("hexColors", hexColors);
	}
}
