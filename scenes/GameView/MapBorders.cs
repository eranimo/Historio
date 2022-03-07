using Godot;
using System;

public class MapBorders : Polygon2D {
	public override void _Ready(){
		
	}

	private ShaderMaterial shader {
		get { return (this.Material as ShaderMaterial); }
	}

	public void DrawBorders(GameMap gameMap, GameWorld world) {
		var containerSize = gameMap.layout.GridDimensions(world.worldSize.col, world.worldSize.row).ToVector();
		this.Polygon = new Vector2[] {
			new Vector2(0, 0),
			new Vector2(0, containerSize.y),
			new Vector2(containerSize.x, containerSize.y),
			new Vector2(containerSize.x, 0),
		};
		this.shader.SetShaderParam("hexSize", gameMap.layout.size.ToVector());
		this.shader.SetShaderParam("gridSize", world.worldSize.ToVector());
		this.shader.SetShaderParam("containerSize", containerSize);

		this.UpdateTerritoryMap(world);
	}

	private void UpdateTerritoryMap(GameWorld world) {
		Image hexTerritoryColorImage = new Image();
		hexTerritoryColorImage.Create(world.worldSize.col, world.worldSize.row, false, Image.Format.Rgbaf);

		hexTerritoryColorImage.Lock();

		var c1 = Color.FromHsv(0.1f, 0.9f, 0.8f, 1f / 10_000);
		var c2 = Color.FromHsv(0.2f, 0.9f, 0.8f, 2f / 10_000);
		hexTerritoryColorImage.SetPixel(1, 1, c1);
		hexTerritoryColorImage.SetPixel(1, 2, c1);
		hexTerritoryColorImage.SetPixel(2, 3, c2);
		hexTerritoryColorImage.SetPixel(3, 3, c2);
		hexTerritoryColorImage.SetPixel(3, 4, c2);

		hexTerritoryColorImage.Unlock();
		ImageTexture hexTerritoryColors = new ImageTexture();
		hexTerritoryColors.CreateFromImage(hexTerritoryColorImage);
		this.shader.SetShaderParam("hexTerritoryColor", hexTerritoryColors);
	}
}
