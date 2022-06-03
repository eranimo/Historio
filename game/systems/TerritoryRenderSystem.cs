using RelEcs;
using System.Collections.Generic;

public class TerritoryRenderSystem : ISystem {
	private List<(Hex, Entity)> updates;

	public TerritoryRenderSystem() {
		updates = new List<(Hex, RelEcs.Entity)>();
	}

	public void Run(Commands commands) {
		var gameMap = commands.GetElement<GameMap>();

		updates.Clear();
		
		commands.Receive((TerritoryTileUpdate e) => {
			var hex = e.tile.Get<Hex>();
			var territory = e.territory.Get<TerritoryData>();
			updates.Add((hex, e.territory));
		});

		if (updates.Count > 0) {
			gameMap.mapBorders.updateTerritoryMap(updates);
			gameMap.mapBorders.updateAreaMap(updates);
		}
	}
}