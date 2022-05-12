using RelEcs;
using System;
using System.Linq;

public class PolityGenerator : IGeneratorStep {
	public void Generate(GameOptions options, GameManager manager) {
		var namegen = new NameFactory("greek");
		var rng = new Random(options.Seed);
		var landTiles = from tile in manager.world.tiles where tile.Get<TileData>().IsLand select tile;
		var availableLandTiles = landTiles.ToHashSet();

		var numPolities = 10; // TODO: make setting

		if (availableLandTiles.Count == 0) {
			throw new Exception("No available tiles to generate polities");
		}

		for (int i = 0; i < numPolities; i++) {
			// find suitable hex for polity capital
			var sourceTile = availableLandTiles.ElementAt(rng.Next(availableLandTiles.Count));
			availableLandTiles.Remove(sourceTile);

			// polity
			var polityName = namegen.GetName();
			var polityData = new PolityData{ name = polityName };
			var polity = manager.state.Spawn();
			polity.Add<PolityData>();

			// capital territory
			var capitalName = namegen.GetName();
			var capitalTerritoryData = new TerritoryData { name = capitalName };
			var capitalTerritory = manager.state.Spawn();
			capitalTerritory.Add<TerritoryData>(capitalTerritoryData);
			capitalTerritory.Add<TerritoryOwner>(polity);

			// add capital building
			var buildingData = new BuildingData { type = Building.BuildingType.Village };
			var building = manager.state.Spawn();
			building.Add<BuildingData>(buildingData);
			building.Add<Hex>(sourceTile);

		}
	}
}