using RelEcs;
using System.Linq;
using System.Collections.Generic;

public class TerritoryRenderSystem : ISystem {
	private List<(Hex, Entity)> hexUpdates;
	private HashSet<Entity> territoryUpdates;
	private HashSet<Entity> polityUpdates;

	public TerritoryRenderSystem() {
		hexUpdates = new List<(Hex, RelEcs.Entity)>();
		territoryUpdates = new HashSet<Entity>();
		polityUpdates = new HashSet<Entity>();
	}

	public void Run(Commands commands) {
		var gameMap = commands.GetElement<GameMap>();
		var world = commands.GetElement<WorldService>();

		hexUpdates.Clear();
		territoryUpdates.Clear();
		polityUpdates.Clear();
		
		commands.Receive((TerritoryTileUpdate e) => {
			var hex = e.tile.Get<Hex>();
			var territoryData = e.territory.Get<TerritoryData>();
			hexUpdates.Add((hex, e.territory));
			territoryUpdates.Add(e.territory);
			polityUpdates.Add(territoryData.ownerPolity);
		});

		if (hexUpdates.Count > 0) {
			gameMap.mapBorders.updateTerritoryMap(hexUpdates);
			gameMap.mapBorders.updateAreaMap(hexUpdates);

			var territoryUpdatesPerPolity = new Dictionary<Entity, HashSet<Hex>>();

			foreach (var territory in territoryUpdates) {
				var label = territory.Get<MapLabel>();
				var territoryHexes = hexUpdates.Where(item => item.Item2 == territory).Select(item => item.Item1).ToHashSet();
				gameMap.mapLabels.RemoveChild(label);
				gameMap.mapLabels.AddChild(label);
				label.SetPosition(gameMap.layout.Centroid(territoryHexes).ToVector());

				var territoryData = territory.Get<TerritoryData>();
				if (territoryUpdatesPerPolity.ContainsKey(territoryData.ownerPolity)) {
					territoryUpdatesPerPolity[territoryData.ownerPolity].UnionWith(territoryHexes);
				} else {
					territoryUpdatesPerPolity.Add(territoryData.ownerPolity, territoryHexes);
				}
			}

			foreach (var polity in polityUpdates) {
				var label = polity.Get<MapLabel>();
				var polityHexes = territoryUpdatesPerPolity[polity];
				gameMap.mapLabels.RemoveChild(label);
				gameMap.mapLabels.AddChild(label);
				label.SetPosition(gameMap.layout.Centroid(polityHexes).ToVector());
			}
		}
	}
}