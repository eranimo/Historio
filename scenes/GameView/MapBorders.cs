using Godot;
using System;

/**
Territories are groups of cells 
Areas are subgroups of territories
*/
public class MapBorders : Polygon2D {
	private GameMap gameMap;
	private ShaderMaterial shader {
		get { return (this.Material as ShaderMaterial); }
	}
	private int? _selectedTerritory = null;
	private Image hexTerritoryColorImage;
	private ImageTexture hexTerritoryColors;
	private ImageTexture hexAreaColors;
	private Image hexAreaColorImage;

	public int? selectedTerritory {
		get { return _selectedTerritory; }
		set {
			_selectedTerritory = value;
			this.shader.SetShaderParam("selectedTerritory", value);
		}
	}

	public override void _Ready() {}

	public void RenderMap(GameMap gameMap) {
		this.gameMap = gameMap;
		var worldSize = gameMap.game.manager.state.worldSize;
		var containerSize = gameMap.layout.GridDimensions(worldSize.col, worldSize.row).ToVector();
		this.Polygon = new Vector2[] {
			new Vector2(0, 0),
			new Vector2(0, containerSize.y),
			new Vector2(containerSize.x, containerSize.y),
			new Vector2(containerSize.x, 0),
		};
		this.shader.SetShaderParam("hexSize", gameMap.layout.size.ToVector());
		this.shader.SetShaderParam("gridSize", worldSize.ToVector());
		this.shader.SetShaderParam("containerSize", containerSize);

		this.setupMap();

		this.updateTerritoryMap();
		this.updateAreaMap();
	}

	public void setupMap() {
		var worldSize = gameMap.game.manager.state.worldSize;
		hexTerritoryColorImage = new Image();
		hexTerritoryColorImage.Create(worldSize.col, worldSize.row, false, Image.Format.Rgbaf);
		hexAreaColorImage = new Image();
		hexAreaColorImage.Create(worldSize.col, worldSize.row, false, Image.Format.Rgbaf);

		hexTerritoryColors = new ImageTexture();
		hexAreaColors = new ImageTexture();
	}

	private void updateTerritoryMap() {
		hexTerritoryColorImage.Lock();

		var t1 = Color.FromHsv(0.1f, 0.9f, 0.8f, 1f / 10_000);
		var t2 = Color.FromHsv(0.2f, 0.9f, 0.8f, 2f / 10_000);
		hexTerritoryColorImage.SetPixel(1, 1, t1);
		hexTerritoryColorImage.SetPixel(1, 2, t1);
		hexTerritoryColorImage.SetPixel(2, 3, t2);
		hexTerritoryColorImage.SetPixel(3, 3, t2);
		hexTerritoryColorImage.SetPixel(3, 4, t2);

		hexTerritoryColorImage.Unlock();
		
		hexTerritoryColors.CreateFromImage(hexTerritoryColorImage);
		this.shader.SetShaderParam("hexTerritoryColor", hexTerritoryColors);
	}

	private void updateAreaMap() {
		hexAreaColorImage.Lock();

		var a1 = Color.FromHsv(0f, 0f, 0f, 1f / 10_000);
		var a2 = Color.FromHsv(0f, 0f, 0f, 2f / 10_000);
		var a3 = Color.FromHsv(0f, 0f, 0f, 3f / 10_000);
		hexAreaColorImage.SetPixel(1, 1, a1);
		hexAreaColorImage.SetPixel(1, 2, a1);
		hexAreaColorImage.SetPixel(2, 3, a2);
		hexAreaColorImage.SetPixel(3, 3, a2);
		hexAreaColorImage.SetPixel(3, 4, a3);

		hexAreaColorImage.Unlock();
		
		hexAreaColors.CreateFromImage(hexAreaColorImage);
		this.shader.SetShaderParam("hexAreaIDs", hexAreaColors);
	}
}
