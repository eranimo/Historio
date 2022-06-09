using RelEcs;
using System.Linq;
using System.Collections.Generic;
using Godot;

public class BorderRenderSystem : ISystem {
	private List<(Hex, Entity)> hexUpdates;
	private HashSet<Entity> settlementUpdates;
	private HashSet<Entity> polityUpdates;
	private PackedScene settlementLabelScene;

	public BorderRenderSystem() {
		hexUpdates = new List<(Hex, RelEcs.Entity)>();
		settlementUpdates = new HashSet<Entity>();
		polityUpdates = new HashSet<Entity>();

		settlementLabelScene = ResourceLoader.Load<PackedScene>("res://scenes/GameView/SettlementLabel.tscn");
	}

	public void Run(Commands commands) {
		var gameMap = commands.GetElement<GameMap>();
		var world = commands.GetElement<World>();

		hexUpdates.Clear();
		settlementUpdates.Clear();
		polityUpdates.Clear();
		
		commands.Receive((TileBorderUpdate e) => {
			var location = e.tile.Get<Location>();
			var territoryData = e.settlement.Get<SettlementData>();
			hexUpdates.Add((location.hex, e.settlement));
			settlementUpdates.Add(e.settlement);
			polityUpdates.Add(territoryData.ownerPolity);
		});

		if (hexUpdates.Count > 0) {
			gameMap.mapBorders.updateTerritoryMap(hexUpdates);
			gameMap.mapBorders.updateAreaMap(hexUpdates);

			var settlementUpdatesPerPolity = new Dictionary<Entity, HashSet<Hex>>();

			foreach (var settlement in settlementUpdates) {
				var settlementLabel = settlementLabelScene.Instance<SettlementLabel>();
				var territoryHexes = hexUpdates.Where(item => item.Item2 == settlement).Select(item => item.Item1).ToHashSet();
				gameMap.settlementLabels.Add(settlementLabel);
				var settlementData = settlement.Get<SettlementData>();
				var polityData = settlementData.ownerPolity.Get<PolityData>();
				settlementLabel.text = settlementData.name;
				settlementLabel.color = polityData.color;
				settlementLabel.position = gameMap.layout.Centroid(territoryHexes).ToVector();
				if (settlementUpdatesPerPolity.ContainsKey(settlementData.ownerPolity)) {
					settlementUpdatesPerPolity[settlementData.ownerPolity].UnionWith(territoryHexes);
				} else {
					settlementUpdatesPerPolity.Add(settlementData.ownerPolity, territoryHexes);
				}
			}

			foreach (var polity in polityUpdates) {
				var label = polity.Get<MapLabel>();
				var polityHexes = settlementUpdatesPerPolity[polity];
				gameMap.mapLabels.AddLabel(label);
				label.SetPosition(gameMap.layout.Centroid(polityHexes).ToVector() - new Godot.Vector2(0, 23));
			}
		}
	}
}