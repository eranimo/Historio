using Godot;
using RelEcs;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq;

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

		for (int i = 0; i < numPolities; i++) {
			// polity
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

			// add capital building
			var buildingData = new BuildingData { type = Building.BuildingType.Village };
			var building = manager.state.Spawn();
			var sourceTileCoord = sourceTile.Get<Hex>();
			building.Add(buildingData);
			building.Add(sourceTileCoord);

			var sprite = new Sprite();
			sprite.Centered = false;
			sprite.Texture = ResourceLoader.Load<Texture>(Building.buildingTypeSpritePath[buildingData.type]);
			building.Add(sprite);
			manager.state.Send(new SpriteAdded { entity = building });
		}

		for(var j = 0; j < maxTilesPerTerritory; j++) {
			foreach (var (polity, territoryHexes) in polityTerritory) {
				var newTile = findAvailableTileBorderingSet(territoryHexes);
				if (newTile is not null) {
					availableLandTiles.Remove(newTile);
					territoryHexes.Add(newTile);
				}
			}
		}

		foreach (var (polity, territoryHexes) in polityTerritory) {
			var polityColor = polityColors[polity];
			// capital territory
			var capitalName = nameFactory.GetName();
			var capitalTerritoryData = new TerritoryData {
				name = capitalName,
				color = polityColor,
			};
			var capitalTerritory = manager.state.Spawn();
			capitalTerritory.Add<TerritoryData>(capitalTerritoryData);
			capitalTerritory.Add<TerritoryPolityOwner>(polity);

			foreach (var tile in territoryHexes) {
				manager.state.Send(new TerritoryTileUpdate { territory = capitalTerritory, tile = tile });
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
			// return neighboringAvailable.ElementAt(rng.Next(neighboringAvailable.Count));
		}
		return null;
	}
}