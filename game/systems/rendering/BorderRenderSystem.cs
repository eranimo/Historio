using RelEcs;
using System.Linq;
using System.Collections.Generic;
using Godot;

public partial class BorderRenderSystem : ISystem {
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
		var gameMap = World.GetElement<GameMap>();
		var world = World.GetElement<WorldService>();

		hexUpdates.Clear();
		settlementUpdates.Clear();
		countryUpdates.Clear();
		
		foreach (var e in World.Receive<SettlementBorderUpdated>(this)) {
			var location = World.GetComponent<Location>(e.countryTile);
			var territoryData = World.GetComponent<SettlementData>(e.settlement);
			var settlementOwner = World.GetTarget<SettlementOwner>(e.settlement);
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
				var settlementLabel = settlementLabelScene.Instantiate<SettlementLabel>();
				var territoryHexes = hexUpdates.Where(item => item.Item2 == settlement).Select(item => item.Item1).ToHashSet();
				gameMap.settlementLabels.Add(settlementLabel);
				var settlementData = World.GetComponent<SettlementData>(settlement);
				var settlementOwner = World.GetTarget<SettlementOwner>(settlement);
				var countryData = World.GetComponent<CountryData>(settlementOwner);
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
				var countryName = World.GetComponent<CountryData>(country).name;
				gameMap.mapLabels.SetLabel(country, MapLabel.MapLabelType.Territory, countryName, position);
				GD.PrintS($"(BorderRenderSystem) Updating label for country {countryName}");
			}
		}
	}
}