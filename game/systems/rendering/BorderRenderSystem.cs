using RelEcs;
using System.Linq;
using System.Collections.Generic;
using Godot;

public class BorderRenderSystem : ISystem {
	private List<(Hex, Entity)> hexUpdates;
	private HashSet<Entity> settlementUpdates;
	private HashSet<Entity> countryUpdates;
	private PackedScene settlementLabelScene;

	public RelEcs.World World { get; set; }

	public BorderRenderSystem() {
		hexUpdates = new List<(Hex, RelEcs.Entity)>();
		settlementUpdates = new HashSet<Entity>();
		countryUpdates = new HashSet<Entity>();

		settlementLabelScene = ResourceLoader.Load<PackedScene>("res://scenes/GameView/SettlementLabel.tscn");
	}

	public void Run() {
		var gameMap = this.GetElement<GameMap>();
		var world = this.GetElement<WorldService>();

		hexUpdates.Clear();
		settlementUpdates.Clear();
		countryUpdates.Clear();
		
		foreach (var e in this.Receive<SettlementBorderUpdated>()) {
			var location = this.GetComponent<Location>(e.countryTile);
			var territoryData = this.GetComponent<SettlementData>(e.settlement);
			var settlementOwner = this.GetTarget<SettlementOwner>(e.settlement);
			hexUpdates.Add((location.hex, e.settlement));
			settlementUpdates.Add(e.settlement);
			countryUpdates.Add(settlementOwner);
			// GD.PrintS($"(BorderRenderSystem) Updating border for {location.hex}", e.countryTile, e.settlement);
		}

		if (hexUpdates.Count > 0) {
			gameMap.mapBorders.updateTerritoryMap(hexUpdates);
			gameMap.mapBorders.updateAreaMap(hexUpdates);

			var settlementUpdatesPerCountry = new Dictionary<Entity, HashSet<Hex>>();

			foreach (var settlement in settlementUpdates) {
				var settlementLabel = settlementLabelScene.Instance<SettlementLabel>();
				var territoryHexes = hexUpdates.Where(item => item.Item2 == settlement).Select(item => item.Item1).ToHashSet();
				gameMap.settlementLabels.Add(settlementLabel);
				var settlementData = this.GetComponent<SettlementData>(settlement);
				var settlementOwner = this.GetTarget<SettlementOwner>(settlement);
				var countryData = this.GetComponent<CountryData>(settlementOwner);
				settlementLabel.text = settlementData.name;
				settlementLabel.color = countryData.color;
				settlementLabel.position = gameMap.layout.Centroid(territoryHexes).ToVector();
				if (settlementUpdatesPerCountry.ContainsKey(settlementOwner)) {
					settlementUpdatesPerCountry[settlementOwner].UnionWith(territoryHexes);
				} else {
					settlementUpdatesPerCountry.Add(settlementOwner, territoryHexes);
				}
				GD.PrintS($"(BorderRenderSystem) Updating label for settlement {settlementData.name}");
			}

			foreach (var country in countryUpdates) {
				var countryHexes = settlementUpdatesPerCountry[country];
				var position = gameMap.layout.Centroid(countryHexes).ToVector() - new Godot.Vector2(0, 23);
				var countryName = this.GetComponent<CountryData>(country).name;
				gameMap.mapLabels.SetLabel(country, MapLabel.MapLabelType.Territory, countryName, position);
				GD.PrintS($"(BorderRenderSystem) Updating label for country {countryName}");
			}
		}
	}
}