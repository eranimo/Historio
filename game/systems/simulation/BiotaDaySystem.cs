using Godot;
using System.Linq;

/*

Biota System:

Each tile has:
- plant space used, plant space max
- animal space used, animal space max
- soil fertility amount

On each day:
	For each tile with biota:
		- handle plant growth:
			- calculate remaining plant space at this tile
			- calculate reproduction rate, capacity share
			- calculate birth for each plant
				- birth is proportional to the remaining capacity, capacity share
		- consumption for animals:
			- process plant consumption for each animal
			- process animal consumption for each animal
		- handle plant spread:
			- increased chance of spreading if not growing (migrate due to competition)
			- new biota in new tile starts with a small population
		- handle animal migration:
			- increased chance of spreading if starving population (migrate to find better conditions)
			- only migrate to tiles that have food that animal can eat
			- prefer to migrate to tiles that do not have the current biota type
			- size of migration is proportional to the size of the population that is not getting needs met

Equations:
	- Plant reproduction rate (R)
		Rb = base reproduction rate (per biota type)
		Tc = tile compatibility (percent how compatible this tile is with this biota type)
		R = Rb * Tc
	- Plant birth for biota (B_plant)
		S = size of biota
		C = plant capacity at tile
		Cs = capacity share (percent) for biota
		Cf = capacity free
		R = reproduction rate (percent)

		B_plant = Cs * Cf * R
	

Example:
	5000 plant capacity on tile
	Turn 1:
		1000 grass (R = 1.0)
		500 vegetables (R = 1.0)
		
		1500 capacity used
		3500 capacity free

		Growth:
			- Grass	
				- 20% share
				- 20% of 3500 = 500 births
				- 1500 grass
			- Vegetables
				- 10% share
				- 10% of 3500 = 350 births
	Turn 2:
		1500 grass
		850 vegetables

		2350 capacity used
		2650 capacity free

		Growth:
			- Grass
				- 63% share
				- 63% of 2650 = 1670 births
			- Vegetables
				- 36% share
				- 35% of 2650 = 964 births



*/


public class BiotaDaySystem : ISystem {
	public void Run(Commands commands) {
		var watch = System.Diagnostics.Stopwatch.StartNew();

		var biotaService = commands.GetElement<BiotaService>();
		var tilesChanged = new HashSet<Entity>();
		commands.Receive((BiotaAdded biotaAdded) => {
			// GD.PrintS("Add biota", biotaAdded.biota.Get<BiotaData>().biotaType);
			biotaService.AddBiota(biotaAdded.biota, biotaAdded.tile);
			tilesChanged.Add(biotaAdded.tile);
		});

		foreach (var tile in tilesChanged) {
			biotaService.UpdateTileBiota(tile);
		}

		var tiles = commands.Query<Entity>().Has<TileData>();
		foreach (var tile in tiles) {
			biotaService.CalculateTile(tile);
			biotaService.DebugTile(tile);
		}

		GD.PrintS($"(BiotaDaySystem) executed in {watch.ElapsedMilliseconds}ms");
	}
}