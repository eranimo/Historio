using Godot;
using System;
using System.Collections.Generic;

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
	private Image hexAreaColorImage;
	private ImageTexture hexTerritoryColors;
	private ImageTexture hexAreaColors;

	private Dictionary<RelEcs.Entity, int> activeTerritoryEntities = new Dictionary<RelEcs.Entity, int>();

	public int? selectedTerritory {
		get { return _selectedTerritory; }
		set {
			_selectedTerritory = value;
			this.shader.SetShaderParam("selectedTerritory", value);
		}
	}

	public override void _Ready() {}

	private Hex worldSize => gameMap.game.state.GetElement<WorldData>().worldSize;

	public void RenderMap(GameMap gameMap) {
		this.gameMap = gameMap;
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
	}

	public void setupMap() {
		hexTerritoryColorImage = new Image();
		hexTerritoryColorImage.Create(worldSize.col, worldSize.row, false, Image.Format.Rgbaf);
		hexAreaColorImage = new Image();
		hexAreaColorImage.Create(worldSize.col, worldSize.row, false, Image.Format.Rgbaf);

		hexTerritoryColors = new ImageTexture();
		hexAreaColors = new ImageTexture();
	}

	public void updateTerritoryMap(List<(Hex hex, RelEcs.Entity territory)> updates) {
		hexTerritoryColorImage.Lock();
		
		foreach (var (hex, territory) in updates) {
			var color = territory.Get<SettlementData>().ownerPolity.Get<PolityData>().color;
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

		hexTerritoryColorImage.Unlock();
		
		hexTerritoryColors.CreateFromImage(hexTerritoryColorImage);
		this.shader.SetShaderParam("hexTerritoryColor", hexTerritoryColors);
	}

	public void updateAreaMap(List<(Hex hex, RelEcs.Entity territory)> updates) {
		hexAreaColorImage.Lock();

		foreach (var (hex, territory) in updates) {
			var color = territory.Get<SettlementData>().ownerPolity.Get<PolityData>().color;
			int id;
			if (activeTerritoryEntities.ContainsKey(territory)) {
				id = activeTerritoryEntities[territory];
			} else {
				id = activeTerritoryEntities.Count + 1;
				activeTerritoryEntities.Add(territory, id);
			}
			var c = Color.FromHsv(0f, 0f, 0f, (float) id / 10_000);
			hexAreaColorImage.SetPixel(hex.col, hex.row, c);
		}

		hexAreaColorImage.Unlock();
		
		hexAreaColors.CreateFromImage(hexAreaColorImage);
		this.shader.SetShaderParam("hexAreaIDs", hexAreaColors);
	}
}
