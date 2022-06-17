using RelEcs;
using System.Linq;
using System.Collections.Generic;
using Godot;

public class BorderRenderSystem : ISystem {
	private List<(Hex, Entity)> hexUpdates;
	private HashSet<Entity> settlementUpdates;
	private HashSet<Entity> countryUpdates;
	private PackedScene settlementLabelScene;

	public BorderRenderSystem() {
		hexUpdates = new List<(Hex, RelEcs.Entity)>();
		settlementUpdates = new HashSet<Entity>();
		countryUpdates = new HashSet<Entity>();

		settlementLabelScene = ResourceLoader.Load<PackedScene>("res://scenes/GameView/SettlementLabel.tscn");
	}

	public void Run(Commands commands) {
		var gameMap = commands.GetElement<GameMap>();
		var world = commands.GetElement<World>();

		hexUpdates.Clear();
		settlementUpdates.Clear();
		countryUpdates.Clear();
		
		commands.Receive((TileBorderUpdate e) => {
			var location = e.tile.Get<Location>();
			var territoryData = e.settlement.Get<SettlementData>();
			hexUpdates.Add((location.hex, e.settlement));
			settlementUpdates.Add(e.settlement);
			countryUpdates.Add(territoryData.ownerCountry);
		});

		if (hexUpdates.Count > 0) {
			gameMap.mapBorders.updateTerritoryMap(hexUpdates);
			gameMap.mapBorders.updateAreaMap(hexUpdates);

			var settlementUpdatesPerCountry = new Dictionary<Entity, HashSet<Hex>>();

			foreach (var settlement in settlementUpdates) {
				var settlementLabel = settlementLabelScene.Instance<SettlementLabel>();
				var territoryHexes = hexUpdates.Where(item => item.Item2 == settlement).Select(item => item.Item1).ToHashSet();
				gameMap.settlementLabels.Add(settlementLabel);
				var settlementData = settlement.Get<SettlementData>();
				var countryData = settlementData.ownerCountry.Get<CountryData>();
				settlementLabel.text = settlementData.name;
				settlementLabel.color = countryData.color;
				settlementLabel.position = gameMap.layout.Centroid(territoryHexes).ToVector();
				if (settlementUpdatesPerCountry.ContainsKey(settlementData.ownerCountry)) {
					settlementUpdatesPerCountry[settlementData.ownerCountry].UnionWith(territoryHexes);
				} else {
					settlementUpdatesPerCountry.Add(settlementData.ownerCountry, territoryHexes);
				}
			}

			foreach (var country in countryUpdates) {
				var label = country.Get<MapLabel>();
				var countryHexes = settlementUpdatesPerCountry[country];
				gameMap.mapLabels.AddLabel(label);
				label.SetPosition(gameMap.layout.Centroid(countryHexes).ToVector() - new Godot.Vector2(0, 23));
			}
		}
	}
}