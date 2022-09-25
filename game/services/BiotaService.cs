using System.Linq;
using Godot;


public class BiotaService {
	private readonly GameManager manager;
	private MultiMap<Entity, Entity> biotaByTile = new MultiMap<Entity, Entity>();
	private MultiMap<Entity, Entity> plantsByTile = new MultiMap<Entity, Entity>();
	private MultiMap<Entity, Entity> animalsByTile = new MultiMap<Entity, Entity>();
	private Dictionary<Entity, MultiSet<BiotaClassification, Entity>> biotaByTileAndClassification = new Dictionary<Entity, MultiSet<BiotaClassification, Entity>>();

	public BiotaService(GameManager manager) {
		this.manager = manager;
	}

	public void AddBiota(BiotaAdded biotaAdded) {
		var biotaType = biotaAdded.biota.Get<BiotaData>().biotaType;
		biotaByTile.Add(biotaAdded.tile, biotaAdded.biota);
		if (biotaType.category == BiotaCategory.Plant) {
			plantsByTile.Add(biotaAdded.tile, biotaAdded.biota);
		} else if (biotaType.category == BiotaCategory.Animal) {
			animalsByTile.Add(biotaAdded.tile, biotaAdded.biota);
		}

		if (!biotaByTileAndClassification.ContainsKey(biotaAdded.tile)) {
			biotaByTileAndClassification[biotaAdded.tile] = new MultiSet<BiotaClassification, Entity>();
		}
		foreach (var classification in biotaType.classifications) {
			biotaByTileAndClassification[biotaAdded.tile].Add(classification, biotaAdded.biota);
		}
	}

	private int getFitness(TileData tileData, BiotaData biotaData) {
		// TODO: implement fitness function depending on tile data
		return 1;
	}

	public void CalculateTile(Entity tile) {
		var tileData = tile.Get<TileData>();
		var plants = from entity in plantsByTile[tile] select entity.Get<BiotaData>();
		var animals = from entity in animalsByTile[tile] select entity.Get<BiotaData>();
		
		if (biotaByTile[tile].Count == 0) {
			return;
		}

		var biotaByClassification = biotaByTileAndClassification[tile];

		// calculate space used
		tileData.plantSpaceUsed = 0;
		foreach (var plant in plants) {
			tileData.plantSpaceUsed += plant.biotaType.plantRequirements.space * plant.size;
		}

		tileData.animalSpaceUsed = 0;
		foreach (var animal in animals) {
			tileData.animalSpaceUsed += animal.biotaType.animalRequirements.space * animal.size;
		}

		foreach (var plant in plants) {
			plant.births = 0;
			plant.deathsKilled = 0;
			plant.deathsStarved = 0;
		}

		// sort plants by fitness
		var sortedPlants = from plant in plants
			where plant.size > 0
			orderby getFitness(tileData, plant) descending
			select plant;

		// sort animals by fitness
		var sortedAnimals = from animal in animals
			where animal.size > 0
			orderby getFitness(tileData, animal) descending
			select animal;

		// calculate plant growth
		var spaceLeft = tileData.plantSpace - tileData.plantSpaceUsed;
		foreach (var plant in sortedPlants) {
			if (spaceLeft <= 0) {
				break;
			}
			var births = plant.size * plant.biotaType.reproductionRate;
			var birthsReal = (int) Mathf.Floor(Mathf.Min(spaceLeft, births));
			plant.size += birthsReal;
			plant.births += birthsReal;
			spaceLeft -= birthsReal;
		}

		foreach (var animal in animals) {
			animal.births = 0;
			animal.deathsKilled = 0;
			animal.deathsStarved = 0;
		}
		
		// calculate animal consumption
		foreach (var animal in sortedAnimals) {
			GD.PrintS($"Consumption: {animal.biotaType.name}");
			var needs = animal.biotaType.animalRequirements.nutrition;

			foreach (var need in needs) {
				// for each need, find biota to consume
				var biotaSatisfyingNeed = biotaByClassification[need.classification];
				var needSize = (int) Mathf.Ceil(need.amount * ((float) animal.size));
				var needFulfilled = 0;
				foreach (var consumedBiota in biotaSatisfyingNeed) {
					BiotaData consumedBiotaData = consumedBiota.Get<BiotaData>();
					var needRemaining = needSize - needFulfilled;

					var consumedSizeReal = (int) Mathf.Min(needRemaining, consumedBiotaData.size);
					consumedBiotaData.size -= consumedSizeReal;
					consumedBiotaData.deathsKilled += consumedSizeReal;
					GD.PrintS($"\tconsume {consumedSizeReal} {consumedBiotaData.biotaType.name}");
					needFulfilled += consumedSizeReal;
				}
				GD.PrintS("\tneedFulfilled", needFulfilled);
				GD.PrintS("\tneedSize", needSize);
				float needPercentFilled = (float) decimal.ToDouble(decimal.Divide(needFulfilled, needSize));
				GD.PrintS("\tneedPercentFilled", needPercentFilled);
				animal.needPercentFilled = needPercentFilled;

				// grow population
				var births = (int) Mathf.Floor(animal.size * animal.biotaType.reproductionRate * needPercentFilled);
				animal.size += births;
				animal.births = births;

				// if we have left over needs, reduce population because of starvation
				var starved = (int) Mathf.Floor(animal.size * ((1 - needPercentFilled) / 10));
				animal.size -= starved;
				animal.deathsStarved += starved;
			}
		}

		// handle plant spread
		var worldService = manager.state.GetElement<WorldService>();
		foreach (var plant in plants) {
			foreach (var neighbor in worldService.GetNeighbors(tile)) {
				var percentShare = tileData.plantSpace;
			}
		}

		// TODO: handle animal migration

		// TODO: delete biota with size 0

	}

	public void DebugTile(Entity tile) {
		if (biotaByTile[tile].Count == 0) {
			return;
		}
		var tileData = tile.Get<TileData>();
		var plants = from entity in plantsByTile[tile] select entity.Get<BiotaData>();
		var animals = from entity in animalsByTile[tile] select entity.Get<BiotaData>();

		GD.PrintS($"Tile biota {tile.Get<Location>().hex}");
		GD.PrintS("\tPlants:");
		foreach (var plant in plants) {
			GD.PrintS($"\t\t{plant.biotaType.name} (size: {plant.size} births: {plant.births} killed: {plant.deathsKilled} starved: {plant.deathsStarved})");
		}
		GD.PrintS("\tAnimals:");
		foreach (var animal in animals) {
			GD.PrintS($"\t\t{animal.biotaType.name} (size: {animal.size} births: {animal.births} killed: {animal.deathsKilled} starved: {animal.deathsStarved} need percent fulfilled: {animal.needPercentFilled})");
		}
	}
}