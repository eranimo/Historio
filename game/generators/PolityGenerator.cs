using Godot;
using RelEcs;
using System;
using System.Linq;
using System.Collections.Generic;

public class PolityGenerator : IGeneratorStep {
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
		var numPolities = 200; 
		var maxTilesPerTerritory = 25;

		if (availableLandTiles.Count == 0) {
			throw new Exception("No available tiles to generate polities");
		}

		var polityTerritory = new Dictionary<Entity, HashSet<Entity>>();
		var polityColors = new Dictionary<Entity, Color>();

		// create polities and capital buildings
		for (int i = 0; i < numPolities; i++) {
			var polityName = nameFactory.GetName();
			var polityData = new PolityData{ name = polityName };
			var polity = manager.state.Spawn();
			var polityColor = Color.FromHsv((float) rng.NextDouble(), 0.5f, 1.0f);
			polity.Add<PolityData>();
			var sourceTile = findAvailableTile();
			availableLandTiles.Remove(sourceTile);
			var territoryHexes = new HashSet<Entity>();
			territoryHexes.Add(sourceTile);
			polityColors.Add(polity, polityColor);
			polityTerritory.Add(polity, territoryHexes);

			manager.state.Send(new PolityAdded { polity = polity });

			// add label
			var label = new MapLabel();
			label.LabelType = MapLabel.MapLabelType.Territory;
			label.Text = polityName;
			polity.Add(label);

			var hex = sourceTile.Get<Hex>();
			// add capital building
			AddBuilding(hex, Building.BuildingType.Village);

			if (i == 0) {
				var player = new Player { playerPolity = polity };
				manager.state.AddElement(player);

				// give the player a scout
				AddUnit(hex, Unit.UnitType.Scout);
			}
		}

		// grow each polities territory
		for(var j = 0; j < maxTilesPerTerritory; j++) {
			foreach (var (polity, territoryHexes) in polityTerritory) {
				var newTile = findAvailableTileBorderingSet(territoryHexes);
				if (newTile is not null) {
					availableLandTiles.Remove(newTile);
					territoryHexes.Add(newTile);
				}
			}
		}

		// add territory entities
		foreach (var (polity, territoryHexes) in polityTerritory) {
			var polityColor = polityColors[polity];
			// capital territory
			var capitalName = nameFactory.GetName();
			var capitalTerritoryData = new TerritoryData {
				name = capitalName,
				color = polityColor,
				ownerPolity = polity,
			};
			var capitalTerritory = manager.state.Spawn();
			capitalTerritory.Add<TerritoryData>(capitalTerritoryData);

			// add label
			var label = new MapLabel();
			label.LabelType = MapLabel.MapLabelType.Region;
			label.Text = capitalName;
			capitalTerritory.Add(label);

			foreach (var tile in territoryHexes) {
				tile.Add(new TerritoryTile { territory = capitalTerritory });
				manager.state.Send(new TerritoryTileUpdate { territory = capitalTerritory, tile = tile });
			}
		}
	}

	private Entity AddBuilding(Hex hex, Building.BuildingType buildingType) {
		var buildingData = new BuildingData { type = buildingType };
		var building = manager.state.Spawn();
		building.Add(buildingData);
		building.Add(hex);

		var sprite = new Sprite();
		sprite.Centered = false;
		sprite.Texture = ResourceLoader.Load<Texture>(Building.buildingTypeSpritePath[buildingData.type]);
		building.Add(sprite);
		manager.state.Send(new SpriteAdded { entity = building });
		return building;
	}

	private Entity AddUnit(Hex hex, Unit.UnitType unitType) {
		var unitData = new UnitData { type = unitType };
		var unit = manager.state.Spawn();
		unit.Add(unitData);
		unit.Add(hex);

		var sprite = new Sprite();
		sprite.Centered = false;
		sprite.Texture = ResourceLoader.Load<Texture>(Unit.unitTypeSpritePath[unitType]);
		unit.Add(sprite);
		manager.state.Send(new SpriteAdded { entity = unit });

		var movement = new Movement();
		movement.currentTarget = hex.Neighbor(Direction.South);
		unit.Add(movement);
		return unit;
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