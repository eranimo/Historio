using System;
using System.Linq;
using Godot;

public partial class BiotaTileData {
	private readonly TileData tileData;
	public MultiSet<BiotaCategory, BiotaData> biotaByCategory;
	public Dictionary<BiotaType, BiotaData> biotaByType;
	public MultiSet<BiotaClassification, BiotaData> biotaByClassification;

	public HashSet<BiotaData> biota;
	public HashSet<BiotaData> plants;
	public HashSet<BiotaData> animals;

	public BiotaTileData(TileData tileData) {
		this.tileData = tileData;
	}

	public void UpdateTileBiota(List<BiotaData> biotaList) {
		biotaByCategory = new MultiSet<BiotaCategory, BiotaData>();
		biotaByType = new Dictionary<BiotaType, BiotaData>();
		biotaByClassification = new MultiSet<BiotaClassification, BiotaData>();
		biota = new HashSet<BiotaData>();
		plants = new HashSet<BiotaData>();
		animals = new HashSet<BiotaData>();

		foreach (var biotaData in biotaList) {
			biotaByType[biotaData.biotaType] = biotaData;
			biota.Add(biotaData);
			if (biotaData.biotaType.category == BiotaCategory.Plant) {
				plants.Add(biotaData);
			} else if (biotaData.biotaType.category == BiotaCategory.Animal) {
				animals.Add(biotaData);
			}

			biotaByCategory.Add(biotaData.biotaType.category, biotaData);

			foreach (var classification in biotaData.biotaType.classifications) {
				biotaByClassification.Add(classification, biotaData);
			}
		}

		tileData.plantSpaceUsed = plants.Sum(plant => plant.spaceUsed);
		tileData.animalSpaceUsed = animals.Sum(animal => animal.spaceUsed);
	}
}

public partial class BiotaService {
	private readonly GameManager manager;
	private MultiMap<TileData, BiotaData> biotaByTile = new MultiMap<TileData, BiotaData>();
	private Dictionary<TileData, BiotaTileData> tilesBiotaData = new Dictionary<TileData, BiotaTileData>();

	public BiotaService(GameManager manager) {
		this.manager = manager;
	}

	public void AddBiota(BiotaData biotaData, TileData tileData) {
		biotaByTile.Add(tileData, biotaData);
	}

	public BiotaData GetBiotaByType(TileData tileData, BiotaType type) {
		if (!tilesBiotaData.ContainsKey(tileData)) {
			return null;
		}
		return tilesBiotaData[tileData].biotaByType[type];
	}

	public bool HasBiotaType(TileData tileData, BiotaType type) {
		if (!tilesBiotaData.ContainsKey(tileData)) {
			return false;
		}
		return tilesBiotaData[tileData].biotaByType.ContainsKey(type);
	}

	// update tile when biota are added or removed
	public void UpdateTileBiota(TileData tileData) {
		if (!tilesBiotaData.ContainsKey(tileData)) {
			tilesBiotaData[tileData] = new BiotaTileData(tileData);
		}
		tilesBiotaData[tileData].UpdateTileBiota(biotaByTile[tileData]);
	}

	public BiotaTileData GetBiotaTileData(TileData tileData) {
		if (!tilesBiotaData.ContainsKey(tileData)) {
			tilesBiotaData[tileData] = new BiotaTileData(tileData);
		}
		return tilesBiotaData[tileData];
	}

