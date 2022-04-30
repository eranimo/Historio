using Godot;
using System;
using System.Linq;

public class PolityGenerator : IGeneratorStep {
	public void Generate(GameOptions options, GameManager manager) {
		var namegen = new NameGenerator("greek");
		var rng = new Random(options.Seed);
		var landTiles = from tile in manager.world.tiles where tile.IsLand select tile;
		var availableLandTiles = landTiles.ToHashSet();

		var numPolities = 10;

		Polity[] polities = new Polity[numPolities];

		for (int i = 0; i < numPolities; i++) {
			var sourceTile = availableLandTiles.ElementAt(rng.Next(availableLandTiles.Count));
			availableLandTiles.Remove(sourceTile);
			var polityName = namegen.GetName();
			var polity = new Polity(polityName);

			var settlementName = namegen.GetName();
			var capital = new Settlement(settlementName);
			capital.territory.Add(sourceTile);
			polity.settlements.Add(capital);
			manager.AddEntity(capital);

			var capitalBuilding = new Building(Building.BuildingType.Village, sourceTile);
			manager.AddEntity(capitalBuilding);

			polities[i] = polity;
			manager.AddEntity(polity);
		}
	}
}