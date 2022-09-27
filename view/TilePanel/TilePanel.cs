using Godot;
using System;

public class TilePanel : GamePanelView {
	private Label locationLabel;
	private Label biomeLabel;
	private Label terrainLabel;
	private Label featureLabel;

	public override void _Ready() {
		base._Ready();
		state.AddElement(this);

		locationLabel = (Label) GetNode("TabContainer/Details/Container/Values/Location");
		biomeLabel = (Label) GetNode("TabContainer/Details/Container/Values/Biome");
		terrainLabel = (Label) GetNode("TabContainer/Details/Container/Values/Terrain");
		featureLabel = (Label) GetNode("TabContainer/Details/Container/Values/Feature");

		UpdateView(gamePanel.CurrentPanel.Value.entity);
	}

	public override void UpdateView(Entity tile) {
		var location = gameView.game.manager.Get<Location>(tile);
		var tileData = gameView.game.manager.Get<TileData>(tile);
		gameView.GameController.GameMap.SetSelectedTile(tile);
		gamePanel.SetTitle($"Tile ({location.hex.col}, {location.hex.row})");

		locationLabel.Text = $"({location.hex.col}, {location.hex.row})";
		biomeLabel.Text = tileData.biome.ToString();
		terrainLabel.Text = tileData.terrain.ToString();
		featureLabel.Text = tileData.feature.ToString();
	}

	public override void ResetView(Entity entity) {
		gameView.GameController.GameMap.SetSelectedTile(null);
	}
}
