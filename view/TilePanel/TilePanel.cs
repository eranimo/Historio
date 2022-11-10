using Godot;
using System;

public partial class TilePanel : GamePanelView {
	private Label locationLabel;
	private Label biomeLabel;
	private Label terrainLabel;
	private Label featureLabel;
	private Label heightLabel;
	private Label temperatureLabel;
	private Label rainfallLabel;
	private Label riverFlowLabel;

	public override void _Ready() {
		base._Ready();
		if (!state.HasElement<TilePanel>()) {
			state.AddElement(this);
		} else {
			state.ReplaceElement(this);
		}

		locationLabel = (Label) GetNode("%Location");
		biomeLabel = (Label) GetNode("%Biome");
		terrainLabel = (Label) GetNode("%Terrain");
		featureLabel = (Label) GetNode("%Feature");

		heightLabel = (Label) GetNode("%Height");
		temperatureLabel = (Label) GetNode("%Temperature");
		rainfallLabel = (Label) GetNode("%Rainfall");
		riverFlowLabel = (Label) GetNode("%RiverFlow");

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

		heightLabel.Text = tileData.height.ToString();
		temperatureLabel.Text = tileData.temperature.ToString();
		rainfallLabel.Text = tileData.rainfall.ToString();
		riverFlowLabel.Text = $"{tileData.riverFlow} ({tileData.flowDir.ShortName()})";
	}

	public override void ResetView(Entity entity) {
		gameView.GameController.GameMap.SetSelectedTile(null);
	}
}
