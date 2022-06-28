using Godot;
using RelEcs;
using System;
using System.Linq;
using System.Collections.Generic;

public class CountryGenerator : IGeneratorStep {
	private HashSet<Entity> availableLandTiles;
	private Random rng;
	private GameManager manager;

	public void Generate(GameOptions options, GameManager manager) {
		var nameFactory = new NameFactory("greek");
		rng = new Random(options.Seed);
		this.manager = manager;
		var landTiles = from tile in manager.world.tiles where tile.Get<TileData>().IsLand select tile;
		availableLandTiles = landTiles.ToHashSet();

		// TODO: make setting
		var numCountries = 200; 
		var maxTilesPerTerritory = 25;

		if (availableLandTiles.Count == 0) {
			throw new Exception("No available tiles to generate countries");
		}

		var countrySettlementTiles = new Dictionary<Entity, HashSet<Entity>>();
		var countryColors = new Dictionary<Entity, Color>();

		// create countries and capital buildings
		for (int i = 0; i < numCountries; i++) {
			var countryName = nameFactory.GetName();
			var countryData = new CountryData{ name = countryName };
			var country = manager.state.Spawn();
			var countryColor = Color.FromHsv((float) rng.NextDouble(), 0.5f, 1.0f);
			country.Add<CountryData>();
			var sourceTile = findAvailableTile();
			availableLandTiles.Remove(sourceTile);
			var territoryHexes = new HashSet<Entity>();
			territoryHexes.Add(sourceTile);
			countryColors.Add(country, countryColor);
			countrySettlementTiles.Add(country, territoryHexes);

			manager.state.Send(new CountryAdded { country = country });

			// add label
			var label = new MapLabel();
			label.LabelType = MapLabel.MapLabelType.Territory;
			label.Text = countryName;
			country.Add(label);

			var hex = sourceTile.Get<Location>().hex;
			// add capital building
			AddDistrict(hex, District.DistrictType.Village);

			if (i == 0) {
				var player = new Player { playerCountry = country };
				manager.state.AddElement(player);

				// give the player a scout
				var unitFactory = manager.state.GetElement<UnitFactory>();
				unitFactory.NewUnit(country, hex, Unit.UnitType.Scout);
				unitFactory.NewUnit(country, hex.Neighbor(Direction.NorthEast, 3), Unit.UnitType.Scout);
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
			country.Get<CountryData>().color = countryColor;
			// capital territory
			var capitalName = nameFactory.GetName();
			var capitalData = new SettlementData {
				name = capitalName,
				ownerCountry = country,
			};
			var capital = manager.state.Spawn();
			capital.Add<SettlementData>(capitalData);
			capital.Add<CapitalSettlement>(new CapitalSettlement(), country);

			foreach (var tile in territoryHexes) {
				tile.Add(new CountryTile(), country);
				tile.Add(new SettlementTile(), capital);
				tile.Add(new ViewStateNode { country = country, range = 3 });
				manager.state.Send(new TileBorderUpdate { settlement = capital, tile = tile });
				manager.state.Send(new ViewStateNodeUpdated { entity = tile });
			}
		}
	}

	private Entity AddDistrict(Hex hex, District.DistrictType buildingType) {
		var districtData = new DistrictData { type = buildingType };
		var district = manager.state.Spawn();
		district.Add(districtData);
		district.Add(new Location { hex = hex });

		var sprite = new Sprite();
		sprite.Centered = false;
		sprite.Texture = ResourceLoader.Load<Texture>(District.spritePath[districtData.type]);
		district.Add(sprite);
		manager.state.Send(new SpriteAdded { entity = district });
		return district;
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