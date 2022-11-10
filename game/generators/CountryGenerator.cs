using Godot;
using RelEcs;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class CountryGenerator : IGeneratorStep {
	private HashSet<Entity> availableLandTiles;
	private Random rng;
	private GameManager manager;

	public void Generate(GameOptions options, GameManager manager) {
		var factories = manager.state.GetElement<Factories>();
		var nameFactory = new NameFactory("greek");
		rng = new Random(options.Seed);
		this.manager = manager;
		var landTiles = from tile in manager.world.tiles where manager.Get<TileData>(tile).IsLand select tile;
		availableLandTiles = landTiles.ToHashSet();

		// TODO: make setting
		var numCountries = 1; 
		var maxTilesPerTerritory = 25;

		if (availableLandTiles.Count == 0) {
			throw new Exception("No available tiles to generate countries");
		}

		var countrySettlementTiles = new Dictionary<Entity, HashSet<Entity>>();
		var countryColors = new Dictionary<Entity, Color>();

		GD.PrintS(Defs.Resource.Get("lumber").category);

		// create countries and capital buildings
		for (int i = 0; i < numCountries; i++) {
			var countryName = nameFactory.GetName();
			var countryData = new CountryData{ name = countryName };
			var country = manager.state.Spawn().Id();
			var countryColor = Color.FromHSV((float) rng.NextDouble(), 0.5f, 1.0f);
			manager.On(country).Add(countryData).Add<Persisted>();
			var sourceTile = findAvailableTile();
			availableLandTiles.Remove(sourceTile);
			var territoryHexes = new HashSet<Entity>();
			territoryHexes.Add(sourceTile);
			countryColors.Add(country, countryColor);
			countrySettlementTiles.Add(country, territoryHexes);

			manager.state.Send(new CountryAdded { country = country });

			var hex = manager.Get<Location>(sourceTile).hex;
			// add capital building
			factories.districtFactory.AddDistrict(hex, Defs.District.Get("village"));

			if (i == 0) {
				var player = new Player { playerCountry = country };
				manager.state.AddElement(player);

				// give the player a scout
				factories.unitFactory.NewUnit(country, hex, Defs.Unit.Get("scout"));
				factories.unitFactory.NewUnit(country, hex.Neighbor(HexDirection.NorthEast, 3), Defs.Unit.Get("builder"));

				var testFarm = hex.Neighbor(HexDirection.North);
				factories.improvementFactory.AddImprovement(testFarm, Defs.Improvement.Get("farm"));
				
				var t =  manager.world.GetTile(hex.Neighbor(HexDirection.South, 1));
				factories.biotaFactory.Add(t, Defs.BiotaType.Get("grass"), 1000);
				factories.biotaFactory.Add(t, Defs.BiotaType.Get("vegetables"), 500);
				factories.biotaFactory.Add(t, Defs.BiotaType.Get("rabbit"), 100);
				factories.biotaFactory.Add(t, Defs.BiotaType.Get("wolf"), 5);
			}
		}

		// grow each countries territory
		for(var j = 0; j < maxTilesPerTerritory; j++) {
			foreach (var (country, territoryHexes) in countrySettlementTiles) {
				var newTile = findAvailableTileBorderingSet(territoryHexes);
				if (newTile is not null) {
					availableLandTiles.Remove(newTile);
					territoryHexes.Add(newTile);
				}
			}
		}

		// add settlement entities
		foreach (var (country, territoryHexes) in countrySettlementTiles) {
			var countryColor = countryColors[country];
			manager.Get<CountryData>(country).color = countryColor;
			// capital territory
			var capitalName = nameFactory.GetName();
			var capitalData = new SettlementData {
				name = capitalName,
			};
			var capital = manager.Spawn()
				.Add<SettlementData>(capitalData)
				.Add<CapitalSettlement>(country)
				.Add<SettlementOwner>(country)
				.Id();

			foreach (var tile in territoryHexes) {
				var hex = manager.Get<Location>(tile).hex;
				var countryTile = manager.Spawn()
					.Add(new Location { hex = hex })
					.Add(new ViewStateNode { range = 3 })
					.Add<CountryTile>(country)
					.Add<ViewStateOwner>(country)
					.Add<CountryTileSettlement>(capital)
					.Id();

				manager.state.Send(new SettlementBorderUpdated { settlement = capital, countryTile = countryTile });
				manager.state.Send(new ViewStateNodeUpdated { entity = countryTile });
			}
		}
	}

	private Entity findAvailableTile() {
		return availableLandTiles.ElementAt(rng.Next(availableLandTiles.Count));
	}

	private Entity findAvailableTileBorderingSet(HashSet<Entity> borderingSet) {
		var neighboringAvailable = new HashSet<Entity>();
		var tileAvailableNeighbors = new HashSet<Entity>();
		foreach (var tile in borderingSet) {
			tileAvailableNeighbors.Clear();
			foreach (var neighborTile in manager.world.GetNeighbors(tile)) {
				if (!borderingSet.Contains(neighborTile)) {
					tileAvailableNeighbors.Add(neighborTile);
				}
			}

			foreach (var neighborTile in tileAvailableNeighbors) {
				if (availableLandTiles.Contains(neighborTile)) {
					neighboringAvailable.Add(neighborTile);
				}
			}
		}
		if (neighboringAvailable.Count > 0) {
			return neighboringAvailable
				.Select((Entity tile) => {
					var numValidNeighbors = 0;
					foreach (var neighborTile in manager.world.GetNeighbors(tile)) {
						if (!borderingSet.Contains(neighborTile)) {
							numValidNeighbors++;
						}
					}
					return (tile, numValidNeighbors);
				})
				.OrderBy(item => item.numValidNeighbors)
				.Select(item => item.tile)
				.First();
		}
		return null;
	}
}