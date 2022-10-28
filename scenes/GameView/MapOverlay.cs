using Godot;
using System;
using System.Collections.Generic;

public class MapOverlay : Polygon2D {
	private GameView gameView;
	private Image hexColorsImage;
	private ImageTexture hexColors;
	private GameMap gameMap;

	public override void _Ready() {
		Material = ResourceLoader.Load<ShaderMaterial>("res://scenes/GameView/MapOverlayShaderMaterial.tres");
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		gameView.game.state.AddElement(this);
	}

	public override void _ExitTree() {
		base._ExitTree();
		Material.Dispose();
	}

	private ShaderMaterial shader {
		get { return (Material as ShaderMaterial); }
	}

	private Hex worldSize => gameMap.game.state.GetElement<WorldData>().worldSize;

	public void SetupMap(GameMap gameMap) {
		// GD.PrintS("(MapOverlay) render map");
		this.gameMap = gameMap;
		var layout = gameView.game.state.GetElement<Layout>();
		var containerSize = layout.GridDimensions(worldSize.col, worldSize.row).ToVector();

		this.Polygon = new Vector2[] {
			new Vector2(0, 0),
			new Vector2(0, containerSize.y),
			new Vector2(containerSize.x, containerSize.y),
			new Vector2(containerSize.x, 0),
		};

		var mapSize = layout.GridDimensions(worldSize.col, worldSize.row).ToVector();

		shader.SetShaderParam("gridSize", worldSize.ToVector());
		shader.SetShaderParam("mapSize", mapSize);
		shader.SetShaderParam("containerSize", containerSize);

		hexColorsImage = new Image();
		hexColorsImage.Create(worldSize.col, worldSize.row, false, Image.Format.Rgbaf);
		hexColors = new ImageTexture();

		UpdateMap();
	}

	public void UpdateMap() {
		hexColorsImage.Lock();

		var player = gameMap.game.manager.state.GetElement<Player>();
		var NO_COLOR = new Color(0f, 0f, 0f, 0f);

		foreach (Entity tile in gameMap.game.manager.world.tiles) {
			var loc = gameView.game.manager.Get<Location>(tile);
			MapMode mapMode = MapModes.CurrentMapMode.Value;
			if (mapMode.HasColor(loc.hex)) {
				var color = mapMode.GetColor(loc.hex);
				hexColorsImage.SetPixel(loc.hex.col, loc.hex.row, color);
			} else {
				hexColorsImage.SetPixel(loc.hex.col, loc.hex.row, NO_COLOR);
			}
		}

		hexColorsImage.Unlock();
		hexColors.CreateFromImage(hexColorsImage);
		shader.SetShaderParam("hexColors", hexColors);
	}
}
