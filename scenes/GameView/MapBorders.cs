using Godot;
using System;
using System.Collections.Generic;

/**
Territories are groups of cells 
Areas are subgroups of territories
*/
public partial class MapBorders : Polygon2D {
	private GameMap gameMap;
	private ShaderMaterial shader {
		get { return (this.Material as ShaderMaterial); }
	}
	private int? _selectedTerritory = null;
	private Image hexTerritoryColorImage;
	private Image hexAreaColorImage;
	private ImageTexture hexTerritoryColors;
	private ImageTexture hexAreaColors;

	private Dictionary<RelEcs.Entity, int> activeTerritoryEntities = new Dictionary<RelEcs.Entity, int>();
	private GameView gameView;

	public int? selectedTerritory {
		get { return _selectedTerritory; }
		set {
			_selectedTerritory = value;
			this.shader.SetShaderParameter("selectedTerritory", Variant.CreateFrom((int) value));
		}
	}

	public override void _Ready() {
		gameView = (GameView) GetTree().Root.GetNode("GameView");
		this.Material = ResourceLoader.Load<ShaderMaterial>("res://scenes/GameView/MapBordersShaderMaterial.tres");
	}

	public override void _ExitTree() {
		base._ExitTree();
		this.Material.Dispose();
	}

	private Hex worldSize => gameMap.game.state.GetElement<WorldData>().worldSize;

	public void RenderMap(GameMap gameMap) {
		// this.gameMap = gameMap;
		// var containerSize = gameMap.layout.GridDimensions(worldSize.col, worldSize.row).ToVector();
		// this.Polygon = new Vector2[] {
		// 	new Vector2(0, 0),
		// 	new Vector2(0, containerSize.y),
		// 	new Vector2(containerSize.x, containerSize.y),
		// 	new Vector2(containerSize.x, 0),
		// };
		// this.shader.SetShaderParameter("hexSize", gameMap.layout.size.ToVector());
		// this.shader.SetShaderParameter("gridSize", worldSize.ToVector());
		// this.shader.SetShaderParameter("containerSize", containerSize);

		// this.setupMap();
	}

	public void setupMap() {
		hexTerritoryColorImage = Image.Create(worldSize.col, worldSize.row, false, Image.Format.Rgbaf);
		hexAreaColorImage = Image.Create(worldSize.col, worldSize.row, false, Image.Format.Rgbaf);

		hexTerritoryColors = new ImageTexture();
		hexAreaColors = new ImageTexture();
	}

	public void updateTerritoryMap(List<(Hex hex, RelEcs.Entity territory)> updates) {
		foreach (var (hex, territory) in updates) {
			var settlementData = gameView.game.manager.Get<SettlementData>(territory);
			var settlementOwner = gameView.game.manager.GetTarget<SettlementOwner>(territory);
			var color = gameView.game.manager.Get<CountryData>(settlementOwner).color;
			int id;
			if (activeTerritoryEntities.ContainsKey(territory)) {
				id = activeTerritoryEntities[territory];
			} else {
				id = activeTerritoryEntities.Count + 1;
				activeTerritoryEntities.Add(territory, id);
			}
			var c = new Color(color);
			c.a = (float) id / 10_000;
			hexTerritoryColorImage.SetPixel(hex.col, hex.row, c);
		}

		hexTerritoryColors = ImageTexture.CreateFromImage(hexTerritoryColorImage);
		this.shader.SetShaderParameter("hexTerritoryColor", hexTerritoryColors);
	}

	public void updateAreaMap(List<(Hex hex, RelEcs.Entity territory)> updates) {
		foreach (var (hex, territory) in updates) {
			var settlementData = gameView.game.manager.Get<SettlementData>(territory);
			var settlementOwner = gameView.game.manager.GetTarget<SettlementOwner>(territory);
			var color = gameView.game.manager.Get<CountryData>(settlementOwner).color;
			int id;
			if (activeTerritoryEntities.ContainsKey(territory)) {
				id = activeTerritoryEntities[territory];
			} else {
				id = activeTerritoryEntities.Count + 1;
				activeTerritoryEntities.Add(territory, id);
			}
			var c = Color.FromHSV(0f, 0f, 0f, (float) id / 10_000);
			hexAreaColorImage.SetPixel(hex.col, hex.row, c);
		}

		hexAreaColors = ImageTexture.CreateFromImage(hexAreaColorImage);
		this.shader.SetShaderParameter("hexAreaIDs", hexAreaColors);
	}
}