	public int CalculateTile(TileData tileData) {
		if (biotaByTile[tileData].Count == 0) {
			return 0;
		}
		var biotaTileData = tilesBiotaData[tileData];

		// calculate plant growth
		var plantSpaceFree = tileData.plantSpace - tileData.plantSpaceUsed;
		GD.PrintS("Plant space", tileData.plantSpace);
		GD.PrintS("Plant space free", plantSpaceFree);
		GD.PrintS("Plant space used", tileData.plantSpaceUsed);
		foreach (var plant in biotaTileData.plants) {
			GD.PrintS($"\tGrowth for {plant.biotaType.name} (size: {plant.size} space used: {plant.spaceUsed})");
			var share = ((float) plant.spaceUsed) / tileData.plantSpace;
			var reproductionRate = plant.biotaType.reproductionRate;
			var births = (int) Math.Round(plantSpaceFree * share * reproductionRate);
			GD.PrintS($"\t\tShare = {share}");
			GD.PrintS($"\t\tReproduction rate = {reproductionRate}");
			GD.PrintS($"\t\tBirths = {births}");
			plant.size += births;
			plant.births = births;
		}

		tileData.plantSpaceUsed = biotaTileData.plants.Sum(plant => plant.biotaType.space * plant.size);
		GD.PrintS("Plant space used", tileData.plantSpaceUsed);
		
		// foreach (var animal in biotaTileData.animals) {
		// 	animal.births = 0;
		// 	animal.deathsKilled = 0;
		// 	animal.deathsStarved = 0;
		// 	animal.migrated = 0;
		// }

		// // calculate animal consumption
		// var animalSpaceLeft = tileData.animalSpace - tileData.animalSpaceUsed;
		// foreach (var animal in biotaTileData.animals) {
		// 	// GD.PrintS($"Consumption: {animal.biotaType.name}");
		// 	var needs = animal.biotaType.animalRequirements.nutrition;

		// 	float totalNeedsFulfilled = 1f;
		// 	foreach (var need in needs) {
		// 		// for each need, find biota to consume
		// 		var biotaSatisfyingNeed = biotaTileData.biotaByClassification[need.classification];
		// 		var needSize = (int) Mathf.Ceil(need.amount * ((float) animal.size));
		// 		var needFulfilled = 0;
		// 		foreach (var consumedBiota in biotaSatisfyingNeed) {
		// 			var needRemaining = needSize - needFulfilled;
		// 			var consumedSizeReal = (int) Mathf.Min(needRemaining, consumedBiota.size);
		// 			consumedBiota.size -= consumedSizeReal;
		// 			consumedBiota.deathsKilled += consumedSizeReal;
		// 			needFulfilled += consumedSizeReal;
		// 		}
		// 		float needPercentFilled = (float) decimal.ToDouble(decimal.Divide(needFulfilled, needSize));
		// 		totalNeedsFulfilled *= needPercentFilled;
		// 	}

		// 	animal.needPercentFilled = totalNeedsFulfilled;

		// 	// grow population
		// 	var births = (int) Mathf.Ceil(animal.size * animal.biotaType.reproductionRate * totalNeedsFulfilled);
		// 	var birthsReal = (int) Mathf.Floor(Mathf.Min(animalSpaceLeft, births));
		// 	animal.size += birthsReal;
		// 	animal.births = birthsReal;

		// 	// if we have left over needs, reduce population because of starvation
		// 	var starved = (int) Mathf.Ceil(animal.size * ((1 - totalNeedsFulfilled) / 10));
		// 	animal.size -= starved;
		// 	animal.deathsStarved += starved;

		// 	animalSpaceLeft -= birthsReal;
		// }
		// tileData.animalSpaceUsed = tileData.animalSpace - animalSpaceLeft;

		// // handle plant spread
		// var worldService = manager.state.GetElement<WorldService>();
		// var biotaFactory = manager.state.GetElement<Factories>().biotaFactory;
		// var rng = new RandomNumberGenerator();
		// foreach (var plant in biotaTileData.plants) {
		// 	var percentShare = (float) plant.size / tileData.plantSpace;
		// 	// GD.PrintS($"Spread: {plant.biotaType.name} ({tile.Get<Location>().hex})");

		// 	// for each neighbor, decide if spreading there
		// 	foreach (var neighbor in worldService.GetNeighbors(tile)) {
		// 		var chance = percentShare * 0.5;
		// 		if (rng.Randf() < chance) {
		// 			var spreadAmount = (int) Mathf.Round(plant.size / 10f);
		// 			if (!HasBiotaType(neighbor, plant.biotaType)) {
		// 				// GD.PrintS($"\tSpread to ({neighbor.Get<Location>().hex}) with size {spreadAmount}");
		// 				biotaFactory.Add(neighbor, plant.biotaType, spreadAmount);
		// 			}
		// 		}
		// 	}
		// }

		// // handle animal migration
		// foreach (var animal in biotaTileData.animals) {
		// 	if (animal.deathsStarved == 0) {
		// 		continue;
		// 	}
		// 	// GD.PrintS($"Migrate: {animal.biotaType.name} ({tile.Get<Location>().hex})");
		// 	var percentChanceToMigrate = ((float) animal.deathsStarved) / ((float) animal.size);
		// 	// GD.PrintS("\tpercentChanceToMigrate", percentChanceToMigrate);
		// 	if (percentChanceToMigrate > 0) {
		// 		int totalMigrated = 0;

		// 		// size of the "stressed" population
		// 		// e.g. if 80% of needs are met, 20% of the population can migrate
		// 		int maxMigrationSize = (int) Mathf.Round((1 - animal.needPercentFilled) * ((float) animal.size));
		// 		foreach (var neighbor in worldService.GetNeighbors(tile)) {
		// 			if (rng.Randf() < percentChanceToMigrate) {
		// 				// up to 1/6 of the max migration size can move to each neighbor
		// 				var migrateAmountMax = (int) Mathf.Ceil(maxMigrationSize / 6f);
		// 				var migrateAmount = rng.RandiRange(0, migrateAmountMax);
		// 				totalMigrated += migrateAmount;
		// 				// GD.PrintS($"\t{animal.biotaType.name} ({tile.Get<Location>().hex}) migrated to ({neighbor.Get<Location>().hex}) with size {migrateAmount}");
		// 				biotaFactory.Add(neighbor, animal.biotaType, migrateAmount);
		// 			}
		// 		}
		// 		animal.size -= totalMigrated;
		// 		animal.migrated += totalMigrated;
		// 	}
		// }

		// TODO: delete biota with size 0

		return biotaTileData.biota.Count;
	}

	public void DebugTile(Entity tile) {
		var tileData = manager.Get<TileData>(tile);
		if (biotaByTile[tileData].Count == 0) {
			return;
		}
		var plants = tilesBiotaData[tileData].plants;
		var animals = tilesBiotaData[tileData].animals;

		GD.PrintS($"Tile biota {manager.Get<Location>(tile).hex}");
		GD.PrintS("\tPlants:");
		foreach (var plant in plants) {
			var share = Math.Round((double) (((float) plant.size) / ((float) tileData.plantSpace)) * 100, 2);
			GD.PrintS($"\t\t{plant.biotaType.name} (size: {plant.size} births: {plant.births} killed: {plant.deathsKilled} share: {share}%)");
		}
		GD.PrintS("\tAnimals:");
		foreach (var animal in animals) {
			var share = Math.Round((double) (((float) animal.size) / ((float) tileData.animalSpace)) * 100, 2);
			GD.PrintS($"\t\t{animal.biotaType.name} (size: {animal.size} births: {animal.births} killed: {animal.deathsKilled} starved: {animal.deathsStarved} needs: {(int) (animal.needPercentFilled * 100)}% share: {share}%)");
		}
	}
}